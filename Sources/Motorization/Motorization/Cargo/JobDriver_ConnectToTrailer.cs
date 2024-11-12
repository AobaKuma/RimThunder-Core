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
        VehiclePawn_Tractor TractorPawn => GetActor() as VehiclePawn_Tractor;
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
            this.FailOn(() => !TractorPawn.TrailerMount.Accepts(TrailerPawn));
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
                TractorPawn.TrailerMount.TryAcceptThing(TrailerPawn);

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
            Vector2 v2 = target.FullRotation.AsVector2.normalized;
            IntVec3 v = new IntVec3((int)v2.x, 0, (int)v2.y) * ((target.def.Size.z + 3) / 2);
            return target.Position + v;
            //到時候要做Unload時生成在指定位置
        }
    }
}
