using Vehicles;
using Verse;
using System.Collections.Generic;
using RimWorld;
using System.Linq;
using UnityEngine;


namespace Motorization
{
    [DefOf]
    public static class VehicleJobDefOf
    {
        public static JobDef RTC_LoadToCargo;
    }
    public class CompProperties_VehicleCargo : CompProperties
    {
        public CompProperties_VehicleCargo()
        {
            compClass = typeof(CompVehicleCargo);
        }
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
                yield return new FloatMenuOption("Test:" + pawn.Name, null);
            }


            if (!(pawn is VehiclePawn)) yield break;//判斷是不是VehiclePawn


            //判斷是否為自家載具跟自家殖民者
            yield return FloatMenuUtility.TryMakeFloatMenuForCargoLoad(pawn, Vehicle);
        }
        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {
            foreach (var item in base.CompFloatMenuOptions(selPawn))
            {
                yield return item;
            }
            yield return new FloatMenuOption("Test2:" + selPawn.Name, null);
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
            if (Vehicle.Spawned)
            {
                if (Cargo.Where(v => v is VehiclePawn).FirstOrDefault() != null)
                {
                    VehiclePawn p = (Cargo.Where(v => v is VehiclePawn).FirstOrDefault() as VehiclePawn);

                    //p.Drawer.renderer.RenderPawnAt(Vehicle.DrawPos + (CompDrawPos() * AllLength(p)), Vehicle.FullRotation.AsAngle, Vehicle.FullRotation, true);
                    p.FullRotation = new SmashTools.Rot8(Vehicle.FullRotation.Opposite.AsInt);//這條等到時候更新要刪掉換成別的。             
                    p.DrawAt(Vehicle.DrawPos + (CompDrawPos() * AllLength(p)), Vehicle.FullRotation.Opposite,0);
                }
            }
        }
        private Vector3 CompDrawPos()
        {
            return  Vehicle.FullRotation.Opposite.AsVector2.ToVector3();
        }
        private float AllLength(VehiclePawn pawn)
        {
            return GapLength(pawn) + GapLength(Vehicle)+1.5f;
        }
        private float GapLength(VehiclePawn pawn)
        {
            return pawn.def.Size.z / 2;
        }
    }
}
