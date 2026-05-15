using System;

namespace EmsPlus.UI.Helpers   
{
    public class InspectionState
    {
        public float Alpha { get; private set; }
        public float Pulse { get; private set; }
        public float PanelSlide { get; set; }
        public float DiagnosticSlide { get; private set; }
        public bool DiagnosticsExpanded { get; private set; }
        public int ActionScrollIndex { get; set; } = 0;
        public float DataSlide { get; private set; }
        public bool DataExpanded { get; private set; }



        public void Update()
        {
            Alpha = Math.Min(Alpha + 0.05f, 1f);
            Pulse += 0.08f;

            const float step = 0.12f;
            DiagnosticSlide = DiagnosticsExpanded ? Math.Min(DiagnosticSlide + step, 1f) : Math.Max(DiagnosticSlide - step, 0f);

            DataSlide = DataExpanded ? Math.Min(DataSlide + step, 1f) : Math.Max(DataSlide - step, 0f);
        }

        public void ToggleDiagnostics()
        {
            DiagnosticsExpanded = !DiagnosticsExpanded;
            if (DiagnosticsExpanded) DataExpanded = false;
        }

        public void ToggleData()
        {
            DataExpanded = !DataExpanded;
            if (DataExpanded) DiagnosticsExpanded = false;
        }

        public void CyclePanels()
        {
            if (!DiagnosticsExpanded && !DataExpanded)
            {
                DiagnosticsExpanded = true;
                DataExpanded = false;
            }
            else if (DiagnosticsExpanded)
            {
                DiagnosticsExpanded = false;
                DataExpanded = true;
            }
            else if (DataExpanded)
            {
                DiagnosticsExpanded = false;
                DataExpanded = false;
            }
        }

        public int BaseAlpha => MathHelper.Clamp((int)(200 * Alpha), 0, 255);
        public int TextAlpha => MathHelper.Clamp((int)(255 * Alpha), 0, 255);
    }
}