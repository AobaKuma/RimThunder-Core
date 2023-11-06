using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CombatExtended;
using Motorization;
using Verse;
using UnityEngine;

namespace MotorizationCE
{
    public class ProjectileCE_ArcSpread : BulletCE
    {
        RotationalArcSpreadExtension Extension => def.GetModExtension<RotationalArcSpreadExtension>();

        public override void Launch(Thing launcher, Vector2 origin, float shotAngle, float shotRotation, float shotHeight = 0, float shotSpeed = -1, Thing equipment = null, float distance = -1)
        {
            for (int i = 0; i < Extension.amount; i++)
            {
                ProjectileCE thing = (ProjectileCE)GenSpawn.Spawn(Extension.projectile, launcher.Position, Map);
                if (Extension.random)
                {
                    thing.Launch(launcher, origin, shotAngle, shotRotation + Rand.Sign + Rand.Value * Extension.arc, shotHeight, shotSpeed, equipment, distance);
                }
                else
                {
                    thing.Launch(launcher, origin, shotAngle, (shotRotation - Extension.arc) + Extension.arc / (Extension.amount - 1) * i * 2, shotHeight, shotSpeed, equipment, distance);
                }
            }
            Destroy();
        }
    }
}
