using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Vehicles;
using Verse;
using static SmashTools.ConditionalPatch;

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
            CurrentBill?.ResultGraphic?.Draw(DrawPos + new Vector3(0, -0.01f, -0.2f), Rotation, this);
        }

        public bool CellOccupied()
        {
            List<IntVec3> cells = GenAdj.CellsOccupiedBy(Position, Rotation, new IntVec2(def.size.x - 2, def.size.z - 2)).ToList();
            foreach (IntVec3 cell in cells)
            {
                if (cell.GetEdifice(Map) != null)
                {
                    return true;
                }
                if (cell.GetFirstThing<VehiclePawn>(Map) != null)
                {
                    return true;
                }
            }
            return false;
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
            Thing thing = ThingMaker.MakeThing(bill.recipe.GetModExtension<ModExt_RTCVehicleRecipe>().thing);
            thing.SetFaction(Faction);
            GenSpawn.Spawn(thing, Position, Map, Rotation);
            Log.Message(thing.Rotation.ToString());
            if (CurrentBill == bill)
            {
                CurrentBill = null;
            }
        }
    }
}
