using Vehicles;
using Verse;
using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse.AI;
using System.Linq;
using System;


namespace Motorization
{
    [HarmonyPatch(typeof(VehiclePawn))]
    [HarmonyPatch(nameof(VehiclePawn.GetExtraFloatMenuOptionsFor))]
    [HarmonyPatch(new Type[] { typeof(IntVec3) })]
    public static class Patch_VehiclePawn_GetExtraFloatMenuOptionsFor
    {
        //public static void Postfix(VehiclePawn __instance, ref IEnumerable<FloatMenuOption> __result, IntVec3 sq)
        //{
        //    if (__instance.Faction != Faction.OfPlayer) return;

        //    if (__instance.GetComp<CompVehicleCargo>() == null && __instance.GetComp<CompTrailerMount>() == null)
        //    {
        //        return;
        //    }

        //    List<FloatMenuOption> options = __result.ToList();
        //    if (__instance.HasComp<CompTrailerMount>())
        //    {
        //        foreach (var item in FloatMenuUtility.GetExtraFloatMenuOptionsForTractor(__instance, sq))
        //        {
        //            options.Add(item);
        //        }
        //    }
        //    if (__instance.HasComp<CompVehicleCargo>())
        //    {
        //        foreach (var item in FloatMenuUtility.GetExtraFloatMenuOptionsForCarrier(__instance, sq))
        //        {
        //            options.Add(item);
        //        }
        //    }
        //    // Update the Gizmo list
        //    __result = options;
        //}
    }
}