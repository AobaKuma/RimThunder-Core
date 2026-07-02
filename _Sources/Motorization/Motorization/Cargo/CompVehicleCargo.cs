using Vehicles;
using Verse;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RimWorld;
using SmashTools;

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

    /// <summary>
    /// 掛在母車(或帶貨艙的拖曳車/拖車)身上，讓它能裝進另一台載具當貨物運送的元件。
    /// </summary>
    public class CompVehicleCargo : CompVehicleAttachment
    {
        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
        }
        public CompProperties_VehicleCargo Props => base.props as CompProperties_VehicleCargo;

        protected override bool RenderOpposite => Props.renderOpposite;

        protected override bool ShouldRenderChild => Props.renderVehicle;

        public bool TryUnloadThing(VehiclePawn thing)
        {
            if (Cargo.Contains(thing))
            {
                Cargo.Remove(thing);
                GenDrop.TryDropSpawn(thing, Vehicle.Position + this.Vehicle.FullRotation.FacingCell * (int)((parent.def.Size.z + thing.def.Size.z) / 2), parent.Map, ThingPlaceMode.Near, out var _);
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
            //選中的是想被裝進去的載具，Vehicle(this.parent)才是主動要裝載的母車。
            return FloatMenuUtility.TryMakeFloatMenuForCargoLoad(Vehicle, vehicle);
        }

        public float MassCapacity => Vehicle.GetStatValue(VehicleStatDefOf.CargoCapacity);

        public override AcceptanceReport Accepts(Thing thing)
        {
            if (thing == null) return false;
            if (thing is VehiclePawn_Tractor tractor && tractor.HasTrailer) return new AcceptanceReport("RTC_TargetIsTowing".Translate());
            if (thing.def.Size.z > Props.lengthLimit) return new AcceptanceReport("RTC_SizeOutOfLimit".Translate(Props.lengthLimit));
            return true;//這邊之後判斷需要額外寫重量那些
        }

        protected override void OnBeforeDrawChild(Vector3 drawLoc, Rot8 parentRot)
        {
            if (DebugSettings.godMode)
            {
                CarrierUtility.DebugDraw(drawLoc + GetDrawOffset(parentRot));
            }
        }

        protected override Vector3 GetChildOffset(Rot8 parentRot, Rot8 childRot, VehiclePawn child)
        {
            return (CompDrawPos(parentRot) * Length(child)) + GetDrawOffset(parentRot);
        }

        private Vector3 CompDrawPos(Rot8 rot)
        {
            return rot.Opposite.AsVector2.ToVector3();
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
