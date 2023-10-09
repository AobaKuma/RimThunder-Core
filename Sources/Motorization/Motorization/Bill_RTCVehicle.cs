using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace Motorization
{
    public class Bill_RTCVehicle : Bill
    {
        public Building_RTCCrane WorkBench;

        public Bill_RTCVehicle(RecipeDef recipe, Precept_ThingStyle precept = null) : base(recipe, precept)
        {
            WorkBench = billStack.billGiver as Building_RTCCrane;
        }

        public override void ExposeData()
        {
            base.ExposeData();
        }

        public override bool ShouldDoNow()
        {
            if (suspended || (WorkBench.CurrentBill != null && WorkBench.CurrentBill != this))
            {
                return false;
            }
            return true;
        }

        public override void Notify_BillWorkStarted(Pawn billDoer)
        {
            WorkBench.CurrentBill = this;
        }

        public override void Notify_IterationCompleted(Pawn billDoer, List<Thing> ingredients)
        {
            base.Notify_IterationCompleted(billDoer, ingredients);
        }
    }

    public class ModExt_RTCVehicleRecipe : DefModExtension
    {
    }
}
