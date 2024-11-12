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
        public VehiclePawn Vehicle => job.GetTarget(TargetIndex.A).Pawn as VehiclePawn;
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

            yield return ToNearestCell(Vehicle);

            CompVehicleCargo cargo = Vehicle.TryGetComp<CompVehicleCargo>();
            this.FailOn(() => !cargo.Accepts(pawn));
            Toil t = Toils_General.Wait(EnterDelay);
            t.PlaySoundAtEnd(SoundDefOf.Artillery_ShellLoaded);
            yield return t;
            yield return Toils_General.Do(delegate
            {
                VehiclePawn vehicle = pawn as VehiclePawn;

                if (!vehicle.AllPawnsAboard.NullOrEmpty()) //crew transport
                {
                    cacheCrews = new List<Pawn>();
                    for (int i = vehicle.AllPawnsAboard.Count - 1; i >= 0; i--)
                    {
                        cacheCrews.Add(vehicle.AllPawnsAboard[i]);
                        Log.Message(vehicle.AllPawnsAboard[i].Name);
                    }
                    vehicle.DisembarkAll();

                    if (!cacheCrews.NullOrEmpty())
                    {
                        foreach (var item in cacheCrews)
                        {
                            if (vehicle.SeatsAvailable <= 0) break;
                            vehicle.TryAddPawn(item);
                        }
                        cacheCrews = null;
                    }
                }

                vehicle.ignition.Drafted = false;
                cargo.TryAcceptThing(vehicle);
            });
        }
        private Toil ToNearestCell(VehiclePawn targetVehicle)
        {
            Toil toil = ToilMaker.MakeToil("GotoThing");
            toil.initAction = delegate
            {
                VehiclePawn vehicle = toil.actor as VehiclePawn;
                vehicle.ignition.Drafted = true;
                toil.actor.pather.StartPath(RearCell(targetVehicle), PathEndMode.Touch);
            };
            toil.defaultCompleteMode = ToilCompleteMode.PatherArrival;
            toil.FailOnDespawnedOrNull(TargetIndex.A);
            return toil;
        }
        private IntVec3 RearCell(VehiclePawn target)
        {
            Vector2 v2 = target.FullRotation.Opposite.AsVector2.normalized;
            IntVec3 v = new IntVec3((int)v2.x, 0, (int)v2.y) * ((target.def.Size.z + (pawn as VehiclePawn).def.Size.z) / 2);
            return target.Position + v;
            //到時候要做Unload時生成在指定位置
        }
    }
}
