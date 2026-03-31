using System;
using System.Drawing;

namespace EmsPlus.UI.Custom.InspectMenu
{
    public class InspectionAction
    {
        public string Label { get; set; }
        public string SubLabel { get; set; }
        public Color Color { get; set; }
        public bool Enabled { get; set; }
        public Action OnExecute { get; set; }

        public InspectionAction(string label, string subLabel, Color color, bool enabled, Action onExecute)
        {
            Label = label;
            SubLabel = subLabel;
            Color = color;
            Enabled = enabled;
            OnExecute = onExecute;
        }
    }
}