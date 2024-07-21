using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using Vehicles;
using Verse;

namespace Motorization
{
    [StaticConstructorOnStartup]
    public class Harmony_FloatMenuOption
    {
        [HarmonyPatch(typeof(VehiclePawn), nameof(VehiclePawn.GetFloatMenuOptions))]
        class Harmony_FloatMenuOption_PostFix
        {
            //static IEnumerable<FloatMenuOption> Postfix(VehiclePawn __instance, object __0)
            //{
            //    if(!(__0 is Pawn pawn)) yield break;

            //    foreach (ThingComp thingComp in __instance.AllComps)
            //    {
            //        if (!(thingComp is VehicleComp vehicleComp))
            //        {
            //            continue;
            //        }

            //        foreach (FloatMenuOption item in vehicleComp.CompFloatMenuOptions(selPawn: pawn))
            //        {
            //            yield return item;
            //        }
            //    }
            //}
        }
    }
}
