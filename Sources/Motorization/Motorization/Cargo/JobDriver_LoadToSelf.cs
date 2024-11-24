using Verse;
using System.Collections.Generic;
using Verse.AI;
using RimWorld;
using Vehicles;
using UnityEngine;
using Verse.AI.Group;
using System.Runtime.InteropServices;


namespace Motorization
{
    //如果一個Tractor拖著一個帶CarrierComp的Trailer,那麼就能透過這個Job直接將符合條件的載具裝載。
    public class JobDriver_LoadToSelf : JobDriver
    {
        public VehiclePawn TargetPawn => job.GetTarget(TargetIndex.A).Pawn as VehiclePawn;

        VehiclePawn_Tractor CarrierPawn => GetActor() as VehiclePawn_Tractor;
        public const int EnterDelay = 60;
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            if (CarrierPawn.TrailerMount.TryGetTrailer(out VehiclePawn_Trailer pawn_Trailer))
            {
                var cargo = pawn_Trailer.GetComp<CompVehicleCargo>();
                AcceptanceReport report = cargo.Accepts(TargetPawn);
                if (report.Accepted) return true;
                else
                {
                    Messages.Message(report.Reason, MessageTypeDefOf.RejectInput, false);
                }
            }
            return false;
        }

        List<Pawn> cacheCrews = null;
        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            yield return CarrierUtility.ToNearestCell(CarrierPawn,TargetPawn);
            this.FailOn(() => !CarrierPawn.HasTrailer);

            if (TargetPawn.CompVehicleTurrets != null && TargetPawn.CompVehicleTurrets.CanDeploy)
            {
                if (TargetPawn.CompVehicleTurrets.Deployed)
                {
                    TargetPawn.CompVehicleTurrets.ToggleDeployment();
                }
            }

            Toil t = Toils_General.Wait(EnterDelay);
            t.PlaySoundAtEnd(SoundDefOf.Artillery_ShellLoaded);
            yield return t;

            yield return Toils_General.Do(delegate
            {
                CarrierPawn.TrailerMount.TryGetTrailer(out VehiclePawn_Trailer pawn_Trailer);
                var cargo = pawn_Trailer.GetComp<CompVehicleCargo>();

                cacheCrews = new List<Pawn>();
                if (!TargetPawn.AllPawnsAboard.NullOrEmpty()) //crew transport
                {
                    for (int i = TargetPawn.AllPawnsAboard.Count - 1; i >= 0; i--)
                    {
                        cacheCrews.Add(TargetPawn.AllPawnsAboard[i]);
                        Log.Message(TargetPawn.AllPawnsAboard[i].Name);
                    }
                    TargetPawn.DisembarkAll();

                    //if (!cacheCrews.NullOrEmpty())
                    //{
                    //    foreach (var item in cacheCrews)
                    //    {
                    //        if (CarrierPawn.SeatsAvailable <= 0) break;
                    //        CarrierPawn.TryAddPawn(item);
                    //    }
                    //    cacheCrews = null;
                    //}
                }
                TargetPawn.ignition.Drafted = false;
                cargo.TryAcceptThing(TargetPawn);

            });
        }
        public static void ThrowAppropriateHistoryEvent(VehicleType type, Pawn pawn)
        {
            if (ModsConfig.IdeologyActive)
            {
                switch (type)
                {
                    case VehicleType.Air:
                        Find.HistoryEventsManager.RecordEvent(new HistoryEvent(HistoryEventDefOf_Vehicles.VF_BoardedAirVehicle, pawn.Named(HistoryEventArgsNames.Doer)));
                        break;
                    case VehicleType.Sea:
                        Find.HistoryEventsManager.RecordEvent(new HistoryEvent(HistoryEventDefOf_Vehicles.VF_BoardedSeaVehicle, pawn.Named(HistoryEventArgsNames.Doer)));
                        break;
                    case VehicleType.Land:
                        Find.HistoryEventsManager.RecordEvent(new HistoryEvent(HistoryEventDefOf_Vehicles.VF_BoardedLandVehicle, pawn.Named(HistoryEventArgsNames.Doer)));
                        break;
                    case VehicleType.Universal:
                        Find.HistoryEventsManager.RecordEvent(new HistoryEvent(HistoryEventDefOf_Vehicles.VF_BoardedUniversalVehicle, pawn.Named(HistoryEventArgsNames.Doer)));
                        break;

                }
            }
        }
    }
}