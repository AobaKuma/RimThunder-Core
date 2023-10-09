using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;

namespace Motorization
{
    public class Building_RTCCrane : Building_WorkTable
    {
        public Bill_RTCVehicle CurrentBill;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref CurrentBill, "CurrentBill");
        }

        public override void Draw()
        {
            base.Draw();
            CurrentBill?.recipe.products.First().thingDef.graphicData.Graphic.Draw(DrawPos + new Vector3(0, -0.01f, -0.2f), Rotation, this);
        }

        public override void Notify_BillDeleted(Bill bill)
        {
            if (CurrentBill == bill)
            {
                CurrentBill = null;
            }
        }

        public void Notify_BillComplete(Bill bill)
        {
            foreach (ThingDefCountClass result in bill.recipe.products)
            {
                Thing thing = ThingMaker.MakeThing(result.thingDef);
                thing.stackCount = result.count;
                thing.Rotation = Rotation;
                GenSpawn.Spawn(thing, Position, Map);
            }
            if (CurrentBill == bill)
            {
                CurrentBill = null;
            }
        }
    }
}
