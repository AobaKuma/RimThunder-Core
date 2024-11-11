using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using static Verse.Text;

namespace Motorization
{
    public class ITab_VehicleTransport : ITab
    {
        public CompVehicleCargo CargoComp
        {
            get
            {
                if (cacheCargo == null)
                {
                    cacheCargo = this.SelPawn.TryGetComp<CompVehicleCargo>();
                }
                return cacheCargo;
            }
            private set { }
        }
        private CompVehicleCargo cacheCargo;
        public ITab_VehicleTransport()
        {
            size = new Vector2(512f, 450f);
            labelKey = "RT_Cargo".Translate();
        }
        protected override void FillTab()
        {
            Text.Font = GameFont.Small;
            Rect rect = new Rect(Vector2.zero, size);
            Rect inner = rect.ContractedBy(3f);

            //Draw Name
            {
                string text = this.SelPawn.Label.Translate();
                Vector2 size = CalcSize(text);
                Vector2 slPosition = new Vector2(14f, inner.y);
                Rect slgizmoRect = new Rect(slPosition, size);
                Widgets.Label(slgizmoRect, text);
                Widgets.DrawLineHorizontal(inner.xMin + 9, inner.y - 75, inner.width - 12);
                foreach (Thing item in CargoComp.Cargo)
                {
                    DrawThingRow(item);
                }
            }

            void DrawThingRow(Thing thing)
            {

            }
        }
    }
}
