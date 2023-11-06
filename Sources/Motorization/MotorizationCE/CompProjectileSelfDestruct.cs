using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CombatExtended;
using UnityEngine;
using Verse;

namespace MotorizationCE
{
    public class CompProjectileSelfDestructCE : ThingComp
    {
        public CompProperties_ProjectileSelfDestructCE Props => (CompProperties_ProjectileSelfDestructCE)props;
        public override void CompTick()
        {
            if (parent.Spawned && parent is ProjectileCE projectile)
            {
                float targetRange = Props.range > 0 ? Props.range : projectile.equipment.def.Verbs[0].range * Props.rangepct;
                if (targetRange > 0 && Vector3.Distance(projectile.OriginIV3.ToVector3(), parent.DrawPos) > targetRange)
                {
                    projectile.Impact(null);
                    projectile.landed = true;
                }
            }
        }
    }

    public class CompProperties_ProjectileSelfDestructCE : CompProperties
    {
        public CompProperties_ProjectileSelfDestructCE()
        {
            compClass = typeof(CompProjectileSelfDestructCE);
        }

        public float range = -1;

        public float rangepct = 1.2f;
    }
}
