using Vehicles;
using Verse;
using System.Collections.Generic;
using System;
using System.Linq;


namespace Motorization
{
    public class CompDeployToggleTexture : VehicleComp
    {
        private bool _isDeployed = false;
        public CompProperties_DeployToggleTexture Props => base.props as CompProperties_DeployToggleTexture;
        private CompDeployable _cachedComp = null;
        public bool IsDeployed => _isDeployed;
        public override void CompTick()
        {
            if (Vehicle.Spawned)
            {
                if (Vehicle.CompVehicleTurrets == null) return;
                if (Vehicle.CompVehicleTurrets.CanDeploy)
                {
                    if (Vehicle.CompVehicleTurrets.Deployed)
                    {
                        if (!_isDeployed)
                        {
                            Vehicle.SetRetexture(Props.GetRetexture(null)); //現在還沒辦法獲取當前retexture，等Phil
                            _isDeployed = true;
                        }
                    }
                    else if (_isDeployed)
                    {
                        Vehicle.SetRetexture(null);
                        _isDeployed = false;
                    }
                }
            }
        }
        public void ToggleDeployment()
        {
            if (_cachedComp == null) _cachedComp = Vehicle.GetComp<CompDeployable>();
            if (_cachedComp.Deployed)
            {
                if (!_isDeployed)
                {
                    Vehicle.SetRetexture(Props.GetRetexture(null)); //現在還沒辦法獲取當前retexture，等Phil
                    _isDeployed = true;
                }
            }
            else if (_isDeployed)
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
        public List<TogglePair> retexturePairs = new List<TogglePair>();
        public RetextureDef GetRetexture(RetextureDef original)
        {
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
