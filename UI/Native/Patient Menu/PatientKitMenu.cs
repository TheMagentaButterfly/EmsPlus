using EmsPlus.Core;
using EmsPlus.Managers;
using EmsPlus.Managers.Actions;
using EmsPlus.Medical;
using EmsPlus.UI.Helpers;
using Rage;
using RAGENativeUI.Elements;

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
                    AddMenuSeparator(KitMenu, Localization.Get("CAT_SEP_DEFIB", "~c~=== MONITOR & DEFIBRILLATOR ==="));

                    // ECG Leads
                    if (!p.IsEcgsConnected)
                    {
                        AddInteractiveItem(KitMenu, Localization.Get("ACT_ATTACH_ECG_LEADS", "Attach ECG Leads"), Localization.Get("ACT_ATTACH_ECG_LEADS_DESC", "Connect ECG monitoring leads"), true, () => {
                            ActionsCore.Run("Attaching ECG leads...", 2000, EntryPoint.AnimationConfig.MedicTreatDict.Value, EntryPoint.AnimationConfig.MedicTreatName.Value, () => {
                                p.ApplyTreatment(EmsTreatment.ECG);
                                Game.DisplayNotification("~g~ECG Leads attached.");
                            });
                            MenuCore.CloseAll();
                        });
                    }
                    else
                    {
                        AddInteractiveItem(KitMenu, Localization.Get("ACT_REMOVE_ECG_LEADS", "Remove ECG Leads"), Localization.Get("ACT_REMOVE_ECG_LEADS_DESC", "Disconnect ECG monitoring leads"), true, () => {
                            ActionsCore.Run("Removing ECG leads...", 1500, EntryPoint.AnimationConfig.MedicTreatDict.Value, EntryPoint.AnimationConfig.MedicTreatName.Value, () => {
                                p.IsEcgsConnected = false;
                                Game.DisplayNotification("~y~ECG Leads removed.");
                            });
                            MenuCore.CloseAll();
                        });
                    }

                    // SpO2 Pulse Oximeter Probe
                    if (!p.IsSpO2Connected)
                    {
                        AddInteractiveItem(KitMenu, Localization.Get("ACT_ATTACH_SPO2_PROBE", "Attach SpO2 Probe"), Localization.Get("ACT_ATTACH_SPO2_PROBE_DESC", "Attach pulse oximeter probe"), true, () => {
                            ActionsCore.Run("Attaching SpO2 probe...", 1500, EntryPoint.AnimationConfig.MedicTreatDict.Value, EntryPoint.AnimationConfig.MedicTreatName.Value, () => {
                                p.ApplyTreatment(EmsTreatment.SpO2);
                                Game.DisplayNotification("~g~SpO2 Probe attached.");
                            });
                            MenuCore.CloseAll();
                        });
                    }
                    else
                    {
                        AddInteractiveItem(KitMenu, Localization.Get("ACT_REMOVE_SPO2_PROBE", "Remove SpO2 Probe"), Localization.Get("ACT_REMOVE_SPO2_PROBE_DESC", "Remove pulse oximeter probe"), true, () => {
                            ActionsCore.Run("Removing SpO2 probe...", 1000, EntryPoint.AnimationConfig.MedicTreatDict.Value, EntryPoint.AnimationConfig.MedicTreatName.Value, () => {
                                p.IsSpO2Connected = false;
                                Game.DisplayNotification("~y~SpO2 Probe removed.");
                            });
                            MenuCore.CloseAll();
                        });
                    }

                    // Blood Pressure Cuff
                    if (!p.IsBpCuffConnected)
                    {
                        AddInteractiveItem(KitMenu, Localization.Get("ACT_ATTACH_BP_CUFF", "Attach BP Cuff"), Localization.Get("ACT_ATTACH_BP_CUFF_DESC", "Apply NIBP cuff"), true, () => {
                            ActionsCore.Run("Applying BP cuff...", 2000, EntryPoint.AnimationConfig.MedicTreatDict.Value, EntryPoint.AnimationConfig.MedicTreatName.Value, () => {
                                p.ApplyTreatment(EmsTreatment.BPCuff);
                                Game.DisplayNotification("~g~BP Cuff attached.");
                            });
                            MenuCore.CloseAll();
                        });
                    }
                    else
                    {
                        AddInteractiveItem(KitMenu, Localization.Get("ACT_REMOVE_BP_CUFF", "Remove BP Cuff"), Localization.Get("ACT_REMOVE_BP_CUFF_DESC", "Remove NIBP cuff"), true, () => {
                            ActionsCore.Run("Removing BP cuff...", 1500, EntryPoint.AnimationConfig.MedicTreatDict.Value, EntryPoint.AnimationConfig.MedicTreatName.Value, () => {
                                p.IsBpCuffConnected = false;
                                Game.DisplayNotification("~y~BP Cuff removed.");
                            });
                            MenuCore.CloseAll();
                        });
                    }
                }
            }

            KitMenu.RefreshIndex();
        }
        #endregion
    }
}