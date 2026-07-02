using Vehicles;
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
        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var item in base.GetGizmos())
            {
                yield return item;
            }
        }
        //FloatMenu選項已改由 FloatMenuOptionProvider_Vehicle 體系提供(見 Cargo/FloatMenuOptionProviders.cs)，
        //GetExtraFloatMenuOptionsFor(IntVec3) 在目前的 FloatMenuMakerMap 已不會被引擎呼叫，故移除。
    }
}
