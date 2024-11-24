using Vehicles;
using Verse;
using System.Collections.Generic;
using System;
using System.Linq;
using SmashTools;


namespace Motorization
{
    public class CompDeployToggleTexture : VehicleComp
    {
        private bool _isDeployed = false;
        public CompProperties_DeployToggleTexture Props => base.props as CompProperties_DeployToggleTexture;

        public bool IsDeployed => _isDeployed;
        public override void CompTick()
        {
            //Log.Message(IsDeployed);
        }
        public override void EventRegistration()
        {
            base.EventRegistration();
            base.Vehicle.AddEvent(VehicleEventDefOf.Deployed, ToggleDeployment);
            base.Vehicle.AddEvent(VehicleEventDefOf.Undeployed, ToggleDeployment);
        }
        public void ToggleDeployment()
        {
            if (Vehicle.CompVehicleTurrets.Deployed)
            {
                //Vehicle.SetRetexture(Props.GetRetexture(Vehicle.Retexture)); //理論上能用了//很抱歉Phil還沒更新。
                Vehicle.SetRetexture(Props.defaultRetexture);
                this.Vehicle.ignition.Drafted = false; 
                _isDeployed = true;
            }
            else
            {
                Vehicle.SetRetexture(null);
                _isDeployed = false;
            }
        }
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref _isDeployed, "_isDeployed", defaultValue: false);
        }
    }
    public class CompProperties_DeployToggleTexture : CompProperties
    {
        public RetextureDef defaultRetexture = null;
        public List<TogglePair> retexturePairs = new List<TogglePair>();
        public RetextureDef GetRetexture(RetextureDef original)
        {
            if (original is null) return defaultRetexture;
            return retexturePairs.Where(p => p.originalDef == original)?.First().textureDef;
        }
        public CompProperties_DeployToggleTexture()
        {
            compClass = typeof(CompDeployToggleTexture);
        }
    }
    [Serializable]
    public class TogglePair
    {
        public RetextureDef originalDef = null;
        public RetextureDef textureDef = null;
    }
}
