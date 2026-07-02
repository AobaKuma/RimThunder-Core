using RimWorld;
using SmashTools;
using System.Collections.Generic;
using UnityEngine;
using Vehicles;
using Verse;


namespace Motorization
{
    public class VehiclePawn_Trailer : VehiclePawn
    {
        public bool HasCargoComp => CargoComp != null;
        public CompVehicleCargo CargoComp => GetComp<CompVehicleCargo>();

        private bool initCheck = false;
        //FloatMenu選項已改由 FloatMenuOptionProvider_Vehicle 體系提供(見 Cargo/FloatMenuOptionProviders.cs)，
        //GetExtraFloatMenuOptionsFor(IntVec3) 在目前的 FloatMenuMakerMap 已不會被引擎呼叫，故移除。
        public CompTrailerMount TrailerMount
        {
            get
            {
                if (cacheComp == null)
                {
                    cacheComp = this.GetComp<CompTrailerMount>();
                    if (cacheComp == null) Log.Error(string.Format("error, {0} doesn't have CompTrailerMount", this.def.defName));
                }
                return cacheComp;
            }
            private set { }
        }

        private CompTrailerMount cacheComp;

        public override void DrawAt(in Vector3 drawLoc, Rot8 rot, float rotation)
        {
            if (!initCheck)
            {
                initCheck = true;
                this.ResetRenderStatus(); //媽的你不要動他。
                return;
            }
            base.DrawAt(drawLoc, rot, rotation);

            if (HasCargoComp)
            {
                CargoComp.DrawUnspawned(drawLoc, rot, rotation);
            }
        }
    }
}