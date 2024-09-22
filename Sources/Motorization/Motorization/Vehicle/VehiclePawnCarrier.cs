using Vehicles;
using Verse;
using System.Collections.Generic;
using RimWorld;
using Verse.AI;
using UnityEngine;


namespace Motorization
{
    public class VehiclePawnCarrier : VehiclePawn
    {
        public override void DynamicDrawPhaseAt(DrawPhase phase, Vector3 drawLoc, bool flip = false)
        {
            base.DynamicDrawPhaseAt(phase, drawLoc, flip);
        }
        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var item in base.GetGizmos())
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
        }
    }
}
