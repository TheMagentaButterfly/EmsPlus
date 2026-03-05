using EmsPlus.Managers;
using EmsPlus.Managers.Actions;
using EmsPlus.Medical.Conditions;
using EmsPlus.Medical.Frameworks;
using EmsPlus.UI.CustomMenus.InspectMenu.Managers;
using System.Drawing;
using System.Linq;

namespace EmsPlus.UI.CustomMenus.InspectMenu.Menus
{
    public static class KitMenu
    {
        public static void Build(BodyPart part, Patient p)
        {
            var placedKit = InventoryManager.PlacedKits.FirstOrDefault(k => k.Prop == part.LinkedEntity);
            if (placedKit == null) return;
            string currentCat = BodyInspectionManager.CurrentMenuCategory;

            // If we are in any bag sub-category, the back button goes to KIT_HOME
            if (currentCat == "KIT_TRAUMA" || currentCat == "KIT_IMMOBILIZE" || currentCat == "KIT_WOUNDCARE")
            {
                BodyInspectionManager.CurrentPanelActions.Add(new InspectionAction(
                    Localization.Get("BTN_BACK"), Localization.Get("BTN_BACK_DESC"), Color.FromArgb(255, 60, 60, 60), true,
                    () => { BodyInspectionManager.CurrentMenuCategory = "KIT_HOME"; BodyInspectionManager.RefreshActions(); }
                ));
            }

            if (currentCat == "KIT_HOME")
            {
                BodyInspectionManager.CurrentPanelActions.Add(new InspectionAction(
                    Localization.Get("BTN_PICKUP_KIT"), Localization.Get("BTN_PICKUP_KIT_DESC"), Color.Orange, true, () => {
                        InventoryManager.PickupKit(part.LinkedEntity);
                        BodyInspectionManager.RefreshActions();
                    }
                ));
            }

            // --- 2. TRAUMA BAG ---
            if (placedKit.KitID == "TRAUMABAG")
            {
                switch (currentCat)
                {
                    case "KIT_TRAUMA":
                        AddToolOption(Localization.Get("TRT_BANDAGE"), EmsTreatment.Bandage);
                        AddToolOption(Localization.Get("TRT_TOURNIQUET"), EmsTreatment.Tourniquet);
                        AddToolOption(Localization.Get("TRT_JUNCTIONALTOURNIQUET"), EmsTreatment.JunctionalTourniquet);
                        AddToolOption(Localization.Get("TRT_WOUNDPACKING"), EmsTreatment.WoundPacking);
                        AddToolOption(Localization.Get("TRT_SUTURE"), EmsTreatment.Suture);
                        break;

                    case "KIT_WOUNDCARE":
                        AddToolOption(Localization.Get("TRT_BURNDRESSING"), EmsTreatment.BurnDressing);
                        AddToolOption(Localization.Get("TRT_WETDRESSING"), EmsTreatment.WetDressing);
                        AddToolOption(Localization.Get("TRT_ICEPACK"), EmsTreatment.IcePack);
                        AddToolOption(Localization.Get("TRT_IRRIGATION"), EmsTreatment.Irrigation);
                        AddToolOption(Localization.Get("TRT_EYEPATCH"), EmsTreatment.EyePatch);
                        AddToolOption(Localization.Get("TRT_EYESHIELD"), EmsTreatment.EyeShield);
                        AddToolOption(Localization.Get("TRT_STABILISEOBJECT"), EmsTreatment.StabiliseObject);
                        break;

                    case "KIT_IMMOBILIZE":
                        AddToolOption(Localization.Get("TRT_SPLINT"), EmsTreatment.Splint);
                        AddToolOption(Localization.Get("TRT_TRACTIONSPLINT"), EmsTreatment.TractionSplint);
                        AddToolOption(Localization.Get("TRT_PELVICBINDER"), EmsTreatment.PelvicBinder);
                        AddToolOption(Localization.Get("TRT_CERVICALCOLLAR"), EmsTreatment.CervicalCollar);
                        AddToolOption(Localization.Get("TRT_SPINALIMMOBILISATION"), EmsTreatment.SpinalImmobilisation);
                        break;

                    default:
                        AddCategoryBtn(Localization.Get("CAT_TRAUMA"), Localization.Get("CAT_TRAUMA_DESC"), "KIT_TRAUMA");
                        AddCategoryBtn(Localization.Get("CAT_WOUNDCARE"), Localization.Get("CAT_WOUNDCARE_DESC"), "KIT_WOUNDCARE");
                        AddCategoryBtn(Localization.Get("CAT_IMMOBILIZE"), Localization.Get("CAT_IMMOBILIZE_DESC"), "KIT_IMMOBILIZE");
                        break;
                }
            }

            // --- 3. OXYGEN BAG ---
            if (placedKit.KitID == "OXYGENBAG")
            {
                AddToolOption(Localization.Get("TRT_OXYGEN"), EmsTreatment.Oxygen);
                AddToolOption(Localization.Get("TRT_HIGHFLOWOXYGEN"), EmsTreatment.HighFlowOxygen);
                AddToolOption(Localization.Get("TRT_BAGVALVEMASK"), EmsTreatment.BagValveMask);
                AddToolOption(Localization.Get("TRT_CHESTSEAL"), EmsTreatment.ChestSeal);
                AddToolOption(Localization.Get("TRT_NEEDLEDECOMP"), EmsTreatment.NeedleDecomp);
            }

            // --- 4. DEFIBRILLATOR RESTORED ---
            if (placedKit.KitID == "DEFIBRILLATOR")
            {
                if (!p.IsEcgsConnected)
                {
                    BodyInspectionManager.CurrentPanelActions.Add(new InspectionAction(Localization.Get("ACT_ATTACH_MONITOR"), Localization.Get("ACT_ATTACH_MONITOR"), Color.LightGreen, true, () => {
                        ActionsCore.Run("", 2000, EntryPoint.AnimationConfig.MedicTreatDict.Value, EntryPoint.AnimationConfig.MedicTreatName.Value, () => { p.IsEcgsConnected = true; });
                        BodyInspectionManager.StopInspection(false);
                    }));
                }
                else
                {
                    BodyInspectionManager.CurrentPanelActions.Add(new InspectionAction(Localization.Get("ACT_REMOVE_MONITOR"), Localization.Get("ACT_REMOVE_MONITOR"), Color.Orange, true, () => {
                        ActionsCore.Run("", 1500, EntryPoint.AnimationConfig.MedicTreatDict.Value, EntryPoint.AnimationConfig.MedicTreatName.Value, () => { p.IsEcgsConnected = false; });
                        BodyInspectionManager.StopInspection(false);
                    }));
                }

                if (!p.IsBpCuffConnected)
                {
                    BodyInspectionManager.CurrentPanelActions.Add(new InspectionAction(Localization.Get("ACT_ATTACH_BP"), Localization.Get("ACT_ATTACH_BP"), Color.LightGreen, true, () => {
                        ActionsCore.Run("", 2000, EntryPoint.AnimationConfig.MedicTreatDict.Value, EntryPoint.AnimationConfig.MedicTreatName.Value, () => { p.IsBpCuffConnected = true; });
                        BodyInspectionManager.StopInspection(false);
                    }));
                }
                else
                {
                    BodyInspectionManager.CurrentPanelActions.Add(new InspectionAction(Localization.Get("ACT_REMOVE_BP"), Localization.Get("ACT_REMOVE_BP"), Color.Orange, true, () => {
                        ActionsCore.Run("", 1500, EntryPoint.AnimationConfig.MedicTreatDict.Value, EntryPoint.AnimationConfig.MedicTreatName.Value, () => { p.IsBpCuffConnected = false; });
                        BodyInspectionManager.StopInspection(false);
                    }));
                }
            }
        }

