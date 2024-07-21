using Verse;

namespace VehicleLoadCargo
{
    public class CargoVehicleSettings : ModSettings
    {
        public int maxShipSizeX = 4;

        public int maxShipSizeY = 5;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref maxShipSizeX, "maxShipSizeX", 4);
            Scribe_Values.Look(ref maxShipSizeY, "maxShipSizeY", 5);
            base.ExposeData();
        }
    }
}
