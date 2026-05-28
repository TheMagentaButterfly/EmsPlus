using EmsPlus.Managers;
using EmsPlus.Managers.Actions;
using EmsPlus.Medical;
using System.Drawing;
using System.Linq;

namespace EmsPlus.UI.Custom.InspectMenu.Menus
{
    public static class KitMenu
    {
        public static void Build(BodyPart part, Patient p)
        {
            var placedKit = InventoryManager.PlacedKits.FirstOrDefault(k => k.Prop == part.LinkedEntity);
            if (placedKit == null) return;
            string currentCat = BodyInspectionManager.CurrentMenuCategory;

            if (currentCat == "KIT_TRAUMA" || currentCat == "KIT_IMMOBILIZE" || currentCat == "KIT_WOUNDCARE")
            {
                BodyInspectionManager.CurrentPanelActions.Add(new InspectionAction(
                    Localization.Get("BTN_BACK", "◄ BACK"), Localization.Get("BTN_BACK_DESC", "Return to previous menu"), Color.FromArgb(255, 60, 60, 60), true,
                    () => { BodyInspectionManager.CurrentMenuCategory = "KIT_HOME"; BodyInspectionManager.RefreshActions(); }
                ));
            }

            if (currentCat == "KIT_HOME")
            {
                BodyInspectionManager.CurrentPanelActions.Add(new InspectionAction(
                    Localization.Get("BTN_PICKUP_KIT", "PICK UP KIT"), Localization.Get("BTN_PICKUP_KIT_DESC", "Equip to hands"), Color.Orange, true, () => {
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
                        AddToolOption(Localization.Get("TRT_BANDAGE", "Bandage"), EmsTreatment.Bandage);
                        AddToolOption(Localization.Get("TRT_WOUNDPACKING", "Wound Packing"), EmsTreatment.WoundPacking);
                        AddToolOption(Localization.Get("TRT_TOURNIQUET", "Tourniquet"), EmsTreatment.Tourniquet);
                        AddToolOption(Localization.Get("TRT_JUNCTIONALTOURNIQUET", "Junctional Tourniquet"), EmsTreatment.JunctionalTourniquet);
                        AddToolOption(Localization.Get("TRT_SUTURE", "Suture Kit"), EmsTreatment.Suture);
                        break;

                    case "KIT_WOUNDCARE":
                        AddToolOption(Localization.Get("TRT_BURNDRESSING", "Burn Dressing"), EmsTreatment.BurnDressing);
                        AddToolOption(Localization.Get("TRT_WETDRESSING", "Wet Dressing"), EmsTreatment.WetDressing);
                        AddToolOption(Localization.Get("TRT_ICEPACK", "Ice Pack"), EmsTreatment.IcePack);
                        AddToolOption(Localization.Get("TRT_IRRIGATION", "Irrigation Fluid"), EmsTreatment.Irrigation);
                        AddToolOption(Localization.Get("TRT_EYEPATCH", "Eye Patch"), EmsTreatment.EyePatch);
                        AddToolOption(Localization.Get("TRT_EYESHIELD", "Eye Shield"), EmsTreatment.EyeShield);
                        AddToolOption(Localization.Get("TRT_STABILISEOBJECT", "Object Stabilization"), EmsTreatment.StabiliseObject);
                        break;

                    case "KIT_IMMOBILIZE":
                        AddToolOption(Localization.Get("TRT_SPLINT", "SAM Splint"), EmsTreatment.Splint);
                        AddToolOption(Localization.Get("TRT_TRACTIONSPLINT", "Traction Splint"), EmsTreatment.TractionSplint);
                        AddToolOption(Localization.Get("TRT_PELVICBINDER", "Pelvic Binder"), EmsTreatment.PelvicBinder);
                        AddToolOption(Localization.Get("TRT_CERVICALCOLLAR", "Cervical Collar"), EmsTreatment.CervicalCollar);
                        //AddToolOption(Localization.Get("TRT_SPINALIMMOBILISATION", "Backboard"), EmsTreatment.SpinalImmobilisation);
                        break;

                    //case "KIT_CIRCULATION":
                    //AddToolOption(Localization.Get("TRT_IVACCESS", "IV Start Kit"), EmsTreatment.IVAccess);
                    //AddToolOption(Localization.Get("TRT_SALINEBAG", "Saline Bag"), EmsTreatment.SalineBag);
                    //break;

                    default:
                        AddCategoryBtn(Localization.Get("CAT_TRAUMA", "Trauma Supplies"), Localization.Get("CAT_TRAUMA_DESC", "Bandages, Tourniquets"), "KIT_TRAUMA");
                        AddCategoryBtn(Localization.Get("CAT_WOUNDCARE", "Wound Care"), Localization.Get("CAT_WOUNDCARE_DESC", "Dressings, Burns, Eyes"), "KIT_WOUNDCARE");
                        AddCategoryBtn(Localization.Get("CAT_IMMOBILIZE", "Immobilization"), Localization.Get("CAT_IMMOBILIZE_DESC", "Splints, Collars"), "KIT_IMMOBILIZE");
                        //AddCategoryBtn(Localization.Get("CAT_CIRCULATION", ""), Localization.Get("CAT_CIRCULATION_DESC", ""), "KIT_CIRCULATION");
                        break;
                }
            }

            // --- 3. OXYGEN BAG ---
            if (placedKit.KitID == "OXYGENBAG")
            {
                AddToolOption(Localization.Get("TRT_OXYGEN", "O2 Mask"), EmsTreatment.Oxygen);
                AddToolOption(Localization.Get("TRT_HIGHFLOWOXYGEN", "NRB Mask (High Flow)"), EmsTreatment.HighFlowOxygen);
                AddToolOption(Localization.Get("TRT_BAGVALVEMASK", "BVM (Bag Valve)"), EmsTreatment.BagValveMask);
                AddToolOption(Localization.Get("TRT_CHESTSEAL", "Chest Seal"), EmsTreatment.ChestSeal);
                AddToolOption(Localization.Get("TRT_NEEDLEDECOMP", "Needle Decompression"), EmsTreatment.NeedleDecomp);
            }

            // --- 4. DEFIBRILLATOR RESTORED ---
            if (placedKit.KitID == "DEFIBRILLATOR")
            {
                if (!p.IsEcgsConnected)
                {
                    BodyInspectionManager.CurrentPanelActions.Add(new InspectionAction(Localization.Get("ACT_ATTACH_MONITOR", "Attach ECG/SpO2 Monitor"), Localization.Get("ACT_ATTACH_MONITOR_DESC", "Attach ECG/SpO2 Monitor"), Color.LightGreen, true, () => {
                        ActionsCore.Run("", 2000, EntryPoint.AnimationConfig.MedicTreatDict.Value, EntryPoint.AnimationConfig.MedicTreatName.Value, () => { p.IsEcgsConnected = true; });
                        BodyInspectionManager.StopInspection(false);
                    }));
                }
                else
                {
                    BodyInspectionManager.CurrentPanelActions.Add(new InspectionAction(Localization.Get("ACT_REMOVE_MONITOR", "Remove ECG/SpO2 Monitor"), Localization.Get("ACT_REMOVE_MONITOR_DESC", "Remove ECG/SpO2 Monitor"), Color.Orange, true, () => {
                        ActionsCore.Run("", 1500, EntryPoint.AnimationConfig.MedicTreatDict.Value, EntryPoint.AnimationConfig.MedicTreatName.Value, () => { p.IsEcgsConnected = false; });
                        BodyInspectionManager.StopInspection(false);
                    }));
                }

                if (!p.IsBpCuffConnected)
                {
                    BodyInspectionManager.CurrentPanelActions.Add(new InspectionAction(Localization.Get("ACT_ATTACH_BP", "Attach BP Cuff"), Localization.Get("ACT_ATTACH_BP_DESC", "Attach BP Cuff"), Color.LightGreen, true, () => {
                        ActionsCore.Run("", 2000, EntryPoint.AnimationConfig.MedicTreatDict.Value, EntryPoint.AnimationConfig.MedicTreatName.Value, () => { p.IsBpCuffConnected = true; });
                        BodyInspectionManager.StopInspection(false);
                    }));
                }
                else
                {
                    BodyInspectionManager.CurrentPanelActions.Add(new InspectionAction(Localization.Get("ACT_REMOVE_BP", "Remove BP Cuff"), Localization.Get("ACT_REMOVE_BP_DESC", "Remove BP Cuff"), Color.Orange, true, () => {
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
            BodyInspectionManager.CurrentPanelActions.Add(new InspectionAction(Localization.Get("ACT_CHECK_BGL", "Check BGL"), Localization.Get("ACT_GLUCOMETER", "Use Glucometer"), Color.FromArgb(255, 0, 150, 200), true, () => {
                DiagnosticActions.CheckBGL();
                BodyInspectionManager.StopInspection(false);
            }));
        }
        private static void AddToolOption(string label, EmsTreatment tool)
        {
            bool inCabin = AmbulanceManager.IsPlayerInRearCabin;
            bool hasSupply = InventoryManager.HasSupply(tool);
            
            int count = InventoryManager.CurrentSupplies.ContainsKey(tool) ? InventoryManager.CurrentSupplies[tool] : 99;
            
            string displayLabel = (inCabin || count == 99) ? label : $"{label} [{count}]";
            string desc = hasSupply ? Localization.Get("BTN_TAKE_ITEM_DESC", "Take item from bag") : Localization.Get("BTN_ITEM_EMPTY_DESC", "~r~Empty! Restock at ambulance.");

            BodyInspectionManager.CurrentPanelActions.Add(new InspectionAction(
                $"{displayLabel}", 
                desc, 
                hasSupply ? Color.LightBlue : Color.FromArgb(255, 60, 60, 60), 
                hasSupply, 
                () => {
                    InventoryManager.ActiveTool = tool;
                    BodyInspectionManager.RefreshActions();
            }));
        }
    }
}