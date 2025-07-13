using RimWorld;
using Verse;
using System.Collections.Generic;
using Verse.AI;
using Vehicles;
using System.Linq;
using System;
using static UnityEngine.GraphicsBuffer;


namespace Motorization
{
    public static class FloatMenuUtility
    {
        public static IEnumerable<FloatMenuOption> GetExtraFloatMenuOptionsForCarrier(Pawn pawn, IntVec3 sq)
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
            if (!pawn.CanReach(sq, PathEndMode.ClosestTouch, Danger.Deadly, false, false, TraverseMode.ByPawn))
            {
                yield return new FloatMenuOption("CanNotReach".Translate() + ": " + "NoPath".Translate().CapitalizeFirst(), null);
                yield break;
            }
            List<Thing> things = sq.GetThingList(pawn.Map);
            for (int i = 0; i < things.Count; i++)
            {
                if (things[i] is VehiclePawn tmp && tmp != pawn)
                {
                    yield return TryMakeFloatMenuForCargoLoad(pawn, tmp);
                }
                else
                {   //自行解除連接的選項。
                    if (pawn is VehiclePawn_Trailer trailer)
                    {
                        if (trailer.HasCargoComp && trailer.CargoComp.HasPayload)
                        {
                            VehiclePawn target = trailer.CargoComp.GetPayload;
                            yield return new FloatMenuOption("RTC_Unload".Translate(target.Label.Translate()), () =>
                            {
                                trailer.CargoComp.TryUnloadThingUnspawned(trailer, target);
                            });
                        }
                        else
                        {
                            yield return new FloatMenuOption("RTC_NoPayload".Translate(), null);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 這個是當拖車拖著帶Carrier的載具時調用。
        /// </summary>
        /// <param name="pawn"></param>
        /// <param name="targetPawn"></param>
        /// <returns></returns>
        public static IEnumerable<FloatMenuOption> TryMakeFloatMenuForActiveLoad(VehiclePawn_Tractor tractor, IntVec3 sq)//targetPawn是被裝的
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
            if (!tractor.CanReach(sq, PathEndMode.ClosestTouch, Danger.Deadly, false, false, TraverseMode.ByPawn))
            {
                yield return new FloatMenuOption("CanNotReach".Translate() + ": " + "NoPath".Translate().CapitalizeFirst(), null);
                yield break;
            }
            if (!tractor.TrailerMount.TryGetTrailer(out var pawn_Trailer)) yield break;

            List<Thing> things = sq.GetThingList(tractor.Map).Where(i => i is VehiclePawn).ToList();
            if (things.NullOrEmpty()) yield break;

            for (int i = 0; i < things.Count; i++)
            {
                if (things[i] != tractor)//不是自己就行。
                {
                    yield return TryMakeFloatMenuForActiveLoad(tractor, things[i] as Pawn);
                }
                else
                {   //自行解除連接掛車的選項。
                    if (tractor.TrailerMount.TryGetTrailer(out var trailer))
                    {
                        if (trailer.HasCargoComp && trailer.CargoComp.HasPayload)
                        {
                            VehiclePawn target = trailer.CargoComp.GetPayload;
                            yield return new FloatMenuOption("RTC_Unload".Translate(target.Label.Translate()), () =>
                            {
                                trailer.CargoComp.TryUnloadThingUnspawned(tractor, target);
                            });
                        }
                        yield return new FloatMenuOption("RTC_Disconnect".Translate(trailer.Label.Translate()), () =>
                        {
                            tractor.TrailerMount.TryDisconnectTrailer(trailer);
                        });
                    }
                    else
                    {
                        yield return new FloatMenuOption("RTC_NoPayload".Translate(), null);
                    }
                }
            }
        }

        public static FloatMenuOption TryMakeFloatMenuForActiveLoad(VehiclePawn_Tractor tractor, Pawn targetPawn)//targetPawn是被裝的
        {
            if (tractor.TrailerMount.TryGetTrailer(out var trailer) && !trailer.GetComp<CompVehicleCargo>().Accepts(targetPawn))
            {
                return new FloatMenuOption("RTC_CanNotLoad".Translate() + ": " + trailer.GetComp<CompVehicleCargo>().Accepts(targetPawn).Reason, null);
            }
            else
            {
                return new FloatMenuOption("RTC_Load".Translate(targetPawn.Label.Translate()), () =>
                {
                    Job j = JobMaker.MakeJob(VehicleJobDefOf.RTC_LoadToSelf, targetPawn);
                    tractor.jobs.TryTakeOrderedJob(j);
                });
            }
        }

        public static FloatMenuOption TryMakeFloatMenuForCargoLoad(Pawn pawn, Pawn targetPawn)//targetPawn是被裝的，pawn是裝載者
        {
            if (!targetPawn.CanReach(pawn, PathEndMode.ClosestTouch, Danger.Deadly, false, false, TraverseMode.ByPawn))
            {
                return new FloatMenuOption("CanNotReach".Translate() + ": " + "NoPath".Translate().CapitalizeFirst(), null);
            }

            AcceptanceReport report = pawn.TryGetComp<CompVehicleCargo>().Accepts(targetPawn);
            if (report.Accepted)
            {
                return new FloatMenuOption("RTC_Load".Translate(targetPawn.Label.Translate()), () =>
                {
                    Job j = JobMaker.MakeJob(VehicleJobDefOf.RTC_LoadToCargo, pawn);
                    targetPawn.jobs.TryTakeOrderedJob(j);
                });
            }
            else
            {
                return new FloatMenuOption("RTC_CanNotLoad".Translate() + ": " + report.Reason, null);
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
            if (!tractor.CanReach(sq, PathEndMode.ClosestTouch, Danger.Deadly, false, false, TraverseMode.ByPawn))
            {
                yield return new FloatMenuOption("CanNotReach".Translate() + ": " + "NoPath".Translate().CapitalizeFirst(), null);
                yield break;
            }

            List<Thing> things = sq.GetThingList(tractor.Map);
            for (int i = 0; i < things.Count; i++)
            {
                if (things[i] is VehiclePawn_Trailer tmp && tmp != tractor)
                {
                    yield return TryMakeFloatMenuForTrailerLoad(tractor as VehiclePawn_Tractor, tmp);
                }
            }
        }
        public static FloatMenuOption TryMakeFloatMenuForTrailerLoad(VehiclePawn_Tractor tractor, Pawn targetPawn)//targetPawn是被裝的
        {
            if (!tractor.TrailerMount.Accepts(targetPawn))
            {
                return new FloatMenuOption("RTC_CanNotLoad".Translate() + ": " + tractor.TrailerMount.Accepts(targetPawn).Reason, null);
            }
            else
            {
                return new FloatMenuOption("RTC_Load".Translate(targetPawn.Label.Translate()), () =>
                {
                    Job j = JobMaker.MakeJob(VehicleJobDefOf.RTC_ConnectToTrailer, targetPawn);
                    tractor.jobs.TryTakeOrderedJob(j);
                });
            }
        }
    }
}