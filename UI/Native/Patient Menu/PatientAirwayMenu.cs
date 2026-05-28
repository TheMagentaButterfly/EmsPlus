using EmsPlus.Core;
using EmsPlus.Managers;
using EmsPlus.Managers.Actions;
using EmsPlus.Medical;
using Rage;
using RAGENativeUI.Elements;

namespace EmsPlus.UI.Native.PatientMenu
{
    public static partial class PatientMenuBuilder
    {
        #region Airway Menu
        private static void BuildAirwayMenu()
        {
            AirwayMenu.Clear();
            var p = GameState.CurrentPatient;
            if (p == null) return;

            Vector3 patPos = p.Character.Position;
            bool hasO2 = InventoryManager.IsKitAvailable("OXYGENBAG", patPos);

            AddInteractiveItem(AirwayMenu, $"~y~{Localization.Get("ACT_MANUAL_AIRWAY", "Manual Airway")}", Localization.Get("DESC_HEAD_TILT", "Tilt the patient's head to open the airway"), true, () => {
                ActionsCore.Run(Localization.Get("ACT_MANAGING_AIRWAY", "Managing Airway..."), 3000, EntryPoint.AnimationConfig.MedicTreatDict.Value, EntryPoint.AnimationConfig.MedicTreatName.Value, () => {
                    p.ApplyTreatment(EmsTreatment.AirwayManagement, PedBoneId.Head);
                    Game.DisplayNotification(Localization.Get("NOTIF_AIRWAY_OPENED", "~g~Airway manually opened."));
                });
            });

            AddMenuSeparator(AirwayMenu, Localization.Get("CAT_SEP_OXYGEN", "~c~=== OXYGEN & MASKS ==="));
            AddInteractiveItem(AirwayMenu, $"~b~{Localization.Get("ACT_APPLY_OXYGEN_MASK", "Apply Oxygen Mask")}", hasO2 ? Localization.Get("DESC_STANDARD_O2", "Apply standard oxygen mask") : $"~r~{Localization.Get("REQ_OXYGEN_BAG", "Requires Oxygen Bag")}", hasO2, () => {
                ActionsCore.Run(Localization.Get("ACT_APPLYING_O2_MASK", "Applying O2 Mask..."), 2000, EntryPoint.AnimationConfig.MedicTreatDict.Value, EntryPoint.AnimationConfig.MedicTreatName.Value, () => {
                    p.IsReceivingOxygen = true;
                    p.ApplyTreatment(EmsTreatment.Oxygen, PedBoneId.Head);
                });
            });

            AddInteractiveItem(AirwayMenu, $"~b~{Localization.Get("ACT_APPLY_NRB_MASK", "Apply NRB Mask")}", hasO2 ? Localization.Get("DESC_HIGH_FLOW_O2", "Apply high-flow oxygen mask") : $"~r~{Localization.Get("REQ_OXYGEN_BAG", "Requires Oxygen Bag")}", hasO2, () => {
                ActionsCore.Run(Localization.Get("ACT_APPLYING_NRB_MASK", "Applying NRB Mask..."), 2000, EntryPoint.AnimationConfig.MedicTreatDict.Value, EntryPoint.AnimationConfig.MedicTreatName.Value, () => {
                    p.IsReceivingOxygen = true;
                    p.ApplyTreatment(EmsTreatment.HighFlowOxygen, PedBoneId.Head);
                });
            });

            AddInteractiveItem(AirwayMenu, $"~b~{Localization.Get("ACT_USE_BVM", "Use BVM")}", hasO2 ? Localization.Get("DESC_ASSIST_VENTILATIONS", "Assist ventilations with BVM") : $"~r~{Localization.Get("REQ_OXYGEN_BAG", "Requires Oxygen Bag")}", hasO2, () => {
                ActionsCore.Run(Localization.Get("ACT_ASSISTING_VENTILATIONS", "Assisting Ventilations..."), 4000, EntryPoint.AnimationConfig.MedicTreatDict.Value, EntryPoint.AnimationConfig.MedicTreatName.Value, () => {
                    p.ApplyTreatment(EmsTreatment.BagValveMask, PedBoneId.Head);
                });
            });

            AirwayMenu.RefreshIndex();
        }
        #endregion
    }
}