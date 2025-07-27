using Vehicles;
using Verse;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SmashTools;
using RimWorld;
using static UnityEngine.GraphicsBuffer;

namespace Motorization
{
    public class CompProperties_VehicleCargo : CompProperties
    {
        public bool renderVehicle = true;
        public int lengthLimit = 5;
        public VehicleDrawData drawData = null;

        public bool renderOpposite = false;
        public Vector3 GetOffsetFor(Rot8 rot)
        {
            if (drawData == null) return Vector3.zero;
            switch (rot.AsInt)
            {
                case 0:
                    return drawData.dataNorth;
                case 1:
                    return drawData.dataEast;
                case 2:
                    return drawData.dataSouth;
                case 3:
                    return drawData.dataWest;
                case 4://NorthEastInt
                    return drawData.dataNorthEast;
                case 5://SouthEastInt
                    return drawData.dataSouthEast;
                case 6://SouthWestInt
                    return drawData.dataSouthWest;
                case 7://NorthWestInt
                    return drawData.dataNorthWest;
            }
            return Vector3.zero;
        }
        public CompProperties_VehicleCargo()
        {
            compClass = typeof(CompVehicleCargo);
        }
    }
    public class CompVehicleCargo : VehicleComp
    {
        private bool initCheck = false;
        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
        }
        public ThingOwner<Thing> Cargo => Vehicle.inventory?.innerContainer;
        public CompProperties_VehicleCargo Props => base.props as CompProperties_VehicleCargo;
        public bool TryUnloadThing(VehiclePawn thing)
        {
            if (Cargo.Contains(thing))
            {
                Cargo.Remove(thing);
                GenDrop.TryDropSpawn(thing, this.Vehicle.FullRotation.FacingCell * (int)((parent.def.Size.z + thing.def.Size.z) / 2), parent.Map, ThingPlaceMode.Near, out var _);
                thing.FullRotation = Props.renderOpposite ? Vehicle.FullRotation.Opposite : Vehicle.FullRotation;
                return true;
            }
            return false;
        }
        public bool TryUnloadThingUnspawned(VehiclePawn Parent, VehiclePawn thing)
        {
            if (Cargo.Contains(thing))
            {
                Cargo.Remove(thing);
                GenDrop.TryDropSpawn(thing, Parent.Position + this.Vehicle.FullRotation.FacingCell * (int)((parent.def.Size.z + thing.def.Size.z) / 2), Parent.Map, ThingPlaceMode.Near, out var _);
                thing.FullRotation = Props.renderOpposite ? Parent.FullRotation.Opposite : Parent.FullRotation;
                return true;
            }
            return false;
        }
        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {
            foreach (var item in base.CompFloatMenuOptions(selPawn))
            {
                yield return item;
            }
            yield return FloatMenuOptionForCargo(selPawn);
        }
        public FloatMenuOption FloatMenuOptionForCargo(Pawn selPawn)
        {
            if (selPawn == null || selPawn.Faction != Faction.OfPlayer || !(selPawn is VehiclePawn))
            {
                return new FloatMenuOption("RTC_CanNotLoad".Translate() + ": " + "RTC_NotAvaliable".Translate(), null);
            }

            VehiclePawn vehicle = selPawn as VehiclePawn;
            return FloatMenuUtility.TryMakeFloatMenuForCargoLoad(vehicle, Vehicle);
        }
        public bool TryAcceptThing(VehiclePawn thing)
        {
            if (!this.Accepts(thing))
            {
                return false;
            }
            thing.DeSpawn();
            Vehicle.AddOrTransfer(thing);
            UpdateRendering();
            return true;
        }
        public float MassCapacity => Vehicle.GetStatValue(VehicleStatDefOf.CargoCapacity);

        public bool HasPayload
        {
            get => Cargo.HasData() && Cargo.ContainsAny(v => v is VehiclePawn || v.def.thingClass.IsSubclassOf(typeof(VehiclePawn)));
            internal set { }
        }
        public VehiclePawn GetPayload => Cargo.Where(v => v is VehiclePawn || v.def.thingClass.IsSubclassOf(typeof(VehiclePawn))).First() as VehiclePawn;

