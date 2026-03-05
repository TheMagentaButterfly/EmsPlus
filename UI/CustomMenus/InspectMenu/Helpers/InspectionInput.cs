using Rage;
using Rage.Native;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace EmsPlus.UI.CustomMenus.InspectMenu.Helpers
{
    public class InspectionInput
    {
        private bool _wasMouseDown;
        public bool IsHoveringPanel => PanelBounds.Contains(MousePosition);
        public Point MousePosition { get; private set; }

        public RectangleF ExitButton { get; set; }
        public RectangleF DiagnosticsButton { get; set; }
        public RectangleF PanelBounds { get; set; }

        public List<RectangleF> PanelActionButtons { get; } = new List<RectangleF>();
        public List<RectangleF> DiagnosticActionButtons { get; } = new List<RectangleF>();
        public List<RectangleF> EquipmentButtons { get; } = new List<RectangleF>();

        public void Update() => MousePosition = Cursor.Position;

        public bool ShouldExit() => Game.IsKeyDown(Keys.Escape);
        public bool ShouldToggleDiagnostics() => NativeFunction.Natives.IS_DISABLED_CONTROL_JUST_PRESSED<bool>(0, 37); // Tab

        public bool IsClicking()
        {
            bool isDown = Game.IsKeyDown(Keys.LButton);
            bool clicked = isDown && !_wasMouseDown;
            _wasMouseDown = isDown;
            return clicked;
        }

        public bool ClickedExit() => IsHovering(ExitButton);
        public bool ClickedDiagnostics() => IsHovering(DiagnosticsButton);

        public int GetClickedPanelAction() => PanelActionButtons.FindIndex(IsHovering);
        public int GetClickedDiagnosticAction() => DiagnosticActionButtons.FindIndex(IsHovering);

        public bool IsHovering(RectangleF rect) => rect.Contains(MousePosition);
        public bool IsHoveringPanelAction(int index) => index >= 0 && index < PanelActionButtons.Count && IsHovering(PanelActionButtons[index]);
        public bool IsHoveringDiagnosticAction(int index) => index >= 0 && index < DiagnosticActionButtons.Count && IsHovering(DiagnosticActionButtons[index]);
    }
}