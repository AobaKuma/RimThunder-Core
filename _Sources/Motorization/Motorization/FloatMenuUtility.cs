using RimWorld;
using Verse;
using Verse.AI;
using Vehicles;

namespace Motorization
{
    /// <summary>
    /// 建立子母載具互動用 FloatMenuOption 的純邏輯工具。
    /// 實際的選單觸發點(選中誰、點擊什麼)由 Cargo/FloatMenuOptionProviders.cs 內的
    /// FloatMenuOptionProvider_Vehicle 子類別負責，這裡只負責組出對應的 FloatMenuOption。
    /// </summary>
    public static class FloatMenuUtility
    {
        /// <summary>
        /// 拖曳車(Tractor)已掛載的拖車(Trailer)本身帶有貨艙時，命令拖曳車將targetPawn裝進拖車貨艙。
        /// </summary>
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

        /// <summary>
        /// pawn(裝載者，具CompVehicleCargo)主動開過去把targetPawn(被裝的載具)收進貨艙。
        /// 對應JobDriver_EnterCargoHold: Carrier(=pawn)=Actor, TargetVehicle(=targetPawn)=TargetA。
        /// </summary>
        public static FloatMenuOption TryMakeFloatMenuForCargoLoad(Pawn pawn, Pawn targetPawn)//targetPawn是被裝的，pawn是主動裝載的母車
        {
            if (!pawn.CanReach(targetPawn, PathEndMode.ClosestTouch, Danger.Deadly, false, false, TraverseMode.ByPawn))
            {
                return new FloatMenuOption("CanNotReach".Translate() + ": " + "NoPath".Translate().CapitalizeFirst(), null);
            }

            AcceptanceReport report = pawn.TryGetComp<CompVehicleCargo>().Accepts(targetPawn);
            if (report.Accepted)
            {
                return new FloatMenuOption("RTC_Load".Translate(targetPawn.Label.Translate()), () =>
                {
                    Job j = JobMaker.MakeJob(VehicleJobDefOf.RTC_LoadToCargo, targetPawn);
                    pawn.jobs.TryTakeOrderedJob(j);
                });
            }
            else
            {
                return new FloatMenuOption("RTC_CanNotLoad".Translate() + ": " + report.Reason, null);
            }
        }

        /// <summary>
        /// 拖曳車(tractor)主動開過去，把targetPawn(拖車)掛載到自己身上。
        /// </summary>
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
