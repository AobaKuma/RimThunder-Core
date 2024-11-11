using System.Collections.Generic;
using Vehicles;
using Verse;
using Verse.AI;


namespace Motorization
{
    public class JobDriver_Deploy : JobDriver
    {
        protected VehiclePawn Vehicle => base.TargetA.Thing as VehiclePawn;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return Vehicle.TryGetComp<CompDeployable>(out var _c);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedOrNull(TargetIndex.A);
            this.FailOn(() => !Vehicle.Spawned);
            Toil deployToil = ToilMaker.MakeToil("MakeNewToils");

            CompDeployable comp = Vehicle.GetComp<CompDeployable>();
            CompDeployToggleTexture retextureComp = Vehicle.GetComp<CompDeployToggleTexture>();

            deployToil.initAction = delegate
            {
                base.Map.pawnDestinationReservationManager.Reserve(Vehicle, job, Vehicle.Position);
                Vehicle.vehiclePather.StopDead();
                if (comp.Deployed)
                {
                    retextureComp?.ToggleDeployment();
                }

                if (comp.Props.deployingSustainer != null)
                {
                    deployToil.PlaySustainerOrSound(comp.Props.deployingSustainer);
                }
            };
            deployToil.tickAction = delegate
            {
                if (!comp.Deployed || Vehicle.CompVehicleTurrets.TurretsAligned)
                {
                    comp.deployTicks--;
                }

                if (comp.deployTicks <= 0)
                {
                    comp.ToggleDeployment();
                    ReadyForNextToil();
                }
            };
            deployToil.WithProgressBar(TargetIndex.A, () => 1f - (float)comp.deployTicks / (float)comp.DeployTicks);
            deployToil.defaultCompleteMode = ToilCompleteMode.Never;
            yield return deployToil;
        }
    }
}
