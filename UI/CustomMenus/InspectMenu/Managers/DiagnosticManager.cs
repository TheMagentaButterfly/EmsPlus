using EmsPlus.Managers.Actions;
using EmsPlus.Framework;
using System;
using System.Collections.Generic;
using EmsPlus.Core;
using EmsPlus.Medical.Frameworks;

namespace EmsPlus.UI.CustomMenus.InspectMenu
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
            newItems.Add(new DiagnosticItem("CONDITION", condition, true));

            foreach (var cond in p.Conditions)
            {
                newItems.Add(new DiagnosticItem(
                    cond.Name,
                    cond.IsTreated ? "TREATED" : "REQUIRED",
                    cond.IsTreated
                ));
            }

            if (p.IsEcgsConnected)
            {
                // HR
                string hrText = GetStateText(p.HeartRate);
                bool hrOk = p.HeartRate == VitalState.Normal;
                newItems.Add(new DiagnosticItem("Heart Rate", hrText, hrOk));

                // SpO2
                string oxyText = GetStateText(p.SpO2);
                bool oxyOk = p.SpO2 == VitalState.Normal;
                newItems.Add(new DiagnosticItem("SpO2", oxyText, oxyOk));
            }
            else
            {
                newItems.Add(new DiagnosticItem("Monitor", "Not Connected", true));
            }

            if (p.IsBpCuffConnected)
            {
                string bpText = GetStateText(p.BloodPressure);
                bool bpOk = p.BloodPressure == VitalState.Normal;
                newItems.Add(new DiagnosticItem("BP", bpText, bpOk));
            }

            if (p.IsBglChecked)
            {
                string bglText = GetStateText(p.BloodGlucose);
                bool bglOk = p.BloodGlucose == VitalState.Normal;
                newItems.Add(new DiagnosticItem("Glucose", bglText, bglOk));
            }

            // Witness
            if (GameState.CurrentBystander != null)
            {
                newItems.Add(new DiagnosticItem(
                    Localization.Get("DIAG_WITNESS_INFO"),
                    Localization.Get("DIAG_WITNESS_PRESENT"),
                    true,
                    Localization.Get("DIAG_WITNESS_ACTION"),
                    (Action)(() => { DiagnosticActions.TalkToBystander(); })
                ));
            }

            Items = newItems;
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

        public DiagnosticItem(string label, string value, bool normal, string btnTxt = null, Action action = null)
        {
            Label = label; Value = value; IsNormal = normal; ActionButtonText = btnTxt; OnAction = action;
        }
    }
}