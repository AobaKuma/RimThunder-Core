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
    internal class PlaceWorker_Crane : PlaceWorker
    {
        private static IEnumerable<IntVec3> GetAdjacentCorners(CellRect rect)
        {
            yield return new IntVec3(rect.minX, 0, rect.minZ);
            yield return new IntVec3(rect.minX, 0, rect.maxZ);
            yield return new IntVec3(rect.maxX, 0, rect.maxZ);
            yield return new IntVec3(rect.maxX, 0, rect.minZ);
        }

        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
        {
            ModExtension_CranePlaceWorker ext = checkingDef.GetModExtension<ModExtension_CranePlaceWorker>();
            if (ext == null) { return true; }

            List<IntVec3> cells = GetAdjacentCorners(GenAdj.OccupiedRect(loc, rot, checkingDef.Size)).ToList();

            bool placeable = true;
            foreach (IntVec3 cell in cells)
            {
                if (!(cell.GetEdifice(map)?.def == ext.pillarDef))
                {
                    GhostDrawer.DrawGhostThing(cell, rot, ext.pillarDef, ext.pillarDef.graphic, Color.red, AltitudeLayer.MetaOverlays);
                    placeable = false;
                }
                else
                {
                    GhostDrawer.DrawGhostThing(cell, rot, ext.pillarDef, ext.pillarDef.graphic, Color.green, AltitudeLayer.MetaOverlays);
                }
            }
            return placeable;
        }
    }

    internal class ModExtension_CranePlaceWorker : DefModExtension
    {
        public ThingDef pillarDef;
    }
}
