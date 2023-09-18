using System;
using System.Linq;
using Vehicles;
using RimWorld;
using SmashTools;
using Verse;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;

[HarmonyPatch(typeof(VehicleTurret))]
[HarmonyPatch("TurretRotationTick")]
public static class VehicleTurret_TurretRotationTick
{

    public static void Postfix(VehicleTurret __instance)
    {
        // there should be only one pivotGun in a Vehicle
        if (__instance.key == "pivotGun")
        {
            Log.Warning("Vehicle Rotation: " + __instance.vehicle.Angle + ", Turret Rotation: " + __instance.TurretRotation);
            Log.Warning("Vehicle ROT4: " + __instance.vehicle.Rotation);

            __instance.vehicle.FullRotation = new Rot8(Rot8.FromIntClockwise((int)((__instance.TurretRotation + 22.5) / 45) % 8));
        }
        /* 
            if (292.5 < __instance.TurretRotation || __instance.TurretRotation <= 67.5)//朝上的狀況
            {
                if (292.5 < __instance.TurretRotation && __instance.TurretRotation <= 337.5)//右上
                {
                    __instance.vehicle.FullRotation = Rot8.NorthWest; //7
                }
                if (__instance.TurretRotation > 337.5 || __instance.TurretRotation <= 22.5)//正上
                {
                    __instance.vehicle.FullRotation = Rot8.North;//0
                }
                if (22.5 < __instance.TurretRotation && __instance.TurretRotation <= 67.5)//左上
                {
                    __instance.vehicle.FullRotation = Rot8.NorthEast;//4
                }
            }
            if (67.5 < __instance.TurretRotation && __instance.TurretRotation <= 112.5)//正左
            {
                __instance.vehicle.FullRotation = Rot8.East;//1
            }
            if (112.5 < __instance.TurretRotation && __instance.TurretRotation <= 247.5)//朝下的狀況
            {
                __instance.vehicle.Rotation = Rot4.South;//2
                if (112.5 < __instance.TurretRotation && __instance.TurretRotation <= 157.5)//左下
                {
                    __instance.vehicle.FullRotation = Rot8.SouthEast;//5
                }
                if (157.5 < __instance.TurretRotation && __instance.TurretRotation <= 202.5)//正下
                {
                    __instance.vehicle.FullRotation = Rot8.South;//2
                }
                if (202.5 < __instance.TurretRotation && __instance.TurretRotation <= 247.5)//右下
                {
                    __instance.vehicle.FullRotation = Rot8.SouthWest;
                }
            }
            if (247.5 < __instance.TurretRotation && __instance.TurretRotation <= 292.5)//正右
            {
                __instance.vehicle.FullRotation = Rot8.West;
            }
        }
        */
    }
}

