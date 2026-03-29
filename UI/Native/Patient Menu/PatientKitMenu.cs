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
                var none = new UIMenuItem(Localization.Get("ITEM_NO_KITS_NEARBY"), Localization.Get("DESC_NO_KITS_NEARBY"));
                none.Enabled = false;
                KitMenu.AddItem(none);
                KitMenu.RefreshIndex();
                return;
            }

            foreach (var kit in InventoryManager.PlacedKits)
            {
                string kitName = Localization.Get($"KIT_NAME_{kit.KitID.ToUpperInvariant()}");

                // Pick Up Option
                AddInteractiveItem(KitMenu, string.Format(Localization.Get("ITEM_PICK_UP_KIT_FORMAT"), kitName), Localization.Get("DESC_PICK_UP_KIT"), true, () => {
                    InventoryManager.PickupKit(kit.Prop);
                    AudioHelper.PlaySuccess();
                    BuildKitMenu(); // Refresh
                });

                // Defib Specifics
                if (kit.KitID == "DEFIBRILLATOR")
                {
                    if (!p.IsEcgsConnected) { AddInteractiveItem(KitMenu, Localization.Get("ITEM_ATTACH_MONITOR"), Localization.Get("DESC_ATTACH_MONITOR"), true, () => { ActionsCore.Run(Localization.Get("ACT_ATTACHING_LEADS"), 2000, EntryPoint.AnimationConfig.MedicTreatDict.Value, EntryPoint.AnimationConfig.MedicTreatName.Value, () => { p.IsEcgsConnected = true; Game.DisplayNotification(Localization.Get("NOTIF_MONITOR_CONNECTED")); }); MenuCore.CloseAll(); }); }
                    else { AddInteractiveItem(KitMenu, Localization.Get("ITEM_REMOVE_MONITOR"), Localization.Get("DESC_REMOVE_MONITOR"), true, () => { ActionsCore.Run(Localization.Get("ACT_REMOVING_LEADS"), 1500, EntryPoint.AnimationConfig.MedicTreatDict.Value, EntryPoint.AnimationConfig.MedicTreatName.Value, () => { p.IsEcgsConnected = false; Game.DisplayNotification(Localization.Get("NOTIF_MONITOR_DISCONNECTED")); }); MenuCore.CloseAll(); }); }

                    if (!p.IsBpCuffConnected) { AddInteractiveItem(KitMenu, Localization.Get("ITEM_ATTACH_BP_CUFF"), Localization.Get("DESC_ATTACH_BP_CUFF"), true, () => { ActionsCore.Run(Localization.Get("ACT_APPLYING_CUFF"), 2000, EntryPoint.AnimationConfig.MedicTreatDict.Value, EntryPoint.AnimationConfig.MedicTreatName.Value, () => { p.IsBpCuffConnected = true; Game.DisplayNotification(Localization.Get("NOTIF_BP_CONNECTED")); }); MenuCore.CloseAll(); }); }
                    else { AddInteractiveItem(KitMenu, Localization.Get("ITEM_REMOVE_BP_CUFF"), Localization.Get("DESC_REMOVE_BP_CUFF"), true, () => { ActionsCore.Run(Localization.Get("ACT_REMOVING_CUFF"), 1500, EntryPoint.AnimationConfig.MedicTreatDict.Value, EntryPoint.AnimationConfig.MedicTreatName.Value, () => { p.IsBpCuffConnected = false; Game.DisplayNotification(Localization.Get("NOTIF_BP_REMOVED")); }); MenuCore.CloseAll(); }); }
                }
            }

            KitMenu.RefreshIndex();
        }

        #endregion
    }
}