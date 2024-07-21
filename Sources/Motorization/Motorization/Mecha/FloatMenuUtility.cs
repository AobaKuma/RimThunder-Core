using RimWorld;
using Verse;
using System.Collections.Generic;
using Verse.AI;
using Vehicles;


namespace Motorization
{
    public static class FloatMenuUtility
    {
        public static IEnumerable<FloatMenuOption> GetExtraFloatMenuOptionsFor(Pawn pawn, IntVec3 sq)
        {
            if (pawn == null)
            {
                Log.Error("Error");
                yield break;
            }
            if (pawn.Map == null)
            {
                Log.Error("Error");
                yield break;
            }
            List<Thing> things = sq.GetThingList(pawn.Map);

            for (int i = 0; i < things.Count; i++)
            {
                if (things[i] is VehiclePawn tmp && tmp != pawn)
                {
                    yield return TryMakeFloatMenuForCargoLoad(pawn, tmp);
                    break;
                }
            }
        }
        public static IEnumerable<FloatMenuOption> GetExtraFloatMenuOptionsFor(Pawn pawn, IntVec3 sq, MechaSupportedWeaponExtension extension)
        {
            if (pawn.Map == null)
            {
                Log.Error("Error");
                yield break;
            }
            if (extension == null)
            {
                Log.Error("Error");
                yield break;
            }
            List<Thing> things = sq.GetThingList(pawn.Map);

            for (int i = 0; i < things.Count; i++)
            {
                if (things[i] is ThingWithComps tmp)
                {
                    if (tmp == null) continue;
                    //沒有開啟武器過濾的情況下任意武器都應該能裝備//如果該裝備是可以使用的
                    if (tmp.TryGetComp<CompEquippable>() != null)
                    {
                        if (extension.supportedWeapons.Contains(tmp.def))
                        {
                            yield return TryMakeFloatMenuForWeapon(pawn, tmp);
                        }
                        else
                        {
                            yield return new FloatMenuOption("CannotEquip".Translate(tmp) + "MZ_WeaponNotSupported".Translate(), null);
                        }
                    }
                }
            }
            yield break;
        }

        public static FloatMenuOption TryMakeFloatMenuForWeapon(this Pawn pawn, ThingWithComps equipment)
        {
            return TryMakeFloatMenu(pawn, equipment);
        }

        public static FloatMenuOption TryMakeFloatMenu(Pawn pawn, ThingWithComps equipment, string key = "Equip")
        {
            string labelShort = equipment.LabelShort;
            if (!pawn.CanReach(equipment, PathEndMode.ClosestTouch, Danger.Deadly, false, false, TraverseMode.ByPawn))
            {
                return new FloatMenuOption("CannotEquip".Translate(labelShort) + ": " + "NoPath".Translate().CapitalizeFirst(), null);
            }

            if (pawn is IWeaponUsable weaponUsable && !(equipment is Apparel))
            {
                return new FloatMenuOption(key.Translate(labelShort, equipment), () =>
                {
                    weaponUsable.Equip(equipment);
                });
            }
            return null;
        }
        public static FloatMenuOption TryMakeFloatMenuForCargoLoad(Pawn pawn, Pawn targetPawn)//pawn是被裝的
        {
            if (pawn == null) Log.Error("1");
            if (targetPawn == null) Log.Error("2");
            string key = string.Format("Load {1} into {0}'s cargo", targetPawn.Name, pawn.Name);
            if (!targetPawn.CanReach(pawn, PathEndMode.ClosestTouch, Danger.Deadly, false, false, TraverseMode.ByPawn))
            {
                return new FloatMenuOption("CanNotReach" + ": " + "NoPath".Translate().CapitalizeFirst(), null);
            }
            else
            {
                return new FloatMenuOption(key.Translate(), () =>
                {
                    Job j = JobMaker.MakeJob(VehicleJobDefOf.RTC_LoadToCargo, pawn);
                    targetPawn.jobs.TryTakeOrderedJob(j);
                    //targetPawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(VehicleJobDefOf.RTC_LoadToCargo, pawn), JobTag.Misc);
                });
            }
        }
    }
}
