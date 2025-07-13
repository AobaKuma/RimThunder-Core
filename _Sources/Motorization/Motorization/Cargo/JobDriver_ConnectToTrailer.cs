using Verse;
using System.Collections.Generic;
using Verse.AI;
using RimWorld;
using Vehicles;
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
            if (!TractorPawn.TrailerMount.Accepts(TrailerPawn))
            {
                Messages.Message(TractorPawn.TrailerMount.Accepts(TrailerPawn).Reason, MessageTypeDefOf.RejectInput, false);
                return false;
            }
            return true;
        }
        List<Pawn> cacheCrews = null;
        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            yield return CarrierUtility.ToNearestCell(TractorPawn,TrailerPawn);
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

                    //if (!cacheCrews.NullOrEmpty())
                    //{
                    //    foreach (var item in cacheCrews)
                    //    {
                    //        if (TractorPawn.SeatsAvailable <= 0) break;
                    //        TractorPawn.TryAddPawn(item);
                    //    }
                    //    cacheCrews = null;
                    //}
                }
                TrailerPawn.ignition.Drafted = false;
                TractorPawn.TrailerMount.TryAcceptThing(TrailerPawn);
                TractorPawn.ignition.Drafted = false;
                TractorPawn.ignition.Drafted = true;
            });
        }
    }
}
