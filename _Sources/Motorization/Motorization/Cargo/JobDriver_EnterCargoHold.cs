using Verse;
using System.Collections.Generic;
using Verse.AI;
using Verse.Sound;
using RimWorld;
using Vehicles;
using SmashTools;


namespace Motorization
{
    //由具備CompVehicleCargo的母車主動開過去，把targetVehicle收進自己的貨艙。
    public class JobDriver_EnterCargoHold : JobDriver
    {
        public VehiclePawn TargetVehicle => job.GetTarget(TargetIndex.A).Pawn as VehiclePawn;
        public VehiclePawn Carrier => this.GetActor() as VehiclePawn;
        public const int EnterDelay = 60;
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }
        List<Pawn> cacheCrews = null;
        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            this.FailOnDowned(TargetIndex.A);

            yield return CarrierUtility.ToNearestCell(Carrier, TargetVehicle);

            CompVehicleCargo cargo = Carrier.TryGetComp<CompVehicleCargo>();
            this.FailOn(() => cargo == null || !cargo.Accepts(TargetVehicle));
            Toil t = Toils_General.Wait(EnterDelay);
            t.PlaySoundAtEnd(SoundDefOf.Artillery_ShellLoaded);
            yield return t;
            yield return Toils_General.Do(delegate
            {
                VehiclePawn target = TargetVehicle;

                if (target.AllPawnsAboard.HasData()) //crew transport
                {
                    cacheCrews = new List<Pawn>();
                    for (int i = target.AllPawnsAboard.Count - 1; i >= 0; i--)
                    {
                        cacheCrews.Add(target.AllPawnsAboard[i]);
                        Log.Message(target.AllPawnsAboard[i].Name.ToString());
                    }
                    target.DisembarkAll();
                }
                target.ignition.Drafted = false;
                cargo.TryAttach(target);
            });
        }
    }
}
