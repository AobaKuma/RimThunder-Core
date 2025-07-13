using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using HarmonyLib;
using Verse;

namespace Motorization
{
    [UsedImplicitly]
    [StaticConstructorOnStartup]
    public class HarmonyMain
    {
        private static Harmony harmonyInstance;

        internal static Harmony HarmonyInstance
        {
            get
            {
                if (harmonyInstance == null)
                {
                    harmonyInstance = new Harmony("RTC_HarmonyPatches");
                }
                return harmonyInstance;
            }
        }

        static HarmonyMain()
        {
            HarmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}
