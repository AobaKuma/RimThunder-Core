using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using UnityEngine;
using Vehicles;

namespace Motorization
{
    internal class TurretRestrictions_Undraft : TurretRestrictions
    {
        public override bool Disabled => !vehicle.Drafted;

        public override string DisableReason => "RTC_TurretRestrictUndraftReason".Translate();
    }
}
