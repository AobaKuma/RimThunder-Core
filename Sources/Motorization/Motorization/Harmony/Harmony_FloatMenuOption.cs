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
            static IEnumerable<FloatMenuOption> Postfix(VehiclePawn __instance, Pawn selPawn)
            {
                if (selPawn is null) yield break;
                if (selPawn.Faction != Faction.OfPlayer) yield break;
                if (!(selPawn is VehiclePawn pawn)) yield break;

                if (__instance.TryGetComp<CompVehicleCargo>(out var vehicleCargo))
                {
                        yield return vehicleCargo.FloatMenuOptionForCargo(pawn);
                }
            }
        }
    }
}
