﻿using Vehicles;
using Verse;
using System.Collections.Generic;
using RimWorld;
using Verse.AI;
using UnityEngine;
using SmashTools;


namespace Motorization
{
    public class VehiclePawn_Tractor : VehiclePawn //負責拖別人的母車。
    {
        public bool HasTrailer => TrailerMount.TryGetTrailer(out var _);


        protected CompTrailerMount trailerMount;
        public CompTrailerMount TrailerMount 
        {
            get 
            {
                if (trailerMount == null) trailerMount = this.TryGetComp<CompTrailerMount>();
                return trailerMount; 
            } 
            private set{ } 
        }

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
            foreach (var item in FloatMenuUtility.GetExtraFloatMenuOptionsForTractor(this,sq))
            {
                yield return item;
            }
            if (this.TryGetComp<CompVehicleCargo>(out CompVehicleCargo vehicleCargo))
            {
                foreach (var item in FloatMenuUtility.GetExtraFloatMenuOptionsForCarrier(this, sq))
                {
                    yield return item;
                }
            }
        }
    }
}
