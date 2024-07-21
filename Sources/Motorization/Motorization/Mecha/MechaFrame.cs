using Vehicles;
using Verse;
using System.Collections.Generic;
using RimWorld;
using Verse.AI;
using UnityEngine;


namespace Motorization
{
    public class VehiclePawnTrailer : VehiclePawn
    {
    }
    public class MechaFrame : VehiclePawn, IWeaponUsable
    {
        public void Equip(ThingWithComps equipment)
        {
            
            equipment.SetForbidden(false);
            jobs.TryTakeOrderedJob(JobMaker.MakeJob(JobDefOf.Equip, equipment), JobTag.Misc);
        }

        public void Wear(ThingWithComps apparel)
        {
            apparel.SetForbidden(false);
            this.jobs.TryTakeOrderedJob(JobMaker.MakeJob(JobDefOf.Wear, apparel), JobTag.Misc);
            this.equipment.AddEquipment(apparel);
        }
        public override void DynamicDrawPhaseAt(DrawPhase phase, Vector3 drawLoc, bool flip = false)
        {
            base.DynamicDrawPhaseAt(phase, drawLoc, flip);
        }
        public IEnumerable<Gizmo> EquipmentGizmos()
        {
            if (equipment.Primary != null)
            {
                foreach (Gizmo item in equipment.PrimaryEq.CompGetEquippedGizmosExtra())
                {
                    yield return item;
                }
            }
        }
        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var item in base.GetGizmos())
            {
                yield return item;
            }
            foreach (var item in EquipmentGizmos())
            {
                yield return item;
            }
        }
        public override IEnumerable<FloatMenuOption> GetExtraFloatMenuOptionsFor(IntVec3 sq)
        {
            foreach (var item in base.GetExtraFloatMenuOptionsFor(sq))
            {
                yield return item;
            }
            foreach (var item in FloatMenuUtility.GetExtraFloatMenuOptionsFor(this,sq))
            {
                yield return item;
            }
            foreach (var item in FloatMenuUtility.GetExtraFloatMenuOptionsFor(this, sq, this.def.GetModExtension<MechaSupportedWeaponExtension>()))
            {
                yield return item;
            }
        }
    }
}
