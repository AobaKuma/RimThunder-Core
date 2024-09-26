﻿using Verse;
using Vehicles;

namespace Motorization
{
    public class LaunchRestriction_WingDeployed : LaunchRestriction_Runway
    {
        public override bool CanStartProtocol(VehiclePawn vehicle, Map map, IntVec3 position, Rot4 rot)
        {
            if (vehicle.TryGetComp<CompDeployable>(out var comp))
            {
                if (comp.Deployed)
                {
                    return base.CanStartProtocol(vehicle, map, position, rot);
                }
                return false;
            }
            return base.CanStartProtocol(vehicle, map, position, rot);
        }
    }
}