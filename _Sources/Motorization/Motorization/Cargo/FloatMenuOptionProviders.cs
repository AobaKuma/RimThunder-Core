using RimWorld;
using Vehicles;
using Verse;

namespace Motorization
{
    /// <summary>
    /// 選中具有 CompVehicleCargo 的載具(拖車本身，或帶貨艙的拖曳車)，右鍵點擊地圖上另一台載具時，
    /// 提供「裝載該載具」的選項。取代舊有 FloatMenuUtility.GetExtraFloatMenuOptionsForCarrier 的載具分支。
    /// </summary>
    public class FloatMenuOptionProvider_LoadVehicleIntoCargo : FloatMenuOptionProvider_Vehicle
    {
        protected override bool Multiselect => false;

        protected override bool SelectedVehicleValid(VehiclePawn vehicle, FloatMenuContext context)
        {
            return vehicle.GetComp<CompVehicleCargo>() != null;
        }

        protected override FloatMenuOption GetSingleOptionFor(Pawn clickedPawn, FloatMenuContext context)
        {
            if (!(clickedPawn is VehiclePawn target)) return null;
            if (!(context.FirstSelectedPawn is VehiclePawn carrier) || target == carrier) return null;

            return FloatMenuUtility.TryMakeFloatMenuForCargoLoad(carrier, target);
        }
    }

    /// <summary>
    /// 選中已裝載貨物的拖車(VehiclePawn_Trailer)時，提供「卸下貨物」選項(沒有貨物則顯示提示訊息)。
    /// 取代舊有 GetExtraFloatMenuOptionsForCarrier 內的自行解除連接分支。
    /// </summary>
    public class FloatMenuOptionProvider_UnloadCargoVehicle : FloatMenuOptionProvider_Vehicle
    {
        protected override bool Multiselect => false;

        protected override bool SelectedVehicleValid(VehiclePawn vehicle, FloatMenuContext context)
        {
            return vehicle is VehiclePawn_Trailer trailer && trailer.HasCargoComp;
        }

        protected override FloatMenuOption GetSingleOption(FloatMenuContext context)
        {
            if (!(context.FirstSelectedPawn is VehiclePawn_Trailer trailer)) return null;

            if (trailer.CargoComp.HasPayload)
            {
                VehiclePawn target = trailer.CargoComp.GetPayload;
                return new FloatMenuOption("RTC_Unload".Translate(target.Label.Translate()), () =>
                {
                    trailer.CargoComp.TryUnloadThingUnspawned(trailer, target);
                });
            }
            return new FloatMenuOption("RTC_NoPayload".Translate(), null);
        }
    }

    /// <summary>
    /// 選中拖曳車(Tractor)，右鍵點擊地圖上的拖車(Trailer)時，提供「掛載該拖車」的選項。
    /// 取代舊有 FloatMenuUtility.GetExtraFloatMenuOptionsForTractor。
    /// </summary>
    public class FloatMenuOptionProvider_ConnectTrailer : FloatMenuOptionProvider_Vehicle
    {
        protected override bool Multiselect => false;

        protected override bool SelectedVehicleValid(VehiclePawn vehicle, FloatMenuContext context)
        {
            return vehicle is VehiclePawn_Tractor tractor && tractor.GetComp<CompTrailerMount>() != null;
        }

        protected override FloatMenuOption GetSingleOptionFor(Pawn clickedPawn, FloatMenuContext context)
        {
            if (!(clickedPawn is VehiclePawn_Trailer target)) return null;
            if (!(context.FirstSelectedPawn is VehiclePawn_Tractor tractor) || (Pawn)target == (Pawn)tractor) return null;

            return FloatMenuUtility.TryMakeFloatMenuForTrailerLoad(tractor, target);
        }
    }

