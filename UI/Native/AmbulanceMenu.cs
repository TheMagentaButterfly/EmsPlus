using EmsPlus.Managers;
using RAGENativeUI;
using RAGENativeUI.Elements;
using System.Collections.Generic;

namespace EmsPlus.UI.Native
{
    public static class AmbulanceMenu
    {
        private static UIMenuItem itemDoors, itemLoad, itemUnload, itemStore, itemCabinToggle;
        private static UIMenuListItem itemEquipment;

        private static List<string> _cachedKitIDs = new List<string> { "TRAUMABAG", "OXYGENBAG", "DEFIBRILLATOR" };
        private static List<string> _cachedKitDescs = new List<string>();

        public static void Build()
        {
            MenuCore.AmbulanceMenu = new UIMenu(Localization.Get("MENU_AMBULANCE_TITLE", "Ambulance"), Localization.Get("MENU_AMBULANCE_SUBTITLE", "Manage your ambulance"));
            MenuCore.AddMenu(MenuCore.AmbulanceMenu);

            itemDoors = new UIMenuItem(Localization.Get("ITEM_TOGGLE_DOORS", "Toggle Doors"), Localization.Get("DESC_TOGGLE_DOORS", "Open or close the ambulance doors"));
            itemLoad = new UIMenuItem(Localization.Get("ITEM_LOAD_STRETCHER", "Load Stretcher"), Localization.Get("DESC_LOAD_STRETCHER", "Load a stretcher into the ambulance"));
            itemUnload = new UIMenuItem(Localization.Get("ITEM_UNLOAD_STRETCHER", "Unload Stretcher"), Localization.Get("DESC_UNLOAD_STRETCHER", "Unload a stretcher from the ambulance"));
            itemStore = new UIMenuItem($"~r~{Localization.Get("ITEM_STORE_CURRENT", "Store Current")}", Localization.Get("DESC_STORE_CURRENT", "Store the current item"));
            itemCabinToggle = new UIMenuItem(Localization.Get("ITEM_ENTER_CABIN", "Enter Cabin"), Localization.Get("DESC_ENTER_CABIN", "Enter or exit the ambulance cabin"));

            List<dynamic> kitNames = new List<dynamic>
            {
                $"~r~{Localization.Get("TRAUMABAG_NAME", "Trauma Bag")}",
                $"~b~{Localization.Get("OXYGENBAG_NAME", "Oxygen Bag")}",
                $"~g~{Localization.Get("DEFIBRILLATOR_NAME", "Defibrillator")}"
            };

            _cachedKitDescs = new List<string>
            {
                Localization.Get("TRAUMABAG_DESC", "A bag containing trauma supplies."),
                Localization.Get("OXYGENBAG_DESC", "A bag containing oxygen supplies."),
                Localization.Get("DEFIBRILLATOR_DESC", "A device used to deliver an electric shock to the heart.")
            };

            string initialDesc = _cachedKitDescs[0];

            itemEquipment = new UIMenuListItem(Localization.Get("ITEM_MEDICAL_EQUIPMENT", "Medical Equipment"), kitNames, 0, initialDesc);

            MenuCore.AmbulanceMenu.OnListChange += (s, item, index) =>
            {
                if (item == itemEquipment && index < _cachedKitDescs.Count)
                {
                    itemEquipment.Description = _cachedKitDescs[index];
                }
            };

            MenuCore.AmbulanceMenu.OnItemSelect += (s, item, i) =>
            {
                if (Rage.Game.LocalPlayer.Character.IsInAnyVehicle(false)) return;

                if (item == itemDoors)
                {
                    AmbulanceManager.ToggleDoors();
                }
                else if (item == itemCabinToggle)
                {
                    MenuCore.CloseAll();
                    if (AmbulanceManager.IsPlayerInRearCabin) AmbulanceManager.ExitRearCabin();
                    else AmbulanceManager.EnterRearCabin();
                }
                else if (item == itemLoad)
                {
                    AmbulanceManager.LoadStretcher();
                }
                else if (item == itemUnload)
                {
                    AmbulanceManager.UnloadStretcher();
                }
                else if (item == itemEquipment)
                {
                    if (itemEquipment.Index < _cachedKitIDs.Count)
                    {
                        var selectedKit = _cachedKitIDs[itemEquipment.Index];
                        InventoryManager.EquipKit(selectedKit);
                        RefreshState();
                    }
                }
                else if (item == itemStore)
                {
                    InventoryManager.StowAllKits();
                    InventoryManager.StoreAllKits();
                    RefreshState();
                }
            };

            MenuCore.AmbulanceMenu.OnMenuOpen += (s) => RefreshState();

            RefreshState();
        }

