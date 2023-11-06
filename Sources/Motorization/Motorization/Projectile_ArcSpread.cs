using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using UnityEngine;

namespace Motorization
{
    public class Projectile_ArcSpread : Bullet
    {
        RotationalArcSpreadExtension Extension => def.GetModExtension<RotationalArcSpreadExtension>();
        public override void Launch(Thing launcher, Vector3 origin, LocalTargetInfo usedTarget, LocalTargetInfo intendedTarget, ProjectileHitFlags hitFlags, bool preventFriendlyFire = false, Thing equipment = null, ThingDef targetCoverDef = null)
        {
            int num = GenRadial.NumCellsInRadius(Extension.range.max);
            List<IntVec3> affectedCells = new List<IntVec3>();
            for (int i = 1; i < num; i++)
            {
                IntVec3 intVec = launcher.Position + GenRadial.RadialPattern[i];
                Vector3 vec3 = (Extension.invert ? launcher.Position - intVec : intVec - launcher.Position).ToVector3();
                vec3.y = 0;
                if (Math.Abs(Vector3.Angle(intendedTarget.Cell.ToVector3() - origin, vec3)) < Extension.arc && Vector3.Distance(origin, vec3) > Extension.range.min)
                {
                    affectedCells.Add(intVec);
                }
            }
            for (int i = 0; i < Extension.amount; i++)
            {
                Projectile thing = (Projectile)GenSpawn.Spawn(Extension.projectile, launcher.Position, Map);
                thing.Launch(launcher, origin, affectedCells.RandomElement(), intendedTarget, hitFlags, preventFriendlyFire, equipment, targetCoverDef);
            }
            Destroy();
        }
    }

    public class RotationalArcSpreadExtension : DefModExtension
    {
        public bool random = false;
        public bool invert = false;
        public ThingDef projectile;
        public int amount = 5;
        public float arc = 45;
        public IntRange range = new IntRange(3, 7);
    }
}