        private static void AddCategoryBtn(string label, string sub, string category)
        {
            BodyInspectionManager.CurrentPanelActions.Add(new InspectionAction(label, sub, Color.Gray, true, () => {
                BodyInspectionManager.CurrentMenuCategory = category;
                BodyInspectionManager.RefreshActions();
            }));
        }
        public static void BuildDiagnostics(BodyPart part, Patient p)
        {
            BodyInspectionManager.CurrentPanelActions.Add(new InspectionAction(Localization.Get("ACT_CHECK_BGL"), Localization.Get("ACT_GLUCOMETER"), Color.FromArgb(255, 0, 150, 200), true, () => {
                DiagnosticActions.CheckBGL();
                BodyInspectionManager.StopInspection(false);
            }));
        }
        private static void AddToolOption(string label, EmsTreatment tool)
        {
            bool inCabin = AmbulanceManager.IsPlayerInRearCabin;
            bool hasSupply = InventoryManager.HasSupply(tool);
            
            // Check dictionary for quantity, default to 99 for infinite/hands-on items
            int count = InventoryManager.CurrentSupplies.ContainsKey(tool) ? InventoryManager.CurrentSupplies[tool] : 99;
            
            string displayLabel = (inCabin || count == 99) ? label : $"{label} [{count}]";
            string desc = hasSupply ? Localization.Get("BTN_TAKE_ITEM_DESC") : Localization.Get("BTN_ITEM_EMPTY_DESC");

            BodyInspectionManager.CurrentPanelActions.Add(new InspectionAction(
                $"TAKE {displayLabel}", 
                desc, 
                hasSupply ? Color.LightBlue : Color.FromArgb(255, 60, 60, 60), 
                hasSupply, 
                () => {
                    InventoryManager.ActiveTool = tool;
                    //Rage.Game.DisplayNotification($"~b~Prepared:~w~ {label}. Select injured body part.");
                    BodyInspectionManager.RefreshActions();
            }));
        }
    }
}