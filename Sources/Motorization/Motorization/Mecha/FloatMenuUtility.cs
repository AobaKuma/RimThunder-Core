using RimWorld;
using Verse;
using System.Collections.Generic;
using Verse.AI;
using Vehicles;


namespace Motorization
{
    public static class FloatMenuUtility
    {
        public static IEnumerable<FloatMenuOption> GetExtraFloatMenuOptionsFor(Pawn pawn, IntVec3 sq)
        {
            if (pawn == null)
            {
                Log.Error("Error");
                yield break;
            }
            if (pawn.Map == null)
            {
                Log.Error("Error");
                yield break;
            }
            List<Thing> things = sq.GetThingList(pawn.Map);

            for (int i = 0; i < things.Count; i++)
            {
                if (things[i] is VehiclePawn tmp && tmp != pawn)
                {
                    yield return TryMakeFloatMenuForCargoLoad(pawn, tmp);
                    break;
                }
            }
        }
        public static FloatMenuOption TryMakeFloatMenuForCargoLoad(Pawn pawn, Pawn targetPawn)//pawn是被裝的
        {
            if (pawn == null) Log.Error("pawn is null");
            if (targetPawn == null) Log.Error("targetPawn is null");
            string key = string.Format("Load {1} into {0}'s cargo", targetPawn.Name, pawn.Name);
            if (!targetPawn.CanReach(pawn, PathEndMode.ClosestTouch, Danger.Deadly, false, false, TraverseMode.ByPawn))
            {
                return new FloatMenuOption("CanNotReach" + ": " + "NoPath".Translate().CapitalizeFirst(), null);
            }
            else
            {
                return new FloatMenuOption(key.Translate(), () =>
                {
                    Job j = JobMaker.MakeJob(VehicleJobDefOf.RTC_LoadToCargo, pawn);
                    targetPawn.jobs.TryTakeOrderedJob(j);
                    //targetPawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(VehicleJobDefOf.RTC_LoadToCargo, pawn), JobTag.Misc);
                });
            }
        }
    }
}
