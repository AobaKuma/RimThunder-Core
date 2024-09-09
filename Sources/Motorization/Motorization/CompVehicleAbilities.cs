using System.Linq;
using Vehicles;
using Verse;
using System.Collections.Generic;
using RimWorld;

namespace Motorization
{
    public class CompVehicleAbilities : VehicleComp
    {
        public CompProperties_Abilities Props => base.props as CompProperties_Abilities;
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            foreach (var item in Props.abilities)
            {
                this.Vehicle.abilities.GetAbility(item);
            }
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            List<Ability> comp = this.Vehicle.abilities.abilities;
            foreach (Ability ability in comp)
            {
                if (ability.GizmosVisible())
                {
                    foreach (var item in ability.GetGizmosExtra())
                    {
                        yield return item;
                    }
                }
            }
        }
    }
    public class CompProperties_Abilities : VehicleCompProperties
    {
        public List<AbilityDef> abilities;
        public CompProperties_Abilities()
        {
            this.compClass = typeof(CompVehicleAbilities);
        }
    }
}
