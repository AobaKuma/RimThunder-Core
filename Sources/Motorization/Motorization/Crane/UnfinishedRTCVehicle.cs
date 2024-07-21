using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using UnityEngine;

namespace Motorization
{
    internal class UnfinishedRTCVehicle : UnfinishedThing
    {
        ThingDef product => Recipe.GetModExtension<ModExt_RTCVehicleRecipe>().thing;

        public override string DescriptionDetailed
        {
            get
            {
                if (product == null)
                {
                    return base.LabelNoCount;
                }
                return product.DescriptionDetailed;
            }
        }

        public override string LabelNoCount
        {
            get
            {
                if (product == null)
                {
                    return base.LabelNoCount;
                }
                return "UnfinishedItem".Translate(product.label);
            }
        }

        public override string DescriptionFlavor
        {
            get
            {
                if (product == null)
                {
                    return base.LabelNoCount;
                }
                return product.description;
            }
        }
    }
}
