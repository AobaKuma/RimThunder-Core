using UnityEngine;
using Verse;

namespace Motorization
{
    public class Projectile_Parabola : Projectile_Explosive
    {
        public Vector3 ExactPos => DrawPos + new Vector3(0f, 0f, 1f) * (ArcHeightFactor * GenMath.InverseParabola(DistanceCoveredFraction));
        private float ArcHeightFactor
        {
            get
            {
                float num = Mathf.Lerp(1, def.projectile.arcHeightFactor, Mathf.Clamp01(Vector3.Magnitude(destination - origin) / 24));
                float num2 = (destination - origin).MagnitudeHorizontalSquared();
                if (Mathf.Pow(num, 2) > num2 * 0.2f * 0.2f)
                {
                    num = Mathf.Sqrt(num2) * 0.2f;
                }
                return num;
            }
        }
        public float Progress => DistanceCoveredFraction;
        private Vector3 LookTowards => new Vector3(destination.x - origin.x, def.Altitude, destination.z - origin.z + ArcHeightFactor * Accelerate);
        public override Quaternion ExactRotation => Quaternion.LookRotation(LookTowards);
        public float Accelerate => 5f - 10f * DistanceCoveredFraction;
        public override void Launch(Thing launcher, Vector3 origin, LocalTargetInfo usedTarget, LocalTargetInfo intendedTarget, ProjectileHitFlags hitFlags, bool preventFriendlyFire = false, Thing equipment = null, ThingDef targetCoverDef = null)
        {
            base.Launch(launcher, origin, usedTarget, intendedTarget, hitFlags, preventFriendlyFire, equipment, targetCoverDef);
        }
    }
}
