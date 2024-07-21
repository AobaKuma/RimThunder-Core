using Vehicles;
using Verse;
using UnityEngine;
using System.Collections.Generic;


namespace Motorization
{
    public class CompProperties_MechaWeapon : CompProperties
    {
        public List<ThingDef> equipmentWhitelists = new List<ThingDef>();
        public CompProperties_MechaWeapon()
        {
            compClass = typeof(CompMechaWeapon);
        }
    }
    public class CompMechaWeapon : VehicleComp
    {
        public CompProperties_MechaWeapon Props => base.props as CompProperties_MechaWeapon;
        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions()
        {
            foreach (var item in base.CompFloatMenuOptions())
            {
                yield return item;
            }
        }
    }

}
