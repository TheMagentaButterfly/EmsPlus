using EmsPlus.Core;
using EmsPlus.Managers;
using EmsPlus.Managers.Actions;
using EmsPlus.Medical;
using Rage;
using RAGENativeUI.Elements;
using System;
using System.Linq;

namespace EmsPlus.UI.Native.PatientMenu
{
    public static partial class PatientMenuBuilder
    {
        #region Diagnostics Menu
        private static void BuildDiagnosticsMenu()
        {
            DiagnosticsMenu.Clear();
            var p = GameState.CurrentPatient;
            if (p == null) return;

            string conscText = Localization.Get($"CONSC_{p.Consciousness.ToString().ToUpperInvariant()}") ?? p.Consciousness.ToString();
            AddReadonlyItem(DiagnosticsMenu, $"~b~{Localization.Get("DIAG_CONSCIOUSNESS", "Consciousness")}", conscText);

            if (p.IsBglChecked)
            {
                string bglResult = p.BloodGlucose == VitalState.Normal ? "~g~NORMAL" : "~r~ABNORMAL";
                AddReadonlyItem(DiagnosticsMenu, $"~b~{Localization.Get("DIAG_BGL", "Blood Glucose Level")}", bglResult);
            }

            var dispatchItem = new UIMenuItem($"~y~{Localization.Get("DIAG_DISPATCH_DIAGNOSIS", "Dispatch Diagnosis")}", p.DispatchDiagnosis);
            dispatchItem.Enabled = false;
            DiagnosticsMenu.AddItem(dispatchItem);

            AddMenuSeparator(DiagnosticsMenu, Localization.Get("CAT_SEP_ASSESSMENTS", "~c~=== ~b~ASSESSMENTS ~c~==="));
            bool hasTraumaKit = InventoryManager.IsKitAvailable("TRAUMABAG", p.Character.Position);
            AddInteractiveItem(DiagnosticsMenu, $"~b~{Localization.Get("ACT_CHECK_BGL", "Check Blood Glucose Level")}", hasTraumaKit ? Localization.Get("ACT_GLUCOMETER", "Use Glucometer") : $"~r~{Localization.Get("REQ_TRAUMA_BAG", "Requires Trauma Bag")}", hasTraumaKit, () => {
                DiagnosticActions.CheckBGL();
            });

            AddInteractiveItem(DiagnosticsMenu, $"~r~{Localization.Get("ACT_TRAUMA_SWEEP", "Trauma Sweep")}", Localization.Get("DESC_TRAUMA_SWEEP", "Perform a trauma sweep"), true, () => {
                ActionsCore.Run(Localization.Get("ACT_TRAUMA_INSPECT", "Inspecting..."), 5000, EntryPoint.AnimationConfig.MedicAssessDict.Value, EntryPoint.AnimationConfig.MedicAssessName.Value, () => {
                    foreach (var bone in Enum.GetValues(typeof(PedBoneId)).Cast<PedBoneId>())
                    {
                        p.MarkBoneInspected(bone);
                    }
                    Game.DisplayNotification(Localization.Get("NOTIF_TRAUMA_SWEEP_COMPLETE", "~g~Trauma sweep complete. Check Diagnostics for tips."));
                });
            });

            var conditions = p.Conditions.Where(c =>
                (c is PhysicalInjury pi && p.IsBoneInspected(pi.Bone)) ||
                (c is SystemicCondition)
            ).ToList();

            if (conditions.Any())
            {
                AddMenuSeparator(DiagnosticsMenu, Localization.Get("CAT_SEP_CONDITIONS", "~c~=== ~r~ACTIVE CONDITIONS ~c~==="));

                string formatDesc = Localization.Get("DESC_TIP_FORMAT", "Status: {0}\nRequired: {1}");
                string noneText = Localization.Get("DIAG_STATUS_NONE", "~c~None");
                string systemicLbl = Localization.Get("LBL_SYSTEMIC", "Systemic");
                foreach (var cond in conditions)
                {
                    string status = cond.IsTreated ? (Localization.Get("DIAG_STATUS_TREATED", "~g~Treated")) : (Localization.Get("DIAG_STATUS_NEEDS_TREATMENT", "~r~Needs Treatment"));

                    var reqList = cond.RequiredTreatments.Select(t => GetLocalizedTreatmentName(t));
                    string reqs = cond.IsTreated ? noneText : $"~y~{string.Join(", ", reqList)}";

                    string desc = formatDesc.Replace("\\n", "\n").Replace("{0}", status).Replace("{1}", reqs);

                    string label = cond is PhysicalInjury inj ? $"~r~{inj.Bone}~w~: {inj.Name}" : $"~r~{systemicLbl}~w~: {cond.Name}";
                    var condItem = new UIMenuItem(label, desc);
                    condItem.Enabled = false;

                    if (cond.IsTreated) condItem.SetRightBadge(UIMenuItem.BadgeStyle.Tick);
                    else condItem.SetRightBadge(UIMenuItem.BadgeStyle.Alert);

                    DiagnosticsMenu.AddItem(condItem);
                }
            }

            DiagnosticsMenu.RefreshIndex();
        }

        private static string GetLocalizedTreatmentName(EmsTreatment treatment)
        {
            string key = $"TRT_{treatment.ToString().ToUpperInvariant()}";
            string localized = Localization.Get(key);

            if (localized != key) return localized;

            if (treatment == EmsTreatment.IVAccess) return "Establish IV";
            if (treatment == EmsTreatment.SalineBag) return "Hang IV Fluids";
            if (treatment == EmsTreatment.NeedleDecomp) return "Needle Decompression";
            if (treatment == EmsTreatment.CervicalCollar) return "Cervical Collar";
            if (treatment == EmsTreatment.ChestSeal) return "Chest Seal";
            if (treatment == EmsTreatment.DirectPressure) return "Direct Pressure";

            return treatment.ToString();
        }
        #endregion
    }
}