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
using Verse.Sound;

namespace Motorization
{
    public class Building_RTCCrane : Building_WorkTable
    {
        public Bill_RTCVehicle CurrentBill;

        public float craneTargetPct = 0;

        public float craneCurrentPct = 1;

        public float cranePauseTicks = 0;

        public VehiclePawn blockedVehicle;

        bool cellOccupied;

        public bool CellOccupied
        {
            get
            {
                if (this.IsHashIntervalTick(300))
                {
                    RefreshOccupyCache();
                }
                return cellOccupied;
            }
        }

        public void RefreshOccupyCache()
        {
            cellOccupied = IsCellOccupied();
        }

        ModExtension_CraneTopData craneTopData => def.GetModExtension<ModExtension_CraneTopData>();

        System.Random rand;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            rand = new System.Random();
            RefreshOccupyCache();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref CurrentBill, "CurrentBill");
            Scribe_Values.Look(ref craneCurrentPct, "craneCurrentPct");
            Scribe_Values.Look(ref craneTargetPct, "craneTargetPct");
        }
        public override void DynamicDrawPhaseAt(DrawPhase phase, Vector3 drawLoc, bool flip = false)
        {
            base.DynamicDrawPhaseAt(phase, drawLoc, flip); CurrentBill?.ResultGraphic?.Draw(DrawPos + new Vector3(0, -0.01f, -0.2f), Rotation, this);
            if (phase == DrawPhase.Draw)
            {
                craneTopData?.graphicData.Graphic.Draw(DrawPos + craneTopData.GetDrawOffset(Rotation, craneCurrentPct) + Altitudes.AltIncVect, Rotation, this);
            }      
        }

        public bool IsCellOccupied()
        {
            //Log.Message("occupycheck");
            List<IntVec3> cells = GenAdj.CellsOccupiedBy(Position, Rotation, new IntVec2(def.size.x - 2, def.size.z - 2)).ToList();
            foreach (IntVec3 cell in cells)
            {
                if (cell.GetEdifice(Map) != null)
                {
                    return true;
                }
                var vehicle = cell.GetFirstThing<VehiclePawn>(Map);
                if (vehicle != null)
                {
                    blockedVehicle = vehicle;
                    return true;
                }
            }
            return false;
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }
            if (blockedVehicle != null)
            {
                Command_Action select = new Command_Action();
                select.defaultLabel = "RTC_CraneSelectVehicle".Translate();
                select.defaultDesc = "RTC_CraneSelectVehicleDesc".Translate();
                select.icon = blockedVehicle.VehicleGraphic.TexAt(Rotation);
                select.action = delegate
                {
                    Find.Selector.ClearSelection();
                    Find.Selector.Select(blockedVehicle);
                };
                select.hotKey = KeyBindingDefOf.Misc5;
                yield return select;
            }
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
                float speed = craneTopData.PctPerSec / 6000;
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
            RefreshOccupyCache();
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
            if (CurrentBill == bill)
            {
                CurrentBill = null;
            }
            RefreshOccupyCache();
        }
    }

    internal class ModExtension_CraneTopData : DefModExtension
    {
        public GraphicData graphicData;

        public CraneEndpoints EndpointSouth;

        public CraneEndpoints EndpointNorth;

        public CraneEndpoints EndpointEast;

        public CraneEndpoints EndpointWest;

        public float PctPerSec = 15f;

        public IntRange pauseTickRange = new IntRange(60, 300);

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
                    return Vector3.Lerp(EndpointNorth.endPointA, EndpointNorth.endPointB, pct);
                case 1:
                    return Vector3.Lerp(EndpointEast.endPointA, EndpointEast.endPointB, pct);
                case 2:
                    return Vector3.Lerp(EndpointSouth.endPointA, EndpointSouth.endPointB, pct);
                case 3:
                    return Vector3.Lerp(EndpointWest.endPointA, EndpointWest.endPointB, pct);
            }
            return Vector3.zero;
        }
    }

    internal class CraneEndpoints
    {
        public Vector3 endPointA;

        public Vector3 endPointB;
    }
}
