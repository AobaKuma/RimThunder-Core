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


        public static IEnumerable<FloatMenuOption> GetExtraFloatMenuOptionsForTractor(Pawn tractor, IntVec3 sq)
        {
            if (tractor == null)
            {
                Log.Error("Error");
                yield break;
            }
            if (tractor.Map == null)
            {
                Log.Error("Error");
                yield break;
            }
            List<Thing> things = sq.GetThingList(tractor.Map);

            for (int i = 0; i < things.Count; i++)
            {
                if (things[i] is VehiclePawn_Trailer tmp && tmp != tractor)
                {
                    if (tractor is VehiclePawn_Tractor tractorPawn && tractorPawn.HasTrailer)
                    {
                        yield return new FloatMenuOption("RTC_CanNotLoad".Translate() + ": " + "RTC_AlreadyTowing".Translate(), null);
                    }
                    else if (!CanTowVehicle(tractor as VehiclePawn_Tractor, tmp))
                    {
                        yield return new FloatMenuOption("RTC_CanNotLoad".Translate() + ": " + "RTC_NoSupportedType".Translate(), null);
                    }
                    else if (false)
                    {
                        yield return new FloatMenuOption("RTC_CanNotLoad".Translate() + ": " + "RTC_NoEnoughPower".Translate(), null);
                    }
                    else
                    {
                        yield return TryMakeFloatMenuForTrailerLoad(tractor, tmp);
                    }
                }
            }
        }
        public static bool CanTowVehicle(VehiclePawn_Tractor tractor, VehiclePawn_Trailer trailer)
        {
            if (!tractor.TryGetComp<CompTrailerMount>(out var tractorMount) & !trailer.TryGetComp<CompTrailerMount>(out var trailerMount)) return false;
            if(tractorMount.Props.supportedType.NullOrEmpty()|| trailerMount.Props.supportedType.NullOrEmpty()) return false;

            foreach (string item in tractorMount.Props.supportedType)
            {
                if(trailerMount.Props.supportedType.Contains(item)) return true;
            }
            return false;
        }
        public static FloatMenuOption TryMakeFloatMenuForTrailerLoad(Pawn pawn, Pawn targetPawn)//targetPawn是被裝的
        {
            if (pawn == null) Log.Error("pawn is null");
            if (targetPawn == null) Log.Error("targetPawn is null");

            string key = string.Format("Load {0} into {1}'s cargo", targetPawn.Label, pawn.Label);
            if (!pawn.CanReach(targetPawn, PathEndMode.ClosestTouch, Danger.Deadly, false, false, TraverseMode.ByPawn))
            {
                return new FloatMenuOption("CanNotReach" + ": " + "NoPath".Translate().CapitalizeFirst(), null);
            }
            else
            {
                return new FloatMenuOption(key.Translate(), () =>
                {
                    Job j = JobMaker.MakeJob(VehicleJobDefOf.RTC_ConnectToTrailer, targetPawn);
                    pawn.jobs.TryTakeOrderedJob(j);
                });
            }
        }
    }
}