    /// <summary>
    /// 選中已掛載拖車、且該拖車本身帶貨艙的拖曳車(Tractor)，右鍵點擊地圖上其他載具時，
    /// 提供「透過拖車貨艙裝載該載具」的選項(實際由拖曳車自己驅動，對應JobDriver_LoadToSelf)。
    /// 取代舊有 FloatMenuUtility.TryMakeFloatMenuForActiveLoad(tractor, sq) 的載具分支。
    /// </summary>
    public class FloatMenuOptionProvider_ActiveLoadViaTrailer : FloatMenuOptionProvider_Vehicle
    {
        protected override bool Multiselect => false;

        protected override bool SelectedVehicleValid(VehiclePawn vehicle, FloatMenuContext context)
        {
            return vehicle is VehiclePawn_Tractor tractor &&
                tractor.GetComp<CompTrailerMount>() != null &&
                tractor.TrailerMount.TryGetTrailer(out var trailer) &&
                trailer.HasCargoComp;
        }

        protected override FloatMenuOption GetSingleOptionFor(Pawn clickedPawn, FloatMenuContext context)
        {
            if (!(clickedPawn is VehiclePawn target)) return null;
            if (!(context.FirstSelectedPawn is VehiclePawn_Tractor tractor) || target == tractor) return null;

            return FloatMenuUtility.TryMakeFloatMenuForActiveLoad(tractor, target);
        }
    }

    /// <summary>
    /// 選中已掛載拖車、且該拖車帶貨艙並已裝載貨物的拖曳車(Tractor)，點擊拖曳車本身時，
    /// 提供「卸下拖車內貨物」選項。取代舊有 TryMakeFloatMenuForActiveLoad(IEnumerable) 內的 RTC_Unload 分支。
    /// </summary>
    public class FloatMenuOptionProvider_UnloadViaTrailer : FloatMenuOptionProvider_Vehicle
    {
        protected override bool Multiselect => false;

        protected override bool CanSelfTarget => true;

        protected override bool SelectedVehicleValid(VehiclePawn vehicle, FloatMenuContext context)
        {
            return vehicle is VehiclePawn_Tractor tractor &&
                tractor.GetComp<CompTrailerMount>() != null &&
                tractor.TrailerMount.TryGetTrailer(out var trailer) &&
                trailer.HasCargoComp && trailer.CargoComp.HasPayload;
        }

        protected override FloatMenuOption GetSingleOptionFor(Pawn clickedPawn, FloatMenuContext context)
        {
            if (!(context.FirstSelectedPawn is VehiclePawn_Tractor tractor) || clickedPawn != tractor) return null;
            if (!tractor.TrailerMount.TryGetTrailer(out var trailer)) return null;

            VehiclePawn target = trailer.CargoComp.GetPayload;
            return new FloatMenuOption("RTC_Unload".Translate(target.Label.Translate()), () =>
            {
                trailer.CargoComp.TryUnloadThingUnspawned(tractor, target);
            });
        }
    }

    /// <summary>
    /// 選中已掛載拖車的拖曳車(Tractor)，點擊拖曳車本身時，提供「解除掛載拖車」選項。
    /// 取代舊有 TryMakeFloatMenuForActiveLoad(IEnumerable) 內的 RTC_Disconnect 分支。
    /// </summary>
    public class FloatMenuOptionProvider_DisconnectTrailer : FloatMenuOptionProvider_Vehicle
    {
        protected override bool Multiselect => false;

        protected override bool CanSelfTarget => true;

        protected override bool SelectedVehicleValid(VehiclePawn vehicle, FloatMenuContext context)
        {
            return vehicle is VehiclePawn_Tractor tractor &&
                tractor.GetComp<CompTrailerMount>() != null &&
                tractor.TrailerMount.TryGetTrailer(out _);
        }

        protected override FloatMenuOption GetSingleOptionFor(Pawn clickedPawn, FloatMenuContext context)
        {
            if (!(context.FirstSelectedPawn is VehiclePawn_Tractor tractor) || clickedPawn != tractor) return null;
            if (!tractor.TrailerMount.TryGetTrailer(out var trailer)) return null;

            return new FloatMenuOption("RTC_Disconnect".Translate(trailer.Label.Translate()), () =>
            {
                tractor.TrailerMount.TryDisconnectTrailer(trailer);
            });
        }
    }
}
