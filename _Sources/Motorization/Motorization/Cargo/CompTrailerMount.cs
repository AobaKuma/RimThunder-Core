using Vehicles;
using Verse;
using System.Collections.Generic;
using SmashTools;
using UnityEngine;
using RimWorld;
using System.Linq;


namespace Motorization
{
    public class CompProperties_TrailerMount : CompProperties
    {
        public List<string> supportedType = new List<string>() { "lowTrailerMount" };

        /// <summary>
        /// 掛點位置表。改用具名欄位(而非按Rot8.AsInt順序排列的List)，
        /// 避免XML作者依照"直覺的"順時鐘羅盤順序(N,NE,E,SE,S,SW,W,NW)填寫，
        /// 但Rot8.AsInt實際上是先四個正向(N,E,S,W=0..3)再四個對角線(NE,SE,SW,NW=4..7)，
        /// 兩者順序不同，用index直接對應會讓對角線掛點全部對錯。
        /// </summary>
        public Vector3 pivotNorth = Vector3.zero;
        public Vector3 pivotEast = Vector3.zero;
        public Vector3 pivotSouth = Vector3.zero;
        public Vector3 pivotWest = Vector3.zero;
        public Vector3 pivotNorthEast = Vector3.zero;
        public Vector3 pivotSouthEast = Vector3.zero;
        public Vector3 pivotSouthWest = Vector3.zero;
        public Vector3 pivotNorthWest = Vector3.zero;

        /// <summary>
        /// 舊版相容欄位：若XML仍使用舊的按Rot8.AsInt順序排列的<rotationPivot>清單(8項)，
        /// 會在PostLoad時自動轉換成上面的具名欄位並記一次警告，避免既有Defs直接失效。
        /// </summary>
        public List<Vector3> rotationPivot;

        public bool flipWhenTrailer = false;

        public CompProperties_TrailerMount()
        {
            compClass = typeof(CompTrailerMount);
        }
        public override void PostLoadSpecial(ThingDef parentDef)
        {
            base.PostLoadSpecial(parentDef);
            if (rotationPivot != null)
            {
                if (rotationPivot.Count == 8)
                {
                    Log.Warning(parentDef.defName + " uses legacy <rotationPivot> list, converting to named pivot fields. Please update the XML to use pivotNorth/pivotEast/pivotSouth/pivotWest/pivotNorthEast/pivotSouthEast/pivotSouthWest/pivotNorthWest instead (ordered by Rot8.AsInt: N,E,S,W,NE,SE,SW,NW).");
                    pivotNorth = rotationPivot[Rot8.NorthInt];
                    pivotEast = rotationPivot[Rot8.EastInt];
                    pivotSouth = rotationPivot[Rot8.SouthInt];
                    pivotWest = rotationPivot[Rot8.WestInt];
                    pivotNorthEast = rotationPivot[Rot8.NorthEastInt];
                    pivotSouthEast = rotationPivot[Rot8.SouthEastInt];
                    pivotSouthWest = rotationPivot[Rot8.SouthWestInt];
                    pivotNorthWest = rotationPivot[Rot8.NorthWestInt];
                }
                else
                {
                    Log.Error(parentDef.defName + " has <rotationPivot> with " + rotationPivot.Count + " entries, expected exactly 8 (N,E,S,W,NE,SE,SW,NW order). Ignoring legacy list.");
                }
                rotationPivot = null;
            }
        }
        public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
        {
            if (!this.supportedType.HasData())
            {
                if (this.supportedType == null) supportedType = new List<string>();
                Log.Error(parentDef.defName + " has empty supportType of trailers. it will make it not able to mount on any trailer or tractors");
            }
            return base.ConfigErrors(parentDef);
        }

        public Vector3 GetPivotFor(Rot8 rot)
        {
            switch (rot.AsInt)
            {
                case Rot8.NorthInt:
                    return pivotNorth;
                case Rot8.EastInt:
                    return pivotEast;
                case Rot8.SouthInt:
                    return pivotSouth;
                case Rot8.WestInt:
                    return pivotWest;
                case Rot8.NorthEastInt:
                    return pivotNorthEast;
                case Rot8.SouthEastInt:
                    return pivotSouthEast;
                case Rot8.SouthWestInt:
                    return pivotSouthWest;
                case Rot8.NorthWestInt:
                    return pivotNorthWest;
                default:
                    return Vector3.zero;
            }
        }
    }

    /// <summary>
    /// 掛在母車(拖曳車)跟子車(拖車)身上的元件。當附掛在拖曳車上時，扮演「容器」角色(繼承CompVehicleAttachment)，
    /// 負責掛載/解除掛載/每幀畫出拖車；當附掛在拖車自己身上時，只提供GetPivot/Props等資料給拖曳車那一側查詢。
    /// </summary>
    public class CompTrailerMount : CompVehicleAttachment
    {
        public Rot8 LatestRot => latestRot;
        protected VehiclePawn Pawn => parent as VehiclePawn;
        protected Rot8 latestRot;
        protected int UpdateInterval = 360;
        protected float tempInterval = 0;
        public CompProperties_TrailerMount Props => base.props as CompProperties_TrailerMount;

        public bool TryGetTrailer(out VehiclePawn_Trailer pawn)
        {
            pawn = null;
            if (Cargo.HasData() && Cargo.Where(a => a is VehiclePawn_Trailer).FirstOrDefault() is VehiclePawn_Trailer trailer)
            {
                pawn = trailer;
                return true;
            }
            return false;
        }

