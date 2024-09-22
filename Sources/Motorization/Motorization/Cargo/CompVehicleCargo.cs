using Vehicles;
using Verse;
using System.Collections.Generic;
using RimWorld;
using System.Linq;
using UnityEngine;
using SmashTools;


namespace Motorization
{
    [DefOf]
    public static class VehicleJobDefOf
    {
        public static JobDef RTC_LoadToCargo;
    }
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
    [System.Serializable]
    public class VehicleDrawData
    {
        public Vector3
            dataEast = Vector3.zero,
            dataWest = Vector3.zero,
            dataSouth = Vector3.zero,
            dataNorth = Vector3.zero,
            dataNorthEast = Vector3.zero,
            dataSouthEast = Vector3.zero,
            dataSouthWest = Vector3.zero,
            dataNorthWest = Vector3.zero;
    }
    public class CompVehicleCargo : VehicleComp
    {
        ThingOwner<Thing> Cargo => Vehicle.inventory.innerContainer;
        public CompProperties_VehicleCargo Props => base.props as CompProperties_VehicleCargo;
        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions() //其他Pawn選擇這個Pawn時產生
        {
            foreach (var item in base.CompFloatMenuOptions())
            {
                yield return item;
            }

            VehiclePawn pawn = Find.Selector.SingleSelectedThing as VehiclePawn; //總之先這樣寫
            if (pawn != null)
            {
                yield return new FloatMenuOption("RT_Load:" + pawn.Name, null);
            }

            if (!(pawn is VehiclePawn)) yield break;//判斷是不是VehiclePawn

            //判斷是否為自家載具跟自家殖民者

            if (!pawn.Faction.IsPlayer) yield break;

            yield return FloatMenuUtility.TryMakeFloatMenuForCargoLoad(pawn, Vehicle);
        }
        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {
            foreach (var item in base.CompFloatMenuOptions(selPawn))
            {
                yield return item;
            }
            yield return new FloatMenuOption("RT_Load:" + selPawn.Name, null);
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
                if (Cargo.Where(v => v is VehiclePawn).FirstOrDefault() != null)
                {
                    VehiclePawn p = (Cargo.Where(v => v is VehiclePawn).FirstOrDefault() as VehiclePawn);

                    //p.Drawer.renderer.RenderPawnAt(Vehicle.DrawPos + (CompDrawPos() * AllLength(p)) + Props.drawData.OffsetForRot(Vehicle.Rotation), Vehicle.FullRotation.Opposite.AsAngle, true);

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
