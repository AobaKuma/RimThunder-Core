using Vehicles;
using Verse;
using System.Collections.Generic;
using SmashTools;
using UnityEngine;
using RimWorld;
using System;
using System.Linq;


namespace Motorization
{
    public class CompProperties_TrailerMount : CompProperties
    {
        public List<string> supportedType = new List<string>() { "lowTrailerMount"};
        public List<Vector3> rotationPivot;
        public bool flipWhenTrailer = false;


        public CompProperties_TrailerMount()
        {
            compClass = typeof(CompTrailerMount);
        }
        public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
        {
            if (this.supportedType.NullOrEmpty())
            {
                if(this.supportedType ==null) supportedType = new List<string>();
                Log.Error(parentDef.defName + " has empty supportType of trailers. it will make it not able to mount on any trailer or tractors");
            }
            return base.ConfigErrors(parentDef);
            
        }
    }

    public class CompTrailerMount : VehicleComp//掛在母車跟子車上的
    {
        public Rot8 LatestRot => latestRot;
        protected VehiclePawn Pawn => parent as VehiclePawn;
        protected Rot8 latestRot;
        protected int UpdateInterval = 360;
        protected float tempInterval = 0;
        public CompProperties_TrailerMount Props => base.props as CompProperties_TrailerMount;
        public ThingOwner<Thing> Cargo => Vehicle.inventory.innerContainer;
        public bool TryGetTrailer(out VehiclePawn_Trailer pawn)
        {
            pawn = null;
            if (!Cargo.NullOrEmpty() && Cargo?.Where(a => a is VehiclePawn_Trailer).First() is VehiclePawn_Trailer trailer)
            {
                pawn = trailer;
                return true;
            }
            return false;
        }
        public bool Accepts(Thing thing)
        {
            if (thing == null) return false;
            if (thing is VehiclePawn_Tractor tractor && tractor.TrailerMount.TryGetTrailer(out var _)) return false;
            else if(!(thing is VehiclePawn_Trailer)) return false;
            return true;//這邊之後判斷需要額外寫重量那些
        }
        public bool TryAcceptThing(VehiclePawn thing)
        {
            if (!this.Accepts(thing))
            {
                return false;
            }
            thing.DeSpawn();
            Vehicle.AddOrTransfer(thing);
            return true;
        }
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            Initial(Vehicle.FullRotation);
        }
        public override void Notify_DefsHotReloaded()
        {
            base.Notify_DefsHotReloaded();
        }
        public bool TryUnloadThing(VehiclePawn thing)
        {
            if (Cargo.Contains(thing))
            {
                Cargo.Remove(thing);
                GenDrop.TryDropSpawn(thing, this.Vehicle.FullRotation.FacingCell * (int)((parent.def.Size.z + thing.def.Size.z) / 2), parent.Map, ThingPlaceMode.Near, out var _);
                thing.FullRotation = Props.flipWhenTrailer ? Vehicle.FullRotation.Opposite : Vehicle.FullRotation;
                return true;
            }
            return false;
        }
        public override void PostDraw()
        {
            base.PostDraw();
            if (parent.Spawned)
            {
                if (DebugSettings.godMode) DebugDraw(parent.DrawPos + GetPivot(Vehicle.FullRotation));
                if (parent is VehiclePawn_Tractor && TryGetTrailer(out var trailer) && parent.TryGetComp<CompTrailerMount>(out var mount))
                {
                    if (mount.LatestRot == null) mount.Initial(Vehicle.FullRotation);
                    mount.DrawTrailer(trailer, Vehicle.DrawPos, Vehicle.FullRotation); //由母車的Comp來叫，並在內部調用子車的Comp
                }
            }
        }
        public void DrawTrailer(VehiclePawn_Trailer pawn_Trailer, Vector3 exactPos, Rot8 rot)
        {
            //旋轉更新
            if (Find.TickManager.CurTimeSpeed != 0 && Pawn.movementStatus == VehicleMovementStatus.Online && Pawn.vehiclePather.Moving)
            {
                if (tempInterval > UpdateInterval)
                {
                    UpdateRotate();
                    tempInterval = 0;
                }
                tempInterval += Vehicle.statHandler.GetStatValue(VehicleStatDefOf.MoveSpeed);
            }
            if (rot.Opposite == latestRot)
            {
                latestRot = rot;
            }
            //母車掛點位置
            Vector3 tractorMount = GetPivot(Vehicle.FullRotation);
            if (pawn_Trailer.TryGetComp<CompTrailerMount>(out CompTrailerMount compTrailerMount))
            {
                //子車旋轉
                Rot8 trailerRot = compTrailerMount.Props.flipWhenTrailer ? LatestRot.Opposite : LatestRot;//顯示的時候如果為逆向則會翻轉。
                pawn_Trailer.Rotation = trailerRot;
                //子車掛點位置
                Vector3 trailerMount = compTrailerMount.GetPivot(trailerRot);

                float trailerAngle = GetAngle(trailerRot);

                //母車位置+母車(自身轉向時的)旋轉偏移 +子車(自身轉向時的)旋轉偏移

                //砲塔
                if (pawn_Trailer.CompVehicleTurrets != null && !pawn_Trailer.CompVehicleTurrets.turrets.NullOrEmpty())
                {
                    foreach (var item in pawn_Trailer.CompVehicleTurrets.turrets)
                    {
                        item.TurretRotation = item.defaultAngleRotated + trailerRot.AsAngle;
                    }
                }
                pawn_Trailer.DrawAt(exactPos + tractorMount - trailerMount, trailerRot, trailerAngle, compDraw: true);
            }
        }
        public void DebugDraw(Vector3 exactPos)
        {
            Mesh mesh;
            mesh = MeshPool.plane10;
            Matrix4x4 matrix = Matrix4x4.TRS(exactPos + Vector3.up, Quaternion.identity, Vector3.one * 2);
            Texture2D texture = ContentFinder<Texture2D>.Get("RT_Dummy");
            Material m = MaterialPool.MatFrom(texture);
            Graphics.DrawMesh(mesh, matrix, m, 0);
        }
        private float GetAngle(Rot8 rot)
        {
            switch (rot.AsInt)
            {
                case 4:
                    return 90;
                case 5:
                    return -90;
                case 6:
                    return 90;
                case 7:
                    return -90;
                default:
                    return 0;
            }
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
            latestRot.Rotate(direction,true);
        }
        public Vector3 GetPivot(Rot8 rot)
        {
            if (Props.rotationPivot != null)
            {
                return Props.rotationPivot[rot.AsInt];
            }
            return Vector3.zero;
        }
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref latestRot, "latestRot", defaultValue: Rot8.Invalid);
            Scribe_Values.Look(ref tempInterval, "tempInterval", defaultValue: UpdateInterval);
        }
    }
}
