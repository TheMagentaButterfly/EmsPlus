using EmsPlus.Configuration;
using EmsPlus.Managers;
using RAGENativeUI;
using RAGENativeUI.Elements;
using System.Collections.Generic;

namespace EmsPlus.UI.NativeMenus
{
    public static class AmbulanceMenuBuilder
    {
        private static UIMenuItem itemDoors, itemLoad, itemUnload, itemStore, itemCabinToggle;
        private static UIMenuListItem itemEquipment;

        private static List<KitDefinition> _cachedKits = new List<KitDefinition>();

        public static void Build()
        {
            MenuCore.AmbulanceMenu = new UIMenu(Localization.Get("MENU_AMBULANCE_TITLE"), Localization.Get("MENU_AMBULANCE_SUBTITLE"));
            MenuCore.AddMenu(MenuCore.AmbulanceMenu);

            itemDoors = new UIMenuItem(Localization.Get("ITEM_TOGGLE_DOORS"), Localization.Get("DESC_TOGGLE_DOORS"));
            itemLoad = new UIMenuItem(Localization.Get("ITEM_LOAD_STRETCHER"), Localization.Get("DESC_LOAD_STRETCHER"));
            itemUnload = new UIMenuItem(Localization.Get("ITEM_UNLOAD_STRETCHER"), Localization.Get("DESC_UNLOAD_STRETCHER"));
            itemStore = new UIMenuItem($"~r~{Localization.Get("ITEM_STORE_CURRENT")}", Localization.Get("DESC_STORE_CURRENT"));
            itemCabinToggle = new UIMenuItem(Localization.Get("ITEM_ENTER_CABIN"), Localization.Get("DESC_ENTER_CABIN"));

            _cachedKits = EntryPoint.KitConfig.Definitions;
            List<dynamic> kitNames = new List<dynamic>();

            foreach (var kit in _cachedKits)
            {
                kitNames.Add(kit.Name);
            }

            if (kitNames.Count == 0) kitNames.Add(Localization.Get("ITEM_NONE_FOUND"));
            string initialDesc = _cachedKits.Count > 0 ? _cachedKits[0].Description : Localization.Get("DESC_NO_EQUIPMENT");

            itemEquipment = new UIMenuListItem(Localization.Get("ITEM_MEDICAL_EQUIPMENT"), kitNames, 0, initialDesc);

            MenuCore.AmbulanceMenu.OnListChange += (s, item, index) =>
            {
                if (item == itemEquipment && index < _cachedKits.Count)
                {
                    itemEquipment.Description = _cachedKits[index].Description;
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
                    if (itemEquipment.Index < _cachedKits.Count)
                    {
                        var selectedKit = _cachedKits[itemEquipment.Index];
                        InventoryManager.EquipKit(selectedKit.ID);
                        RefreshState();
                    }
                }
                else if (item == itemStore)
                {
                    InventoryManager.StowKit();
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

            if (inVehicleSeat && !inRearCabin)
            {
                UIMenuItem warningItem = new UIMenuItem("", Localization.Get("DESC_EXIT_VEHICLE"));
                warningItem.Enabled = false;
                MenuCore.AmbulanceMenu.AddItem(warningItem);
                MenuCore.AmbulanceMenu.RefreshIndex();
                return;
            }

            // 2. REAR CABIN LOGIC (Player is sitting in the back)
            if (inRearCabin)
            {
                itemCabinToggle.Text = Localization.Get("ITEM_EXIT_CABIN");
                itemCabinToggle.Description = Localization.Get("DESC_EXIT_CABIN");
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

                // 1. DOORS
                itemDoors.Text = doorsOpen ? Localization.Get("ITEM_CLOSE_REAR_DOORS") : Localization.Get("ITEM_OPEN_REAR_DOORS");
                itemDoors.Enabled = true;
                itemDoors.Description = Localization.Get("DESC_TOGGLE_DOORS");
                MenuCore.AmbulanceMenu.AddItem(itemDoors);

                // 2. STRETCHER LOGIC
                if (doorsOpen)
                {
                    if (atRear)
                    {
                        if (isLoaded)
                        {
                            itemUnload.Enabled = true;
                            itemUnload.Description = Localization.Get("DESC_UNLOAD_STRETCHER");
                            itemUnload.SetRightBadge(UIMenuItem.BadgeStyle.None);
                            MenuCore.AmbulanceMenu.AddItem(itemUnload);
                        }
                        else
                        {
                            if (isHolding)
                            {
                                itemLoad.Enabled = true;
                                itemLoad.Description = Localization.Get("DESC_LOAD_STRETCHER");
                            }
                            else
                            {
                                itemLoad.Enabled = false;
                                itemLoad.Description = Localization.Get("DESC_MUST_HOLD_STRETCHER");
                            }
                            MenuCore.AmbulanceMenu.AddItem(itemLoad);
                        }
                    }
                }
                if (isLoaded)
                {
                    itemCabinToggle.Text = Localization.Get("ITEM_ENTER_CABIN");
                    itemCabinToggle.Description = Localization.Get("DESC_ENTER_CABIN");
                    MenuCore.AmbulanceMenu.AddItem(itemCabinToggle);
                }

                // 3. EQUIPMENT
                itemEquipment.Enabled = true;
                MenuCore.AmbulanceMenu.AddItem(itemEquipment);

                // 4. STORE KITS
                bool hasKitInHand = InventoryManager.CurrentKitID != "NONE";
                bool hasKitsOnGround = InventoryManager.PlacedKits.Count > 0;

                if (hasKitInHand || hasKitsOnGround)
                {
                    itemStore.Enabled = true;
                    itemStore.Description = Localization.Get("DESC_COLLECT_KITS");
                    MenuCore.AmbulanceMenu.AddItem(itemStore);
                }
            }

            MenuCore.AmbulanceMenu.RefreshIndex();
        }
    }
}