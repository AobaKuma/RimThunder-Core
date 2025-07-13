using System;
using Vehicles;
using SmashTools;
using System.Reflection;
using HarmonyLib;


namespace Motorization
{

    [HarmonyPatch(typeof(VehicleTurret))]
    [HarmonyPatch("TurretRotationTick")]
    public static class VehicleTurret_TurretRotationTick
    {
        public static void Postfix(VehicleTurret __instance)
        {
            // there should be only one pivotGun in a Vehicle
            if (__instance.key == "pivotGun")
            {
                __instance.vehicle.FullRotation = new Rot8(Rot8.FromIntClockwise((int)((__instance.TurretRotation + 22.5) / 45) % 8));
            }
        }
    }
}
