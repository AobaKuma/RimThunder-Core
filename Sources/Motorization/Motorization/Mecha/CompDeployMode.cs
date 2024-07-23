using Vehicles;
using Verse;
using System.Collections.Generic;


namespace Motorization
{
    public class CompDeployMode : VehicleComp
    {
        private bool _isDeployed = false;
        public CompProperties_DeployMode Props => base.props as CompProperties_DeployMode;
            
        public override void CompTick()
        {
            if (Vehicle.Spawned)
            {
                if (Vehicle.CompVehicleTurrets.CanDeploy)
                {
                    if (Vehicle.CompVehicleTurrets.Deployed)
                    {
                        if (!_isDeployed)
                        {
                            Vehicle.SetRetexture(Props.retextureDef);
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
    }
    
    public class CompProperties_DeployMode : CompProperties
    {
        public RetextureDef retextureDef;
        public CompProperties_DeployMode()
        {
            compClass = typeof(CompDeployMode);
        }
    }

}