        public static void RefreshState()
        {
            MenuCore.AmbulanceMenu.Clear();

            bool inRearCabin = AmbulanceManager.IsPlayerInRearCabin;
            bool inVehicleSeat = Rage.Game.LocalPlayer.Character.IsInAnyVehicle(false);

            if (Rage.Game.LocalPlayer != null && Rage.Game.LocalPlayer.Character != null && Rage.Game.LocalPlayer.Character.Exists())
            {
                inVehicleSeat = Rage.Game.LocalPlayer.Character.IsInAnyVehicle(false);
            }

            if (inVehicleSeat && !inRearCabin)
            {
                UIMenuItem warningItem = new UIMenuItem("", Localization.Get("DESC_EXIT_VEHICLE", "You must exit the vehicle to access this menu."));
                warningItem.Enabled = false;
                MenuCore.AmbulanceMenu.AddItem(warningItem);
                MenuCore.AmbulanceMenu.RefreshIndex();
                return;
            }

            if (inRearCabin)
            {
                itemCabinToggle.Text = Localization.Get("ITEM_EXIT_CABIN", "Exit Cabin");
                itemCabinToggle.Description = Localization.Get("DESC_EXIT_CABIN", "Exit the ambulance cabin.");

                if (AmbulanceManager.CurrentConfig != null && !AmbulanceManager.CurrentConfig.CanEnterCabin)
                {
                    itemCabinToggle.Enabled = false;
                    itemCabinToggle.Description = Localization.Get("ERR_NO_CABIN", "This vehicle does not have an accessible cabin.");
                }
                else
                {
                    itemCabinToggle.Enabled = true;
                }

                MenuCore.AmbulanceMenu.AddItem(itemCabinToggle);

                itemEquipment.Enabled = true;
                MenuCore.AmbulanceMenu.AddItem(itemEquipment);

                MenuCore.AmbulanceMenu.RefreshIndex();
                return;
            }
            else
            {
                bool isHolding = StretcherManager.IsAttachedToPlayer;
                bool isLoaded = AmbulanceManager.IsStretcherLoaded;
                bool doorsOpen = AmbulanceManager.AreDoorsOpen;
                bool atRear = AmbulanceManager.IsPlayerAtRear();

                itemDoors.Text = doorsOpen ? Localization.Get("ITEM_CLOSE_REAR_DOORS", "Close Rear Doors") : Localization.Get("ITEM_OPEN_REAR_DOORS", "Open Rear Doors");
                itemDoors.Enabled = true;
                itemDoors.Description = Localization.Get("DESC_TOGGLE_DOORS", "Toggle the rear doors.");
                MenuCore.AmbulanceMenu.AddItem(itemDoors);

                if (doorsOpen)
                {
                    if (atRear)
                    {
                        if (isLoaded)
                        {
                            itemUnload.Enabled = true;
                            itemUnload.Description = Localization.Get("DESC_UNLOAD_STRETCHER", "Unload the stretcher.");
                            itemUnload.SetRightBadge(UIMenuItem.BadgeStyle.None);
                            MenuCore.AmbulanceMenu.AddItem(itemUnload);
                        }
                        else
                        {
                            if (isHolding)
                            {
                                itemLoad.Enabled = true;
                                itemLoad.Description = Localization.Get("DESC_LOAD_STRETCHER", "Load the stretcher.");
                            }
                            else
                            {
                                itemLoad.Enabled = false;
                                itemLoad.Description = Localization.Get("DESC_MUST_HOLD_STRETCHER", "You must hold the stretcher to load it.");
                            }
                            MenuCore.AmbulanceMenu.AddItem(itemLoad);
                        }
                    }
                }
                if (isLoaded)
                {
                    itemCabinToggle.Text = Localization.Get("ITEM_ENTER_CABIN", "Enter Cabin");
                    itemCabinToggle.Description = Localization.Get("DESC_ENTER_CABIN", "Enter the ambulance cabin.");
                    MenuCore.AmbulanceMenu.AddItem(itemCabinToggle);
                }

                itemEquipment.Enabled = true;
                MenuCore.AmbulanceMenu.AddItem(itemEquipment);

                bool hasKitInHand = InventoryManager.EquippedKits.Count > 0;
                bool hasKitsOnGround = InventoryManager.PlacedKits.Count > 0;

                if (hasKitInHand || hasKitsOnGround)
                {
                    itemStore.Enabled = true;
                    itemStore.Description = Localization.Get("DESC_STORE_KITS", "Store all kits.");
                    MenuCore.AmbulanceMenu.AddItem(itemStore);
                }
            }

            MenuCore.AmbulanceMenu.RefreshIndex();
        }
    }
}