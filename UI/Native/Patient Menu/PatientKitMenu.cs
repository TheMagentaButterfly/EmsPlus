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
                var none = new UIMenuItem($"~c~{Localization.Get("ITEM_NO_KITS_NEARBY")}", Localization.Get("DESC_NO_KITS_NEARBY"));
                none.Enabled = false;
                KitMenu.AddItem(none);
                KitMenu.RefreshIndex();
                return;
            }

            foreach (var kit in InventoryManager.PlacedKits)
            {
                string kitName = Localization.Get($"KIT_NAME_{kit.KitID.ToUpperInvariant()}") ?? kit.KitID;

                AddInteractiveItem(KitMenu, $"~p~{string.Format(Localization.Get("ACT_PICK_UP_KIT_FORMAT") ?? "Pick Up {0}", kitName)}", Localization.Get("DESC_PICK_UP_KIT"), true, () => {
                    InventoryManager.PickupKit(kit.Prop);
                    AudioHelper.PlaySuccess();
                    BuildKitMenu();
                });

                if (kit.KitID == "DEFIBRILLATOR")
                {
                    AddMenuSeparator(KitMenu, Localization.Get("CAT_SEP_DEFIB") ?? "~c~=== DEFIBRILLATOR ===");

                    if (!p.IsEcgsConnected) { AddInteractiveItem(KitMenu, $"~g~{Localization.Get("ACT_ATTACH_MONITOR")}", Localization.Get("DESC_ATTACH_MONITOR"), true, () => { ActionsCore.Run("Attaching leads...", 2000, EntryPoint.AnimationConfig.MedicTreatDict.Value, EntryPoint.AnimationConfig.MedicTreatName.Value, () => { p.IsEcgsConnected = true; Game.DisplayNotification("~g~Monitor connected."); }); MenuCore.CloseAll(); }); }
                    else { AddInteractiveItem(KitMenu, $"~y~{Localization.Get("ACT_REMOVE_MONITOR")}", Localization.Get("DESC_REMOVE_MONITOR"), true, () => { ActionsCore.Run("Removing leads...", 1500, EntryPoint.AnimationConfig.MedicTreatDict.Value, EntryPoint.AnimationConfig.MedicTreatName.Value, () => { p.IsEcgsConnected = false; Game.DisplayNotification("~y~Monitor disconnected."); }); MenuCore.CloseAll(); }); }

                    if (!p.IsBpCuffConnected) { AddInteractiveItem(KitMenu, $"~g~{Localization.Get("ACT_ATTACH_BP")}", Localization.Get("DESC_ATTACH_BP_CUFF"), true, () => { ActionsCore.Run("Applying cuff...", 2000, EntryPoint.AnimationConfig.MedicTreatDict.Value, EntryPoint.AnimationConfig.MedicTreatName.Value, () => { p.IsBpCuffConnected = true; Game.DisplayNotification("~g~BP Cuff attached."); }); MenuCore.CloseAll(); }); }
                    else { AddInteractiveItem(KitMenu, $"~y~{Localization.Get("ACT_REMOVE_BP")}", Localization.Get("DESC_REMOVE_BP_CUFF"), true, () => { ActionsCore.Run("Removing cuff...", 1500, EntryPoint.AnimationConfig.MedicTreatDict.Value, EntryPoint.AnimationConfig.MedicTreatName.Value, () => { p.IsBpCuffConnected = false; Game.DisplayNotification("~y~BP Cuff removed."); }); MenuCore.CloseAll(); }); }
                }
            }

            KitMenu.RefreshIndex();
        }
        #endregion
    }
}