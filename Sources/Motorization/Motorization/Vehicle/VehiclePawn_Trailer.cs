using SmashTools;
using System.Collections.Generic;
using UnityEngine;
using Vehicles;
using Verse;


namespace Motorization
{
    public class VehiclePawn_Trailer : VehiclePawn
    {
        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)//其他pawn選擇這個載具時顯示
        {
            if (selPawn is VehiclePawn_Tractor) //拖車拖掛的部分
            {
                Log.Message(selPawn);
            }
            return base.GetFloatMenuOptions(selPawn);
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

        public void DrawTrailer(Map map, Vector3 pos, float angleFloat, Rot8 rot8Direction)
        {
            DrawAt(pos, Rot8.FromAngle(angleFloat), 0);
        }
    }
}
