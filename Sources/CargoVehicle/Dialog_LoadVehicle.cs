using RimWorld.Planet;
using RimWorld;
using SmashTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Vehicles;
using Verse;
using Verse.Sound;

namespace VehicleLoadCargo
{
    // This is the loading dialogue for the Avenger
    // It's pretty much a standard loading dialog with vehicles added, so I'm not sure if should be an entire copy of the dialog ratherthan just adjust the item population to include vehicles
    // But I am sure I don't want to re-write from scratch, plus I might be missing something non-obvious.

    public class Dialog_LoadVehicle : Window
    {
        private VehiclePawn vehicle;

        private List<TransferableOneWay> transferables = new List<TransferableOneWay>();

        private TransferableOneWayWidget itemsTransfer;

        private static CargoVehicleSettings modSettings = null;

        public static CargoVehicleSettings ModSettings
        {
            get
            {
                if (modSettings == null)
                {
                    modSettings = LoadedModManager.GetMod<CargoVehicleMod>().GetSettings<CargoVehicleSettings>();
                }
                return modSettings;
            }
        }

        private bool massUsageDirty;

        private float cachedMassUsage;

        private readonly Vector2 BottomButtonSize = new Vector2(160f, 40f);

        private static readonly List<Pair<float, Color>> MassColor = new List<Pair<float, Color>>
    {
        new Pair<float, Color>(0.1f, Color.green),
        new Pair<float, Color>(0.75f, Color.yellow),
        new Pair<float, Color>(1f, new Color(1f, 0.6f, 0f))
    };

        public override Vector2 InitialSize => new Vector2(1024f, UI.screenHeight);

        public float MassUsage
        {
            get
            {
                if (massUsageDirty)
                {
                    massUsageDirty = false;
                    cachedMassUsage = CollectionsMassCalculator.MassUsageTransferables(transferables, IgnorePawnsInventoryMode.IgnoreIfAssignedToUnloadOrPlayerPawn, includePawnsMass: true);
                    cachedMassUsage += MassUtility.GearAndInventoryMass(vehicle);
                }
                return cachedMassUsage;
            }
        }

        public float MassCapacity => vehicle.GetStatValue(VehicleStatDefOf.CargoCapacity);

        public override void PostClose()
        {
            base.PostClose();
        }

        public override void DoWindowContents(Rect inRect)
        {
            Rect rect = new Rect(0f, 0f, inRect.width, 35f);
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(rect, vehicle.LabelShortCap);
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;
            DrawCargoNumbers(new Rect(12f, 35f, inRect.width - 24f, 40f));
            Rect rect2 = new Rect(inRect.width - 225f, 35f, 225f, 40f);
            bool showAllCargoItems = VehicleMod.settings.showAllCargoItems;
            string text = "VF_ShowAllItemsOnMap".Translate();
            Widgets.Label(rect2, text);
            rect2.x += Text.CalcSize(text).x + 20f;
            Widgets.Checkbox(new Vector2(rect2.x, rect2.y), ref VehicleMod.settings.showAllCargoItems);
            if (showAllCargoItems != VehicleMod.settings.showAllCargoItems)
            {
                CalculateAndRecacheTransferables();
            }

            inRect.yMin += 60f;
            Widgets.DrawMenuSection(inRect);
            inRect = inRect.ContractedBy(17f);
            Widgets.BeginGroup(inRect);
            Rect rect3 = inRect.AtZero();
            BottomButtons(rect3);
            Rect inRect2 = rect3;
            inRect2.yMax -= 76f;
            itemsTransfer.OnGUI(inRect2, out var anythingChanged);
            if (anythingChanged)
            {
                CountToTransferChanged();
            }

            Widgets.EndGroup();
        }

