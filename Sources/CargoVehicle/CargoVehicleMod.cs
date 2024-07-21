using UnityEngine;
using Verse;

namespace VehicleLoadCargo
{
    public class CargoVehicleMod : Mod
    {
        readonly CargoVehicleSettings settings;

        public CargoVehicleMod(ModContentPack content) : base(content)
        {
            settings = GetSettings<CargoVehicleSettings>();
        }
        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();

            listingStandard.Begin(inRect);

            listingStandard.Label("VLC_maxShipSize".Translate(settings.maxShipSizeX, settings.maxShipSizeY));
            listingStandard.Label("VLC_XSlider".Translate());
            settings.maxShipSizeX = (int)listingStandard.Slider(settings.maxShipSizeX, 0, 100);
            listingStandard.Label("VLC_YSlider".Translate());
            settings.maxShipSizeY = (int)listingStandard.Slider(settings.maxShipSizeY, 0, 100);

            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "VLC_ModSettings".Translate();
        }
    }
}
