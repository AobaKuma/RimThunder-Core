using Verse;


namespace Motorization
{
    public interface IWeaponUsable
    {
        void Equip(ThingWithComps equipment);

        void Wear(ThingWithComps equipment);
    }
}