        public void BottomButtons(Rect rect)
        {
            Rect rect2 = new Rect(rect.width / 2f - BottomButtonSize.x / 2f, rect.height - 55f - 17f, BottomButtonSize.x, BottomButtonSize.y);
            if (Widgets.ButtonText(rect2, "AcceptButton".Translate()))
            {
                int totalTiles = 0;
                List<TransferableOneWay> cargoToLoad = transferables.Where((TransferableOneWay t) => t.CountToTransfer > 0).ToList();
                foreach (TransferableOneWay item in cargoToLoad)
                {
                    foreach (Thing thing3 in item.things)
                    {
                        if (thing3 is VehiclePawn)
                        {
                            totalTiles += thing3.def.size.Area;
                        }
                    }
                }
                foreach (Thing thing2 in vehicle.inventory.innerContainer)
                {
                    if (thing2 is VehiclePawn)
                    {
                        totalTiles += thing2.def.size.Area;
                    }
                }
                int vehicleMaxSize = ModSettings.maxShipSizeX * ModSettings.maxShipSizeY;
                if (totalTiles > vehicleMaxSize)
                {
                    SoundDefOf.Tick_Low.PlayOneShotOnCamera();
                    Messages.Message("Total tiles used by vehicles is greater than " + vehicleMaxSize, MessageTypeDefOf.RejectInput, historical: false);
                    return;
                }
                vehicle.cargoToLoad = cargoToLoad;
                ComponentCache.GetCachedMapComponent<VehicleReservationManager>(vehicle.Map).RegisterLister(vehicle, "LoadVehicle");
                SoundDefOf.Tick_High.PlayOneShotOnCamera();
                Close();
            }
            if (Widgets.ButtonText(new Rect(rect2.x - 10f - BottomButtonSize.x, rect2.y, BottomButtonSize.x, BottomButtonSize.y), "ResetButton".Translate()))
            {
                SoundDefOf.Tick_Low.PlayOneShotOnCamera();
                CalculateAndRecacheTransferables();
            }
            if (Widgets.ButtonText(new Rect(rect2.xMax + 10f, rect2.y, BottomButtonSize.x, BottomButtonSize.y), "CancelButton".Translate()))
            {
                Close();
            }
            if (!Prefs.DevMode)
            {
                return;
            }
            float width = 200f;
            float num = BottomButtonSize.y / 2f;
            if (Widgets.ButtonText(new Rect(0f, rect.height - 55f - 17f, width, num), "Dev: Pack Instantly"))
            {
                SoundDefOf.Tick_High.PlayOneShotOnCamera();
                for (int i = 0; i < transferables.Count; i++)
                {
                    List<Thing> things = transferables[i].things;
                    int countToTransfer = transferables[i].CountToTransfer;
                    Action<Thing, IThingHolder> transferred = delegate (Thing thing, IThingHolder originalHolder)
                    {
                        vehicle.AddOrTransfer(thing);
                    };
                    TransferableUtility.Transfer(things, countToTransfer, transferred);
                }
                Close(doCloseSound: false);
            }
            if (Widgets.ButtonText(new Rect(0f, rect.height - 55f - 17f + num, width, num), "Dev: Select everything"))
            {
                SoundDefOf.Tick_High.PlayOneShotOnCamera();
                SetToSendEverything();
            }
        }

        public void DrawCargoNumbers(Rect rect)
        {
            Color color = ((MassUsage > MassCapacity) ? Color.red : ((MassCapacity != 0f) ? GenUI.LerpColor(MassColor, MassUsage / MassCapacity) : Color.grey));
            Color color2 = GUI.color;
            GUI.color = color;
            string label = string.Format("{0}: {1}/{2}", "Mass".Translate(), MassUsage, MassCapacity);
            Widgets.Label(rect, label);
            GUI.color = color2;
        }

        private void AddToTransferables(Thing t, bool setToTransferMax = false)
        {
            TransferableOneWay transferableOneWay = TransferableUtility.TransferableMatching(t, transferables, TransferAsOneMode.PodsOrCaravanPacking);
            if (transferableOneWay == null)
            {
                transferableOneWay = new TransferableOneWay();
                transferables.Add(transferableOneWay);
            }
            if (transferableOneWay.things.Contains(t))
            {
                Log.Error("Tried to add the same thing twice to TransferableOneWay: " + t);
                return;
            }
            transferableOneWay.things.Add(t);
            if (setToTransferMax)
            {
                transferableOneWay.AdjustTo(transferableOneWay.CountToTransfer + t.stackCount);
            }
        }

        private void CalculateAndRecacheTransferables()
        {
            transferables = new List<TransferableOneWay>();
            AddItemsToTransferables();
            itemsTransfer = new TransferableOneWayWidget(transferables, null, null, null, drawMass: true, IgnorePawnsInventoryMode.IgnoreIfAssignedToUnload, includePawnsMassInMassUsage: false, () => MassCapacity - MassUsage);
            CountToTransferChanged();
        }

        private void SetToSendEverything()
        {
            for (int i = 0; i < transferables.Count; i++)
            {
                transferables[i].AdjustTo(transferables[i].GetMaximumToTransfer());
            }
            CountToTransferChanged();
        }

        private void CountToTransferChanged()
        {
            massUsageDirty = true;
        }

        private void AddItemsToTransferables()
        {
            // Start by adding any vehicles with no pawns in them to the list 
            List<Pawn> list = (List<Verse.Pawn>)Find.CurrentMap.mapPawns.AllPawnsSpawned;
            for (int j = 0; j < list.Count; j++)
            {
                if (list[j] is VehiclePawn pawn && pawn.AllPawnsAboard.Count == 0 && pawn != vehicle)
                {

                    if ((list[j].def.size.x <= ModSettings.maxShipSizeX && list[j].def.size.z <= ModSettings.maxShipSizeY) || (list[j].def.size.z <= ModSettings.maxShipSizeX && list[j].def.size.x <= ModSettings.maxShipSizeY))
                    {

                        AddToTransferables(list[j]);
                    }
                }
            }
            //Then add normal caravan stuff to the list
            List<Thing> itemList = CaravanFormingUtility.AllReachableColonyItems(vehicle.Map, VehicleMod.settings.showAllCargoItems);
            for (int i = 0; i < itemList.Count; i++)
            {
                AddToTransferables(itemList[i]);
            }
        }

        public override void PostOpen()
        {
            base.PostOpen();
            massUsageDirty = true;
            CalculateAndRecacheTransferables();
        }

        public Dialog_LoadVehicle(VehiclePawn vehicle)
        {
            this.vehicle = vehicle;
            closeOnAccept = true;
            closeOnCancel = true;
            forcePause = false;
            absorbInputAroundWindow = true;
        }
    }
}
