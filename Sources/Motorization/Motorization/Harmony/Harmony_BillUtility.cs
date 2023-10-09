using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Motorization
{
    [StaticConstructorOnStartup]
    public class Harmony_BillUtility
    {
        [HarmonyPatch(typeof(BillUtility), "MakeNewBill")]
        static class MakeNewBill_PostFix
        {
            [HarmonyPostfix]
            static void PostFix(RecipeDef recipe, ref Bill __result)
            {
                if (recipe.HasModExtension<ModExt_RTCVehicleRecipe>())
                {
                    __result = new Bill_RTCVehicle(recipe, null);
                }
            }
        }
    }
}
