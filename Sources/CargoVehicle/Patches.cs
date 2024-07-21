using HarmonyLib;
using KTrie;
using RimWorld;
using RimWorld.BaseGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Vehicles;
using Verse;

namespace VehicleLoadCargo
{
    [StaticConstructorOnStartup]
    public static class Patches
    {
        public static FieldInfo shutDown;

        public static MethodInfo getVehicle;

        //private static readonly Type patchType;

        static Patches() 
        {
            //These two are just fidning methods so they can be called from elesewhere, not actually part of patching anything.
            //Do these somehow magically figure the right object to get when called even though they are in a static constructor?
            shutDown = AccessTools.Field(typeof(CompProjectileInterceptor), "shutDown");
            getVehicle = AccessTools.PropertyGetter(typeof(ITab_Vehicle_Cargo), "Vehicle");

            //Disabling the patch for dropping vehicle, seems to work fien without it in 1.5 seems to handle this OK. Mostly.
            //patchType = typeof(JAHV.Patches);
            //new Harmony("temmie3754.jahv").Patch(AccessTools.Method(typeof(ITab_Vehicle_Cargo), "InterfaceDrop"), new HarmonyMethod(patchType, "Patch_InterfaceDrop"));
        }


        // ~~~~~~~~~~~~~~~~~~~~~~~~ PATCH CURRENTLY NOT BEING APPLIED! ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //Patch to use different place method for vehicles over other cargo. Does not affect DropAll.
        public static bool Patch_InterfaceDrop(ITab_Vehicle_Cargo __instance, Thing thing)
        {

            if (!(thing is VehiclePawn vehiclepawn))
            {
                //thing is not VehiclePawn, so use default behaviour
                return true;
            }
            else
            {
                // thing IS a VehiclePawn, so placing it down
                VehiclePawn containingVehicle = (VehiclePawn)getVehicle.Invoke(__instance, null); //Get the vehicle the Cargo tab is for                                
                if (containingVehicle.inventory.innerContainer.TryDrop(thing, containingVehicle.Position, containingVehicle.Map, ThingPlaceMode.Near, out var _, null, (IntVec3 pos) => !containingVehicle.Map.thingGrid.ThingsListAt(pos).Any((Thing t) => t is VehiclePawn)))
                {
                    Log.Message("containingVehicle.inventory.innerContainer.TryDrop returned TRUE");
                    //If below line is enabled (or any variation I tried) then the function fails as soon as it starts: Exception filling tab Vehicles.ITab_Vehicle_Cargo: System.MissingMethodException: System.Collections.Generic.Dictionary`2<Vehicles.VehicleEventDef, SmashTools.EventTrigger> Vehicles.VehiclePawn.get_EventRegistry()
                    //vehicleObject.EventRegistry[VehicleEventDefOf.CargoRemoved].ExecuteEvents(); //I'm not certain what this does, other than cause erros as soon as the method starts.
                }
                else
                {
                    Log.Message("containingVehicle.inventory.innerContainer.TryDrop returned FALSE");
                }
                return false;
            } 
        }
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        
    }
}