namespace Motorization
{
    [StaticConstructorOnStartup]
    public static class Main
    {
        static Main()
        {
            new Harmony("Aoba.MZ.MobileWorker").PatchAll(Assembly.GetExecutingAssembly());
        }
    }
    public class CompMechaWeapon : VehicleAIComp, IRefundable
    {
        public VehicleTurret turret;
        public CompProperties_MechaWeapon Props => (CompProperties_MechaWeapon)props;
        private void DrawEquipment(Vector3 rootLoc, Rot4 pawnRotation, PawnRenderFlags flags)
        {
            Vector3 drawLoc = new Vector3(0f, (pawnRotation == Rot4.North) ? (-0.00289575267f) : 0.03474903f, 0f);
            Stance_Busy stance_Busy = Vehicle.stances.curStance as Stance_Busy;
            float equipmentDrawDistanceFactor = Vehicle.ageTracker.CurLifeStage.equipmentDrawDistanceFactor;
            if (stance_Busy != null && !stance_Busy.neverAimWeapon && stance_Busy.focusTarg.IsValid && (flags & PawnRenderFlags.NeverAimWeapon) == 0)
            {
                Vector3 vector = ((!stance_Busy.focusTarg.HasThing) ? stance_Busy.focusTarg.Cell.ToVector3Shifted() : stance_Busy.focusTarg.Thing.DrawPos);
                float num = 0f;
                if ((vector - Vehicle.DrawPos).MagnitudeHorizontalSquared() > 0.001f)
                {
                    num = (vector - Vehicle.DrawPos).AngleFlat();
                }

                Verb currentEffectiveVerb = Vehicle.CurrentEffectiveVerb;
                if (currentEffectiveVerb != null && currentEffectiveVerb.AimAngleOverride.HasValue)
                {
                    num = currentEffectiveVerb.AimAngleOverride.Value;
                }

                drawLoc += rootLoc + new Vector3(0f, 0f, 0.4f + Vehicle.equipment.Primary.def.equippedDistanceOffset).RotatedBy(num) * equipmentDrawDistanceFactor;
                DrawEquipmentAiming(Vehicle.equipment.Primary, drawLoc, num);
            }
            else
            {
                if (pawnRotation == Rot4.South)
                {
                    drawLoc += rootLoc + new Vector3(0f, 0f, -0.22f) * equipmentDrawDistanceFactor;
                    DrawEquipmentAiming(Vehicle.equipment.Primary, drawLoc, 143f);
                }
                else if (pawnRotation == Rot4.North)
                {
                    drawLoc += rootLoc + new Vector3(0f, 0f, -0.11f) * equipmentDrawDistanceFactor;
                    DrawEquipmentAiming(Vehicle.equipment.Primary, drawLoc, 143f);
                }
                else if (pawnRotation == Rot4.East)
                {
                    drawLoc += rootLoc + new Vector3(0.2f, 0f, -0.22f) * equipmentDrawDistanceFactor;
                    DrawEquipmentAiming(Vehicle.equipment.Primary, drawLoc, 143f);
                }
                else if (pawnRotation == Rot4.West)
                {
                    drawLoc += rootLoc + new Vector3(-0.2f, 0f, -0.22f) * equipmentDrawDistanceFactor;
                    DrawEquipmentAiming(Vehicle.equipment.Primary, drawLoc, 217f);
                }
            }
        }

        public void DrawEquipmentAiming(Thing eq, Vector3 drawLoc, float aimAngle)
        {
            Mesh mesh = null;
            float num = aimAngle - 90f;
            if (aimAngle > 20f && aimAngle < 160f)
            {
                mesh = MeshPool.plane10;
                num += eq.def.equippedAngleOffset;
            }
            else if (aimAngle > 200f && aimAngle < 340f)
            {
                mesh = MeshPool.plane10Flip;
                num -= 180f;
                num -= eq.def.equippedAngleOffset;
            }
            else
            {
                mesh = MeshPool.plane10;
                num += eq.def.equippedAngleOffset;
            }

            num %= 360f;
            CompEquippable compEquippable = eq.TryGetComp<CompEquippable>();
            if (compEquippable != null)
            {
                EquipmentUtility.Recoil(eq.def, EquipmentUtility.GetRecoilVerb(compEquippable.AllVerbs), out var drawOffset, out var angleOffset, aimAngle);
                drawLoc += drawOffset;
                num += angleOffset;
            }

            Material material = null;
            Graphic_StackCount graphic_StackCount = eq.Graphic as Graphic_StackCount;
            material = ((graphic_StackCount == null) ? eq.Graphic.MatSingleFor(eq) : graphic_StackCount.SubGraphicForStackCount(1, eq.def).MatSingleFor(eq));
            Matrix4x4 matrix = Matrix4x4.TRS(s: new Vector3(eq.Graphic.drawSize.x, 0f, eq.Graphic.drawSize.y), pos: drawLoc, q: Quaternion.AngleAxis(num, Vector3.up));
            Graphics.DrawMesh(mesh, matrix, material, 0);
        }
        public IEnumerable<(ThingDef thingDef, float count)> Refunds //返還彈藥
        {
            get
            {
                    yield return (turret.loadedAmmo, turret.shellCount * turret.turretDef.chargePerAmmoCount);
            }
        }
    }
    public class CompProperties_MechaWeapon : CompProperties
    {
        public ThingDef vehicleTurret;
        public CompProperties_MechaWeapon()
        {
            compClass = typeof(CompMechaWeapon);
        }
    }
}
