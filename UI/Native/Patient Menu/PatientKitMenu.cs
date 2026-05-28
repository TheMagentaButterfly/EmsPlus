using EmsPlus.UI.Helpers;
using EmsPlus.Managers;
using EmsPlus.Managers.Actions;
using Rage;
using RAGENativeUI.Elements;
using EmsPlus.Core;

namespace EmsPlus.UI.Native.PatientMenu
{
    public static partial class PatientMenuBuilder
    {
        #region Kit Interaction Menu
        private static void BuildKitMenu()
        {
            KitMenu.Clear();
            var p = GameState.CurrentPatient;
            if (p == null) return;

            if (InventoryManager.PlacedKits.Count == 0)
            {
                var none = new UIMenuItem($"~c~{Localization.Get("ITEM_NO_KITS_NEARBY", "No Kits Nearby")}", Localization.Get("DESC_NO_KITS_NEARBY", "There are no kits nearby."));
                none.Enabled = false;
                KitMenu.AddItem(none);
                KitMenu.RefreshIndex();
                return;
            }

            foreach (var kit in InventoryManager.PlacedKits)
            {
                string defaultName = kit.KitID;
                string kitColor = "~p~";

                if (kit.KitID == "TRAUMABAG") { defaultName = "Trauma Bag"; kitColor = "~r~"; }
                else if (kit.KitID == "OXYGENBAG") { defaultName = "Oxygen Bag"; kitColor = "~b~"; }
                else if (kit.KitID == "DEFIBRILLATOR") { defaultName = "Defibrillator"; kitColor = "~g~"; }

                string kitName = Localization.Get($"{kit.KitID.ToUpperInvariant()}_NAME", defaultName);

                string pickUpText = Localization.Get("ACT_PICK_UP_KIT_FORMAT", "Pick Up {0}");
                string itemLabel = $"~p~{string.Format(pickUpText, kitColor + kitName)}";

                AddInteractiveItem(KitMenu, itemLabel, Localization.Get("DESC_PICK_UP_KIT", "Equip this item to your hands."), true, () => {
                    InventoryManager.PickupKit(kit.Prop);
                    AudioHelper.PlaySuccess();
                    BuildKitMenu(); 
                });

                if (kit.KitID == "DEFIBRILLATOR")
                {
                    AddMenuSeparator(KitMenu, Localization.Get("CAT_SEP_DEFIB", "~c~=== DEFIBRILLATOR ==="));

                    if (!p.IsEcgsConnected) { AddInteractiveItem(KitMenu, $"~g~{Localization.Get("ACT_ATTACH_MONITOR", "Attach Monitor")}", Localization.Get("DESC_ATTACH_MONITOR", "Connect ECG/SpO2 Leads"), true, () => { ActionsCore.Run(Localization.Get("ACT_ATTACHING_LEADS", "Attaching leads..."), 2000, EntryPoint.AnimationConfig.MedicTreatDict.Value, EntryPoint.AnimationConfig.MedicTreatName.Value, () => { p.IsEcgsConnected = true; Game.DisplayNotification(Localization.Get("NOTIF_MONITOR_CONNECTED", "~g~Monitor connected.")); }); MenuCore.CloseAll(); }); }
                    else { AddInteractiveItem(KitMenu, $"~y~{Localization.Get("ACT_REMOVE_MONITOR", "Remove Monitor")}", Localization.Get("DESC_REMOVE_MONITOR", "Disconnect ECG/SpO2 Leads"), true, () => { ActionsCore.Run(Localization.Get("ACT_REMOVING_LEADS", "Removing leads..."), 1500, EntryPoint.AnimationConfig.MedicTreatDict.Value, EntryPoint.AnimationConfig.MedicTreatName.Value, () => { p.IsEcgsConnected = false; Game.DisplayNotification(Localization.Get("NOTIF_MONITOR_DISCONNECTED", "~y~Monitor disconnected.")); }); MenuCore.CloseAll(); }); }

                    if (!p.IsBpCuffConnected) { AddInteractiveItem(KitMenu, $"~g~{Localization.Get("ACT_ATTACH_BP", "Attach BP Cuff")}", Localization.Get("DESC_ATTACH_BP_CUFF", "Auto-Cycle BP"), true, () => { ActionsCore.Run(Localization.Get("ACT_APPLYING_CUFF", "Applying cuff..."), 2000, EntryPoint.AnimationConfig.MedicTreatDict.Value, EntryPoint.AnimationConfig.MedicTreatName.Value, () => { p.IsBpCuffConnected = true; Game.DisplayNotification(Localization.Get("NOTIF_BP_CONNECTED", "~g~BP Cuff attached.")); }); MenuCore.CloseAll(); }); }
                    else { AddInteractiveItem(KitMenu, $"~y~{Localization.Get("ACT_REMOVE_BP", "Remove BP Cuff")}", Localization.Get("DESC_REMOVE_BP_CUFF", "Remove Cuff"), true, () => { ActionsCore.Run(Localization.Get("ACT_REMOVING_CUFF", "Removing cuff..."), 1500, EntryPoint.AnimationConfig.MedicTreatDict.Value, EntryPoint.AnimationConfig.MedicTreatName.Value, () => { p.IsBpCuffConnected = false; Game.DisplayNotification(Localization.Get("NOTIF_BP_REMOVED", "~y~BP Cuff removed.")); }); MenuCore.CloseAll(); }); }
                }
            }

            KitMenu.RefreshIndex();
        }
        #endregion
    }
}