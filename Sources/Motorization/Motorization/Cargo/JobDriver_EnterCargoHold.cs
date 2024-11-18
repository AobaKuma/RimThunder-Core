using Verse;
using System.Collections.Generic;
using Verse.AI;
using Verse.Sound;
using RimWorld;
using Vehicles;
using UnityEngine;
using SmashTools;


namespace Motorization
{
    public class JobDriver_EnterCargoHold : JobDriver
    {
        public VehiclePawn Carrier => job.GetTarget(TargetIndex.A).Pawn as VehiclePawn;
        public VehiclePawn Loader => this.GetActor() as VehiclePawn;
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

            yield return CarrierUtility.ToNearestCell(Loader, Carrier,true);

            CompVehicleCargo cargo = Carrier.TryGetComp<CompVehicleCargo>();
            this.FailOn(() => !cargo.Accepts(pawn));
            Toil t = Toils_General.Wait(EnterDelay);
            t.PlaySoundAtEnd(SoundDefOf.Artillery_ShellLoaded);
            yield return t;
            yield return Toils_General.Do(delegate
            {

                if (!Loader.AllPawnsAboard.NullOrEmpty()) //crew transport
                {
                    cacheCrews = new List<Pawn>();
                    for (int i = Loader.AllPawnsAboard.Count - 1; i >= 0; i--)
                    {
                        cacheCrews.Add(Loader.AllPawnsAboard[i]);
                        Log.Message(Loader.AllPawnsAboard[i].Name);
                    }
                    Loader.DisembarkAll();

                    if (!cacheCrews.NullOrEmpty())
                    {
                        foreach (var item in cacheCrews)
                        {
                            if (Carrier.SeatsAvailable <= 0) break;

                            Carrier.TryAddPawn(item);
                        }
                        cacheCrews = null;
                    }
                }
                Loader.ignition.Drafted = false;
                cargo.TryAcceptThing(Loader);
            });
        }
    }
}
