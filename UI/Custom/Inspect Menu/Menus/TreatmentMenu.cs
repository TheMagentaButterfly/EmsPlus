using EmsPlus.Managers;
using EmsPlus.Managers.Actions;
using EmsPlus.Medical;
using Rage;
using System.Drawing;
using System.Linq;

namespace EmsPlus.UI.Custom.InspectMenu.Menus
{
    public static class TreatmentMenu
    {
        public static void BuildAirway(BodyPart part, Patient p)
        {
            bool hasO2 = InventoryManager.IsKitAvailable("OXYGENBAG", p.Character.Position);

            AddAction(true, "NONE", Localization.Get("ACT_MANUAL_AIRWAY", "Manual Airway Management"), Localization.Get("DESC_HEAD_TILT", "Head-tilt/Chin-lift"), EmsTreatment.AirwayManagement, part.BoneId);

            if (hasO2)
            {
                AddAction(hasO2, "OXYGENBAG", Localization.Get("ACT_APPLY_OXYGEN_MASK", "Apply Oxygen Mask"), Localization.Get("DESC_STANDARD_O2", "Standard O2 Therapy"), EmsTreatment.Oxygen, part.BoneId);
                AddAction(hasO2, "OXYGENBAG", Localization.Get("ACT_APPLY_NRB_MASK", "Apply NRB Mask"), Localization.Get("DESC_HIGH_FLOW_O2", "High Flow Oxygen"), EmsTreatment.HighFlowOxygen, part.BoneId);
                AddAction(hasO2, "OXYGENBAG", Localization.Get("ACT_USE_BVM", "Use Bag Valve Mask"), Localization.Get("DESC_ASSIST_VENTILATIONS", "Assist Ventilations"), EmsTreatment.BagValveMask, part.BoneId);
            }
        }

        public static void BuildOral(BodyPart part, Patient p)
        {
            BuildDynamicMedicationMenu(part, p, "ORAL");
        }

        public static void BuildIV(BodyPart part, Patient p)
        {
            bool hasTrauma = InventoryManager.IsKitAvailable("TRAUMABAG", p.Character.Position);

            if (!p.IsIVEstablished)
            {
                BodyInspectionManager.CurrentPanelActions.Add(new InspectionAction(
                    Localization.Get("ACT_ESTABLISH_IV", "Establish IV"),
                    hasTrauma ? Localization.Get("ACT_START_LINE", "Start IV Line") : Localization.Get("REQ_TRAUMA_BAG", "Requires Trauma Bag"),
                    Color.FromArgb(255, 255, 100, 0),
                    hasTrauma,
                    () => {
                        TreatmentActions.EstablishIV(part.BoneId);
                    }
                ));
            }
            else
            {
                // Fluids
                if (!p.IsReceivingFluids)
                {
                    AddAction(hasTrauma, "TRAUMABAG", Localization.Get("ITEM_HANG_FLUIDS", "Hang Fluids"), Localization.Get("DESC_HANG_FLUIDS", "Hang IV Fluids"), EmsTreatment.SalineBag, part.BoneId);
                }
                else
                {
                    BodyInspectionManager.CurrentPanelActions.Add(new InspectionAction(
                        Localization.Get("ACT_STOP_FLUIDS", "Stop Fluids"), Localization.Get("DESC_STOP_FLUIDS", "Stop IV Fluids"), Color.FromArgb(255, 255, 100, 0), true,
                        () => {
                            p.IsReceivingFluids = false;
                            Game.DisplayNotification(Localization.Get("NOTIF_FLUIDS_STOPPED", "IV Fluids stopped."));
                            BodyInspectionManager.RefreshActions();
                        }
                    ));
                }
                BuildDynamicMedicationMenu(part, p, "IV");
            }
        }

        public static void BuildIM(BodyPart part, Patient p)
        {
            BuildDynamicMedicationMenu(part, p, "IM");
        }

        private static void BuildDynamicMedicationMenu(BodyPart part, Patient p, string category)
        {
            var meds = EntryPoint.MedicationConfig.Definitions.Where(m => m.Categories.Contains(category)).ToList();

            foreach (var med in meds)
            {
                if (med.AllowedBones != null && med.AllowedBones.Count > 0 && !med.AllowedBones.Contains(part.BoneId))
                    continue;

                string reqKit = med.RequiredKit;
                if (string.IsNullOrEmpty(reqKit) || reqKit == "NONE")
                {
                    if (category == "IV" || category == "IM") reqKit = "TRAUMABAG";
                    else reqKit = "NONE";
                }

                bool hasKit = reqKit == "NONE" || InventoryManager.IsKitAvailable(reqKit, p.Character.Position);
                bool canGive = hasKit;

                if (category == "IV" && !p.IsIVEstablished) canGive = false;

                string subText;
                if (category == "IV" && !p.IsIVEstablished) subText = Localization.Get("REQ_IV", "Requires IV");
                else if (!hasKit) subText = string.Format(Localization.GetFormat("REQ_GENERIC", "Requires {0}"), reqKit);
                else subText = med.Description;

                BodyInspectionManager.CurrentPanelActions.Add(new InspectionAction(
                    med.Name,
                    subText,
                    canGive ? Color.FromArgb(255, 255, 50, 50) : Color.FromArgb(255, 60, 60, 60),
                    canGive,
                    () => {
                        TreatmentActions.AdministerGeneric(med.Name);
                        BodyInspectionManager.RefreshActions();
                    }
                ));
            }
        }

        public static void BuildTreatments(BodyPart part, Patient p) { }

        private static void AddAction(bool hasKit, string reqKit, string label, string desc, EmsTreatment treatment, PedBoneId bone)
        {
            BodyInspectionManager.CurrentPanelActions.Add(new InspectionAction(
                label,
                hasKit ? desc : string.Format(Localization.GetFormat("REQ_GENERIC", "Requires {0}"), reqKit),
                hasKit ? Color.FromArgb(255, 0, 180, 255) : Color.FromArgb(255, 60, 60, 60),
                hasKit,
                () => {
                    BodyInspectionManager.HandleTreatmentLogic(treatment, bone);
                }
            ));
        }
    }
}