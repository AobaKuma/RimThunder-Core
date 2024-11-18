using SmashTools;
using System.Collections.Generic;
using UnityEngine;
using Vehicles;
using Verse;


namespace Motorization
{
    public class VehiclePawn_Trailer : VehiclePawn
    {
        public override IEnumerable<FloatMenuOption> GetExtraFloatMenuOptionsFor(IntVec3 sq)
        {
            foreach (var item in base.GetExtraFloatMenuOptionsFor(sq))
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

        public override void DrawAt(Vector3 drawLoc, Rot8 rot, float extraRotation, bool flip = false, bool compDraw = true)
        {
            base.DrawAt(drawLoc, rot, extraRotation, flip, compDraw);
            if (compDraw)
            {
                if (this.TryGetComp<CompVehicleCargo>(out CompVehicleCargo vehicleCargo))
                {
                    vehicleCargo.PostDrawUnspawned(drawLoc, rot, extraRotation);
                }
            }
        }
        public override void ExposeData()
        {
            base.ExposeData();
            this.ResetRenderStatus();
        }
    }
}
