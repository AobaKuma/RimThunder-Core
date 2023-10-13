using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

        public float craneTargetPct;

        public float craneCurrentPct;

        public float cranePauseTicks;

        ModExtension_CraneTopData craneTopData => def.GetModExtension<ModExtension_CraneTopData>();

        System.Random rand;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            rand = new System.Random();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref CurrentBill, "CurrentBill");
            Scribe_Values.Look(ref craneCurrentPct, "craneCurrentPct");
            Scribe_Values.Look(ref craneTargetPct, "craneTargetPct");
        }

        public override void Draw()
        {
            base.Draw();
            CurrentBill?.ResultGraphic?.Draw(DrawPos + new Vector3(0, -0.01f, -0.2f), Rotation, this);
            craneTopData?.graphicData.Draw(DrawPos + craneTopData.GetDrawOffset(Rotation, craneCurrentPct), Rotation, this);
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

        public override void UsedThisTick()
        {
            base.UsedThisTick();
            CraneTick();
        }

        public void CraneTick()
        {
            if (craneTopData == null)
            {
                return;
            }
            if (craneCurrentPct == craneTargetPct)
            {
                craneTargetPct = (float)rand.NextDouble();
                cranePauseTicks = craneTopData.pauseTickRange.RandomInRange;
            }
            if (cranePauseTicks <= 0)
            {
                float speed = craneTopData.PctPerSec / 60;
                if (Math.Abs(craneTargetPct - craneCurrentPct) <= speed)
                {
                    craneCurrentPct = craneTargetPct;
                }
                else
                {
                    _ = (craneTargetPct < craneCurrentPct) ? craneCurrentPct -= speed : craneCurrentPct += speed;
                }
            }
            else
            {
                cranePauseTicks--;
            }
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

    internal class ModExtension_CraneTopData : DefModExtension
    {
        public Graphic graphicData;

        public CraneEndpoints EndpointSouth;

        public CraneEndpoints EndpointNorth;

        public CraneEndpoints EndpointEast;

        public CraneEndpoints EndpointWest;

        public float PctPerSec = 5f;

        public IntRange pauseTickRange = new IntRange(60, 600);

        public Vector3 GetDrawOffset(Rot4 rotation, float pct)
        {
            if (EndpointNorth == null || EndpointEast == null)
            {
                return Vector3.zero;
            }

            if (EndpointSouth == null)
            {
                EndpointSouth = EndpointNorth;
            }

            if (EndpointWest == null)
            {
                EndpointWest = new CraneEndpoints()
                {
                    endPointA = new Vector3(-EndpointEast.endPointA.x, EndpointEast.endPointA.y, EndpointEast.endPointA.z),
                    endPointB = new Vector3(-EndpointEast.endPointB.x, EndpointEast.endPointB.y, EndpointEast.endPointB.z),
                };
            }

            switch (rotation.AsInt)
            {
                case 0:
                    return DrawOffsetPct(EndpointNorth.endPointA, EndpointNorth.endPointB, pct);
                case 1:
                    return DrawOffsetPct(EndpointEast.endPointA, EndpointEast.endPointB, pct);
                case 2:
                    return DrawOffsetPct(EndpointSouth.endPointA, EndpointSouth.endPointB, pct);
                case 3:
                    return DrawOffsetPct(EndpointWest.endPointA, EndpointWest.endPointB, pct);
            }
            return Vector3.zero;
        }

        Vector3 DrawOffsetPct(Vector3 A, Vector3 B, float pct)
        {
            return A + (A - B) * pct;
        }
    }

    internal class CraneEndpoints
    {
        public Vector3 endPointA;

        public Vector3 endPointB;
    }
}
