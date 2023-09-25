using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace Motorization
{
    internal class PlaceWorker_Crane : PlaceWorker
    {
        private static IEnumerable<IntVec3> GetAdjacentCorners(CellRect rect)
        {
            yield return new IntVec3(rect.minX - 1, 0, rect.minZ - 1);
            yield return new IntVec3(rect.minX - 1, 0, rect.maxZ + 1);
            yield return new IntVec3(rect.maxX + 1, 0, rect.maxZ + 1);
            yield return new IntVec3(rect.maxX + 1, 0, rect.minZ - 1);
        }

        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
        {
            ModExtension_CranePlaceWorker ext = checkingDef.GetModExtension<ModExtension_CranePlaceWorker>();
            if (ext != null) { return true; }

            List<IntVec3> cells = GetAdjacentCorners(GenAdj.OccupiedRect(loc, rot, checkingDef.Size)).ToList();

            bool placeable = true;
            foreach (IntVec3 cell in cells)
            {
                if (!(cell.GetEdifice(map)?.def == ext.pillarDef))
                {
                    placeable = false;
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