        public AcceptanceReport Accepts(Thing thing)
        {
            if (thing == null) return false;
            if (thing is VehiclePawn_Tractor tractor && tractor.HasTrailer) return new AcceptanceReport("RTC_TargetIsTowing".Translate());
            if (thing.def.Size.z > Props.lengthLimit) return new AcceptanceReport("RTC_SizeOutOfLimit".Translate(Props.lengthLimit));
            return true;//這邊之後判斷需要額外寫重量那些
        }
        public void UpdateRendering()
        {
            if (Vehicle.Spawned && Props.renderVehicle)
            {
                if (DebugSettings.godMode) CarrierUtility.DebugDraw(Vehicle.DrawPos + GetDrawOffset(Vehicle.FullRotation));

                if (Cargo.Where(v => v is VehiclePawn || v.def.thingClass.IsSubclassOf(typeof(VehiclePawn))).FirstOrDefault() != null)
                {
                    VehiclePawn p = (Cargo.Where(v => v is VehiclePawn || v.def.thingClass.IsSubclassOf(typeof(VehiclePawn))).FirstOrDefault() as VehiclePawn);
                    if (!initCheck)
                    {
                        initCheck = true;
                        p.ResetRenderStatus();
                        return;
                    }
                    //顯示的時候如果為逆向則會翻轉。
                    Rot8 rot = this.Props.renderOpposite ? Vehicle.FullRotation.Opposite : Vehicle.FullRotation;
                    p.Rotation = rot;
                    float trailerAngle = GetAngle(rot);

                    //砲塔
                    if (p.CompVehicleTurrets != null && p.CompVehicleTurrets.Turrets != null)
                    {
                        foreach (var item in p.CompVehicleTurrets.Turrets)
                        {
                            item.TurretRotation = item.defaultAngleRotated + rot.AsAngle;
                        }
                    }
                    Vehicle.DrawTracker.DynamicDrawPhaseAt(DrawPhase.Draw, Vehicle.DrawPos + (CompDrawPos(Vehicle.FullRotation) * Length(p)) + GetDrawOffset(Vehicle.FullRotation), rot, trailerAngle);
                    //p.DrawAt(Vehicle.DrawPos + (CompDrawPos(Vehicle.FullRotation) * Length(p)) + GetDrawOffset(Vehicle.FullRotation), rot, trailerAngle);
                }
            }
        }
        public void DrawUnspawned(Vector3 drawLoc, Rot8 rot8, float rotation)
        {
            if (!Props.renderVehicle) return;
            if (DebugSettings.godMode) CarrierUtility.DebugDraw(drawLoc + GetDrawOffset(rot8));

            if (Cargo != null && Cargo.Where(v => v is VehiclePawn || v.def.thingClass.IsSubclassOf(typeof(VehiclePawn))).FirstOrDefault() != null)
            {
                VehiclePawn p = (Cargo.Where(v => v is VehiclePawn || v.def.thingClass.IsSubclassOf(typeof(VehiclePawn))).FirstOrDefault() as VehiclePawn);
                if (!initCheck)
                {
                    initCheck = true;
                    p.ResetRenderStatus();
                    return;
                }
                //顯示的時候如果為逆向則會翻轉。
                Rot8 rot = this.Props.renderOpposite ? rot8.Opposite : rot8;

                //砲塔
                if (p.CompVehicleTurrets != null && p.CompVehicleTurrets.Turrets != null)
                {
                    foreach (var item in p.CompVehicleTurrets.Turrets)
                    {
                        item.TurretRotation = item.defaultAngleRotated + rot.AsAngle;
                    }
                }
                Vehicle.DrawTracker.DynamicDrawPhaseAt(DrawPhase.Draw, drawLoc + (CompDrawPos(rot8) * Length(p)) + GetDrawOffset(rot8), rot, GetAngle(rot8));
                //p.DrawAt(drawLoc + (CompDrawPos(rot8) * Length(p)) + GetDrawOffset(rot8), rot, GetAngle(rot));
            }
        }
        private Vector3 CompDrawPos(Rot8 rot)
        {
            return rot.Opposite.AsVector2.ToVector3();
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
        private Vector3 GetDrawOffset(Rot8 rot)
        {
            return Props.GetOffsetFor(rot);
        }
        private float Length(VehiclePawn pawn)
        {
            if (this.Props.drawData != null) return 0;
            return GapLength(pawn) + GapLength(Vehicle) + 1.5f;
        }
        private float GapLength(VehiclePawn pawn)
        {
            return pawn.def.Size.z / 2;
        }
    }
}
