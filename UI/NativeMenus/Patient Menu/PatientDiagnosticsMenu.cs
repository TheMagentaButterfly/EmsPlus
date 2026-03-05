using EmsPlus.Core;
using EmsPlus.Framework;
using EmsPlus.Managers;
using EmsPlus.Managers.Actions;
using RAGENativeUI.Elements;

namespace EmsPlus.UI.NativeMenus.PatientMenu
{
    public static partial class PatientMenuBuilder
    {
        #region Diagnostics Menu
        private static void BuildDiagnosticsMenu()
        {
            DiagnosticsMenu.Clear();
            var p = GameState.CurrentPatient;
            if (p == null) return;

            // Header Vitals (Disabled visual items)
            var infoItem = new UIMenuItem(string.Format(Localization.Get("ITEM_STATUS_CONSCIOUSNESS"), p.Consciousness));
            infoItem.Enabled = false;
            DiagnosticsMenu.AddItem(infoItem);

            // Interactive Actions
            bool hasBystander = GameState.CurrentBystander != null;
            AddInteractiveItem(DiagnosticsMenu, Localization.Get("ITEM_CHECK_MED_TAGS"), Localization.Get("DESC_CHECK_MED_TAGS"), true, () => { DiagnosticActions.CheckHistory(); });

            bool hasTrauma = InventoryManager.IsKitAvailable("TraumaBag", p.Character.Position);
            AddInteractiveItem(DiagnosticsMenu, Localization.Get("ITEM_CHECK_BGL"), hasTrauma ? Localization.Get("DESC_CHECK_BGL") : Localization.Get("REQ_TRAUMA_BAG"), hasTrauma, () => { DiagnosticActions.CheckBGL(); MenuCore.CloseAll(); });

            DiagnosticsMenu.RefreshIndex();
        }

        #endregion
    }
}