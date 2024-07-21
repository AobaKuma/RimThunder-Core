using Verse;
using System.Collections.Generic;
using Verse.AI;
using Verse.Sound;
using RimWorld;
using Vehicles;
using UnityEngine;
using SmashTools;
using Verse.Noise;
using static UnityEngine.GraphicsBuffer;


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
        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            this.FailOnDowned(TargetIndex.A);

            yield return ToNearestCell(Vehicle);

            CompVehicleCargo cargo = Vehicle.TryGetComp<CompVehicleCargo>();
            this.FailOn(() => !cargo.Accepts(pawn));
            Toil t = Toils_General.Wait(60);
            t.PlaySoundAtEnd(SoundDefOf.Artillery_ShellLoaded);
            yield return t;
            yield return Toils_General.Do(delegate { cargo.TryAcceptThing(pawn as VehiclePawn); });
        }
        private Toil ToNearestCell(VehiclePawn targetVehicle)
        {
            Toil toil = ToilMaker.MakeToil("GotoThing");
            toil.initAction = delegate
            {
                toil.actor.pather.StartPath(ClosestCell(targetVehicle), PathEndMode.OnCell);
            };
            toil.defaultCompleteMode = ToilCompleteMode.PatherArrival;
            toil.FailOnDespawnedOrNull(TargetIndex.A);
            return toil;
        }
        private IntVec3 ClosestCell(VehiclePawn target)
        {
            Vector2 v2 = target.FullRotation.Opposite.AsVector2;
            IntVec3 v = new IntVec3((int)v2.x, 0, (int)v2.y) * ((target.def.Size.z + 3) / 2);
            Log.Error(v.ToString());
            return target.Position + v;
            //到時候要做Unload時生成在指定位置
        }
    }
}
