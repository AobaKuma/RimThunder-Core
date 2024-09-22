using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Vehicles;
using Verse;
using Verse.AI;
using Verse.Sound;


namespace Motorization
{
    /// <summary>
    /// 單獨獨立出來的部屬系統，這樣就可以給其他非戰鬥類載具提供功能性部屬的能力。
    /// </summary>
    public class CompDeployable : VehicleComp
    {
        public CompProperties_Deployable Props => base.props as CompProperties_Deployable;
        private bool deployed;
        internal int deployTicks;

        public bool CanDeploy { get; private set; }
        public bool Deployed => deployed;
        public int DeployTicks => Mathf.RoundToInt(Props.deployTime * 60f);

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (base.Vehicle.Faction != Faction.OfPlayer && !DebugSettings.ShowDevGizmos)
            {
                yield break;
            }
            if (CanDeploy)
            {
                Command_Toggle deployToggle = new Command_Toggle
                {
                    icon = (Deployed ? VehicleTex.UndeployVehicle : VehicleTex.DeployVehicle),
                    defaultLabel = (Deployed ? "VF_Undeploy".Translate() : "VF_Deploy".Translate()),
                    defaultDesc = "VF_DeployDescription".Translate(),
                    toggleAction = delegate
                    {
                        base.Vehicle.jobs.StartJob(new Job(JobDefOf_Vehicles.DeployVehicle, base.Vehicle), JobCondition.InterruptForced);
                        deployTicks = DeployTicks;
                    },
                    isActive = () => Deployed
                };
                if (!base.Vehicle.CanMoveFinal)
                {
                    deployToggle.Disable();
                }

                if (base.Vehicle.Deploying)
                {
                    deployToggle.Disable();
                }

                if (base.Vehicle.vehiclePather.Moving)
                {
                    deployToggle.Disable();
                }

                if (base.Vehicle.CompUpgradeTree != null && base.Vehicle.CompUpgradeTree.Upgrading)
                {
                    deployToggle.Disable("VF_DisabledByVehicleUpgrading".Translate(base.Vehicle.LabelCap));
                }

                yield return deployToggle;
            }
        }
        public void RecacheDeployment()
        {
            CanDeploy = SettingsCache.TryGetValue(base.Vehicle.VehicleDef, typeof(CompProperties_VehicleTurrets), "deployTime", Props.deployTime) > 0f;
        }
        public void ToggleDeployment()
        {
            deployed = !deployed;
            deployTicks = 0;
            if (deployed)
            {
                Props.deploySound?.PlayOneShot(base.Vehicle);
                if (Props.mobileWhileDeployed) Vehicle.movementStatus = VehicleMovementStatus.Offline;
                base.Vehicle.EventRegistry[VehicleEventDefOf.Deployed].ExecuteEvents();
            }
            else
            {
                Props.undeploySound?.PlayOneShot(base.Vehicle);
                base.Vehicle.EventRegistry[VehicleEventDefOf.Undeployed].ExecuteEvents();
                if (Props.mobileWhileDeployed) Vehicle.movementStatus = VehicleMovementStatus.Online;
            }
        }
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref deployed, "deployed", defaultValue: false);
            Scribe_Values.Look(ref deployTicks, "deployTicks", 0);
        }
    }
    public class CompProperties_Deployable : CompProperties
    {
        public bool mobileWhileDeployed = true;
        public int deployTime;

        public SoundDef deployingSustainer;
        public SoundDef deploySound;
        public SoundDef undeploySound;
        public CompProperties_Deployable()
        {
            compClass = typeof(CompDeployToggleTexture);
        }
    }
}
