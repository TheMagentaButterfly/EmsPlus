using EmsPlus.Core;
using EmsPlus.Managers.Actions;
using EmsPlus.Medical;
using System;
using System.Collections.Generic;

namespace EmsPlus.UI.Custom.InspectMenu
{
    public class DiagnosticManager
    {
        public List<DiagnosticItem> Items { get; private set; } = new List<DiagnosticItem>();

        public DiagnosticManager() => Refresh();

        public void Refresh()
        {
            var newItems = new List<DiagnosticItem>();
            var p = GameState.CurrentPatient;
            if (p == null) return;

            string condition = p.DispatchDiagnosis;
            newItems.Add(new DiagnosticItem(Localization.Get("DIAG_DISPATCH_DIAGNOSIS"), condition, true));

            foreach (var cond in p.Conditions)
            {
                if (cond is PhysicalInjury injury && !p.IsBoneInspected(injury.Bone))
                {
                    continue;
                }
                List<string> steps = new List<string>();
                if (!cond.IsTreated)
                {
                    foreach (var req in cond.RequiredTreatments)
                    {
                        steps.Add($"• {GetLocalizedTreatmentName(req)}");
                    }
                }
                newItems.Add(new DiagnosticItem(
                    cond.Name,
                    cond.IsTreated ? (Localization.Get("DIAG_STATUS_TREATED") ?? "TREATED") : (Localization.Get("DIAG_STATUS_REQUIRED") ?? "REQUIRED"),
                    cond.IsTreated,
                    null, null, steps
                ));
            }

            if (p.IsEcgsConnected)
            {
                // HR
                string hrText = GetStateText(p.HeartRate);
                bool hrOk = p.HeartRate == VitalState.Normal;
                newItems.Add(new DiagnosticItem(Localization.Get("DIAG_HR"), hrText, hrOk));

                // SpO2
                string oxyText = GetStateText(p.SpO2);
                bool oxyOk = p.SpO2 == VitalState.Normal;
                newItems.Add(new DiagnosticItem(Localization.Get("DIAG_SPO2"), oxyText, oxyOk));
            }
            else
            {
                newItems.Add(new DiagnosticItem(Localization.Get("DIAG_MONITOR"), Localization.Get("DIAG_MONITOR_NOT_CONNECTED"), true));
            }

            if (p.IsBpCuffConnected)
            {
                string bpText = GetStateText(p.BloodPressure);
                bool bpOk = p.BloodPressure == VitalState.Normal;
                newItems.Add(new DiagnosticItem(Localization.Get("DIAG_BP"), bpText, bpOk));
            }

            if (p.IsBglChecked)
            {
                string bglText = GetStateText(p.BloodGlucose);
                bool bglOk = p.BloodGlucose == VitalState.Normal;
                newItems.Add(new DiagnosticItem(Localization.Get("DIAG_BGL"), bglText, bglOk));
            }
            Items = newItems;

            string conscLoc = Localization.Get($"CONSC_{p.Consciousness.ToString().ToUpperInvariant()}") ?? p.Consciousness.ToString();
            newItems.Add(new DiagnosticItem(
                Localization.Get("DIAG_CONSCIOUSNESS") ?? "Consciousness",
                conscLoc,
                p.Consciousness == ConsciousnessLevel.Alert || p.Consciousness == ConsciousnessLevel.Verbal
            ));
        }

        private string GetLocalizedTreatmentName(EmsTreatment treatment)
        {
            string key = $"TRT_{treatment.ToString().ToUpperInvariant()}";
            string localized = Localization.Get(key);

            if (localized == key)
            {
                if (treatment == EmsTreatment.DirectPressure) return "Direct Pressure";
                if (treatment == EmsTreatment.AirwayManagement) return "Manage Airway";
                if (treatment == EmsTreatment.ChestSeal) return "Chest Seal";
                if (treatment == EmsTreatment.NeedleDecomp) return "Needle Decomp.";
                if (treatment == EmsTreatment.CervicalCollar) return "C-Collar";
                if (treatment == EmsTreatment.PelvicBinder) return "Pelvic Binder";
                if (treatment == EmsTreatment.TractionSplint) return "Traction Splint";
                if (treatment == EmsTreatment.WoundPacking) return "Wound Packing";
                if (treatment == EmsTreatment.JunctionalTourniquet) return "Junctional TQ";
                if (treatment == EmsTreatment.BurnDressing) return "Burn Dressing";
                if (treatment == EmsTreatment.WetDressing) return "Wet Dressing";
                if (treatment == EmsTreatment.EyePatch) return "Eye Patch";
                if (treatment == EmsTreatment.EyeShield) return "Eye Shield";
                if (treatment == EmsTreatment.IcePack) return "Ice Pack";
                return treatment.ToString();
            }
            return localized;
        }

        private string GetStateText(VitalState state)
        {
            switch (state)
            {
                case VitalState.None: return "NONE / FLATLINE";
                case VitalState.CriticalLow: return "CRITICAL LOW";
                case VitalState.Low: return "LOW";
                case VitalState.Normal: return "NORMAL";
                case VitalState.Elevated: return "ELEVATED";
                case VitalState.CriticalHigh: return "CRITICAL HIGH";
                default: return "UNKNOWN";
            }
        }

        private string FormatTaskName(string type)
        {
            return type;
        }
    }
    public class DiagnosticItem
    {
        public string Label { get; }
        public string Value { get; }
        public bool IsNormal { get; }
        public string ActionButtonText { get; }
        public Action OnAction { get; }
        public bool HasAction => OnAction != null && !string.IsNullOrEmpty(ActionButtonText);
        public List<string> Details { get; } = new List<string>();

        public DiagnosticItem(string label, string value, bool normal, string btnTxt = null, Action action = null, List<string> details = null)
        {
            Label = label;
            Value = value;
            IsNormal = normal;
            ActionButtonText = btnTxt;
            OnAction = action;
            if (details != null) Details = details;
        }
    }
}