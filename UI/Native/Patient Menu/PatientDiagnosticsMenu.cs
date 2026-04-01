using EmsPlus.Core;
using EmsPlus.Managers;
using EmsPlus.Managers.Actions;
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

            // Header Vitals (Disabled visual items)
            var infoItem = new UIMenuItem(string.Format(Localization.Get("ITEM_STATUS_CONSCIOUSNESS"), p.Consciousness));
            infoItem.Enabled = false;
            DiagnosticsMenu.AddItem(infoItem);

            // Interactive Actions
            bool hasBystander = GameState.CurrentBystander != null;
            AddInteractiveItem(DiagnosticsMenu, Localization.Get("ITEM_CHECK_MED_TAGS"), Localization.Get("DESC_CHECK_MED_TAGS"), true, () => { DiagnosticActions.CheckHistory(); });

            bool hasTrauma = InventoryManager.IsKitAvailable("TRAUMABAG", p.Character.Position);
            AddInteractiveItem(DiagnosticsMenu, Localization.Get("ITEM_CHECK_BGL"), hasTrauma ? Localization.Get("DESC_CHECK_BGL") : Localization.Get("REQ_TRAUMA_BAG"), hasTrauma, () => { DiagnosticActions.CheckBGL(); MenuCore.CloseAll(); });

            AddInteractiveItem(DiagnosticsMenu, Localization.Get("ACT_TRAUMA_SWEEP") ?? "Perform Trauma Sweep", Localization.Get("DESC_TRAUMA_SWEEP") ?? "Examine patient's entire body for injuries.", true, () => {
                ActionsCore.Run(Localization.Get("NOTIF_INSPECTING") ?? "Inspecting...", 5000, EntryPoint.AnimationConfig.MedicAssessDict.Value, EntryPoint.AnimationConfig.MedicAssessName.Value, () => {
                    foreach (var bone in Enum.GetValues(typeof(PedBoneId)).Cast<PedBoneId>())
                    {
                        p.MarkBoneInspected(bone);
                    }
                    Game.DisplayNotification(Localization.Get("NOTIF_SWEEP_COMPLETE") ?? "~g~Trauma sweep complete.");
                });
                MenuCore.CloseAll();
            });

            DiagnosticsMenu.RefreshIndex();
        }

        #endregion
    }
}