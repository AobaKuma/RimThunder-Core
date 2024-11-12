using Vehicles;
using Verse;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SmashTools;
using RimWorld;

namespace Motorization
{
    public class CompProperties_VehicleCargo : CompProperties
    {
        public bool renderVehicle = true;
        public VehicleDrawData drawData = new VehicleDrawData();
        public Vector3 GetOffsetFor(Rot8 rot)
        {
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
        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
        }
        public ThingOwner<Thing> Cargo => Vehicle.inventory.innerContainer;

        public CompProperties_VehicleCargo Props => base.props as CompProperties_VehicleCargo;

        public FloatMenuOption FloatMenuOptionForCargo(Pawn selPawn)
        {
            if (selPawn == null ||selPawn.Faction !=Faction.OfPlayer || !(selPawn is VehiclePawn))
            {
                return new FloatMenuOption("RTC_CanNotLoad".Translate() + ": " + "RTC_NotAvaliable".Translate(), null);
            }

            VehiclePawn vehicle = selPawn as VehiclePawn;
            return FloatMenuUtility.TryMakeFloatMenuForCargoLoad(vehicle, Vehicle);
        }
        public bool TryUnloadThing(VehiclePawn thing)
        {
            if (Cargo.Contains(thing))
            {
                Cargo.Remove(thing);
                GenDrop.TryDropSpawn(thing, this.parent.Rotation.FacingCell * (int)(parent.def.Size.x / 2), parent.Map, ThingPlaceMode.Near, out var _);
                return true;
            }
            return false;
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
        public float MassCapacity => Vehicle.GetStatValue(VehicleStatDefOf.CargoCapacity);
        public bool Accepts(Thing thing)
        {
            if (thing == null) return false;
            return true;//這邊之後判斷需要額外寫重量那些
        }
        public override void PostDraw()
        {
            if (Vehicle.Spawned && Props.renderVehicle)
            {
                if (Cargo.Where(v => v is VehiclePawn && !(v is VehiclePawn_Trailer)).FirstOrDefault() != null)
                {
                    VehiclePawn p = (Cargo.Where(v => v is VehiclePawn).FirstOrDefault() as VehiclePawn);
                    p.FullRotation = Vehicle.Rotation.Opposite;
                    p.DrawAt(Vehicle.DrawPos + (CompDrawPos() * AllLength(p)) + GetDrawOffset(), Vehicle.FullRotation.Opposite, GetAngle(), compDraw: true);
                }
            }
        }
        private Vector3 CompDrawPos()
        {
            return Vehicle.FullRotation.Opposite.AsVector2.ToVector3();
        }
        private float GetAngle()
        {
            switch (Vehicle.FullRotation.AsInt)
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
        private Vector3 GetDrawOffset()
        {
            return Props.GetOffsetFor(Vehicle.FullRotation);
        }
        private float AllLength(VehiclePawn pawn)
        {
            return GapLength(pawn) + GapLength(Vehicle) + 1.5f;
        }
        private float GapLength(VehiclePawn pawn)
        {
            return pawn.def.Size.z / 2;
        }
    }
}