        public override AcceptanceReport Accepts(Thing thing)
        {
            if (thing == null) return false;
            else if (!(thing is VehiclePawn_Trailer)) return new AcceptanceReport("RTC_TargetIsNotTrailer".Translate());
            if (TryGetTrailer(out var _)) return new AcceptanceReport("RTC_AlreadyTowing".Translate());
            if (thing is VehiclePawn_Trailer trailer && !CanTowVehicle(Vehicle as VehiclePawn_Tractor, trailer)) return new AcceptanceReport("RTC_NotSupportType".Translate());
            return true;//這邊之後判斷需要額外寫重量那些
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            Initial(Vehicle.FullRotation);
        }

        public bool TryDisconnectTrailer(VehiclePawn dropTarget)//作為拖車被母車Unload
        {
            if (!(Vehicle is VehiclePawn_Tractor) || !Vehicle.Spawned) return false;

            if (this.Cargo.Contains(dropTarget))
            {
                Cargo.Remove(dropTarget);
                IntVec3 tractorMount = this.GetPivot(Vehicle.FullRotation).ToIntVec3();
                IntVec3 trailerMount = dropTarget.TryGetComp<CompTrailerMount>().GetPivot(Vehicle.FullRotation).ToIntVec3();
                GenDrop.TryDropSpawn(dropTarget, Vehicle.Position + tractorMount - trailerMount, Vehicle.Map, ThingPlaceMode.Near, out var _);
                dropTarget.FullRotation = Props.flipWhenTrailer ? Vehicle.FullRotation.Opposite : Vehicle.FullRotation;
                return true;
            }
            return false;
        }

        protected override void OnBeforeDrawChild(Vector3 drawLoc, Rot8 parentRot)
        {
            if (DebugSettings.godMode)
            {
                CarrierUtility.DebugDraw(drawLoc + GetPivot(parentRot));
            }
        }

        public override void CompTick()
        {
            base.CompTick();
            TickRotationCatchup();
        }

        /// <summary>
        /// 拖車轉向延遲(甩尾感)的追趕邏輯。必須掛在CompTick(每個遊戲tick固定執行一次)，
        /// 不能放在PostDraw/GetChildRotation裡——那些是每「渲染影格」執行一次，
        /// 而渲染影格數跟遊戲tick數並不是1:1(受畫面幀率、遊戲時間倍速影響)，
        /// 用畫面影格去累加MoveSpeed(每tick位移量)單位對不上，
        /// 會導致實際倍速越快、追趕越跟不上，甩尾角度長時間卡住不動(如影片所示)。
        /// </summary>
        private void TickRotationCatchup()
        {
            if (Find.TickManager.CurTimeSpeed == TimeSpeed.Paused) return;
            if (Pawn.movementStatus != VehicleMovementStatus.Online || !Pawn.vehiclePather.Moving) return;

            tempInterval += Vehicle.statHandler.GetStatValue(VehicleStatDefOf.MoveSpeed);
            if (tempInterval > UpdateInterval)
            {
                UpdateRotate();
                tempInterval = 0;
            }
        }

        protected override Rot8 GetChildRotation(Rot8 parentRot)
        {
            if (parentRot.Opposite == latestRot)
            {
                latestRot = parentRot;
            }
            //顯示的時候如果為逆向則會翻轉。
            return Props.flipWhenTrailer ? LatestRot.Opposite : LatestRot;
        }

        protected override Vector3 GetChildOffset(Rot8 parentRot, Rot8 childRot, VehiclePawn child)
        {
            //母車掛點位置
            Vector3 tractorMount = GetPivot(parentRot);
            if (child.TryGetComp<CompTrailerMount>(out CompTrailerMount childMount))
            {
                //子車掛點位置
                Vector3 trailerMount = childMount.GetPivot(childRot);
                //母車位置+母車(自身轉向時的)旋轉偏移+子車(自身轉向時的)旋轉偏移
                return tractorMount - trailerMount;
            }
            return tractorMount;
        }

        public static bool CanTowVehicle(VehiclePawn_Tractor tractor, VehiclePawn target)
        {
            if (!tractor.TryGetComp<CompTrailerMount>(out var tractorMount) || !target.TryGetComp<CompTrailerMount>(out var trailerMount)) return false;
            if (!tractorMount.Props.supportedType.HasData() || !trailerMount.Props.supportedType.HasData()) return false;

            foreach (string item in tractorMount.Props.supportedType)
            {
                if (trailerMount.Props.supportedType.Contains(item)) return true;
            }
            return false;
        }

        public void Initial(Rot8 rot)
        {
            latestRot = rot;
        }

        protected void UpdateRotate()
        {
            if (latestRot == Vehicle.FullRotation) return;

            //如果大於4代表應該是順時鐘旋轉
            //4 - 3 = 1
            float ra = Vehicle.FullRotation.AsAngle - latestRot.AsAngle;
            if (ra < 0) ra += 360;
            RotationDirection direction = ra > 180 ? RotationDirection.Counterclockwise : RotationDirection.Clockwise;
            latestRot.Rotate(direction, true);
        }

        public Vector3 GetPivot(Rot8 rot)
        {
            return Props.GetPivotFor(rot);
        }

        public override void PostExposeData()
        {
            Scribe_Values.Look(ref latestRot, "latestRot", defaultValue: Rot8.Invalid);
            Scribe_Values.Look(ref tempInterval, "tempInterval", defaultValue: UpdateInterval);
        }
    }
}
