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
        public override IEnumerable<FloatMenuOption> GetExtraFloatMenuOptionsFor(IntVec3 sq)
        {
            foreach (var item in base.GetExtraFloatMenuOptionsFor(sq))
            {
                yield return item;
            }
            if (this.HasCargoComp)
            {
                foreach (var item in FloatMenuUtility.GetExtraFloatMenuOptionsForCarrier(this, sq))
                {
                    yield return item;
                }
            }
        }
        public CompTrailerMount TrailerMount
        {
            get
            {
                if (cacheComp != null)
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