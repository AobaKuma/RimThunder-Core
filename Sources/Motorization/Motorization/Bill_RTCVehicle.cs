using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Vehicles;
using Verse;

namespace Motorization
{
    public class Bill_RTCVehicle : Bill_Production
    {
        public Building_RTCCrane WorkBench => billStack.billGiver as Building_RTCCrane;

        public ModExt_RTCVehicleRecipe Extension => recipe.GetModExtension<ModExt_RTCVehicleRecipe>();

        public Bill_RTCVehicle()
        {
        }

        public Bill_RTCVehicle(RecipeDef recipe, Precept_ThingStyle precept = null) : base(recipe, precept)
        {
        }

        public override void ExposeData()
        {
            base.ExposeData();
        }

        public override bool ShouldDoNow()
        {
            if (suspended || (WorkBench.CurrentBill != null && WorkBench.CurrentBill != this) || WorkBench.CellOccupied)
            {
                return false;
            }
            return true;
        }

        public override void Notify_BillWorkStarted(Pawn billDoer)
        {
            WorkBench.CurrentBill = this;
        }

        public Graphic ResultGraphic
        {
            get
            {
                if (Extension?.unfinishedGraphic != null)
                {
                    return Extension.unfinishedGraphic.Graphic;
                }
                if (Extension?.thing is VehicleBuildDef vehicleBuilding)
                {
                    return vehicleBuilding.thingToSpawn.graphic;
                }
                return Extension?.thing.graphic;
            }
        }

        public override void Notify_IterationCompleted(Pawn billDoer, List<Thing> ingredients)
        {
            if (repeatMode == BillRepeatModeDefOf.RepeatCount)
            {
                if (repeatCount > 0)
                {
                    repeatCount--;
                }
                if (repeatCount == 0)
                {
                    Messages.Message("MessageBillComplete".Translate(LabelCap), (Thing)billStack.billGiver, MessageTypeDefOf.TaskCompletion);
                }
            }
            recipe.Worker.Notify_IterationCompleted(billDoer, ingredients);
            WorkBench.Notify_BillComplete(this);
        }
    }

    public class ModExt_RTCVehicleRecipe : DefModExtension
    {
        public ThingDef thing;

        public GraphicData unfinishedGraphic;
    }
}
