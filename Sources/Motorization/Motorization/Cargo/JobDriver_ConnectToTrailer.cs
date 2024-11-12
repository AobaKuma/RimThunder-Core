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
    public class JobDriver_ConnectToTrailer : JobDriver
    {
        public VehiclePawn TrailerPawn => job.GetTarget(TargetIndex.A).Pawn as VehiclePawn;
        VehiclePawn TractorPawn => GetActor() as VehiclePawn;
        public const int EnterDelay = 60;
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            if (!TractorPawn.TryGetComp<CompTrailerMount>(out var _)) return false;
            return true;
        }
        List<Pawn> cacheCrews = null;
        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            yield return ToNearestCell(TrailerPawn);

            CompTrailerMount mount = TractorPawn.GetComp<CompTrailerMount>();
            this.FailOn(() => !mount.Accepts(TrailerPawn));
            if (TrailerPawn.CompVehicleTurrets != null && TrailerPawn.CompVehicleTurrets.CanDeploy)
            {
                if (TrailerPawn.CompVehicleTurrets.Deployed)
                {
                    TrailerPawn.CompVehicleTurrets.ToggleDeployment();
                }
            }
            Toil t = Toils_General.Wait(EnterDelay);
            t.PlaySoundAtEnd(SoundDefOf.Artillery_ShellLoaded);
            yield return t;

            yield return Toils_General.Do(delegate
            {
                cacheCrews = new List<Pawn>();
                if (!TrailerPawn.AllPawnsAboard.NullOrEmpty()) //crew transport
                {
                    for (int i = TrailerPawn.AllPawnsAboard.Count - 1; i >= 0; i--)
                    {
                        cacheCrews.Add(TrailerPawn.AllPawnsAboard[i]);
                        Log.Message(TrailerPawn.AllPawnsAboard[i].Name);
                    }
                    TrailerPawn.DisembarkAll();

                    if (!cacheCrews.NullOrEmpty())
                    {
                        foreach (var item in cacheCrews)
                        {
                            if (TractorPawn.SeatsAvailable <= 0) break;
                            TractorPawn.TryAddPawn(item);
                        }
                        cacheCrews = null;
                    }
                }
                TrailerPawn.ignition.Drafted = false;
                mount.TryAcceptThing(TrailerPawn);

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
        public static Toil BoardVehicle(Pawn pawn)
        {
            Toil toil = new Toil();
            toil.initAction = delegate ()
            {
                VehiclePawn vehicle = toil.actor.jobs.curJob.GetTarget(TargetIndex.A).Thing as VehiclePawn;
                if (pawn.GetLord()?.LordJob is LordJob_FormAndSendVehicles lordJob)
                {
                    (VehiclePawn vehicle, VehicleHandler handler) assignedSeat = lordJob.GetVehicleAssigned(pawn);
                    assignedSeat.vehicle.TryAddPawn(pawn, assignedSeat.handler);
                    return;
                }
                vehicle.Notify_Boarded(pawn);
                ThrowAppropriateHistoryEvent(vehicle.VehicleDef.vehicleType, toil.actor);

            };
            toil.defaultCompleteMode = ToilCompleteMode.Instant;
            return toil;
        }

        private Toil ToNearestCell(VehiclePawn targetVehicle)
        {
            Toil toil = ToilMaker.MakeToil("GotoThing");
            toil.initAction = delegate
            {
                TractorPawn.ignition.Drafted = true;
                toil.actor.pather.StartPath(RearCell(targetVehicle), PathEndMode.OnCell);
            };
            toil.defaultCompleteMode = ToilCompleteMode.PatherArrival;
            toil.FailOnDespawnedOrNull(TargetIndex.A);
            return toil;
        }
        private IntVec3 RearCell(VehiclePawn target)
        {
            Vector2 v2 = target.FullRotation.Opposite.AsVector2.normalized;
            IntVec3 v = new IntVec3((int)v2.x, 0, (int)v2.y) * ((target.def.Size.z + 3) / 2);
            return target.Position + v;
            //到時候要做Unload時生成在指定位置
        }
    }
}
