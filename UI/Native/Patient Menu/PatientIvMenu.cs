using EmsPlus.Core;
using EmsPlus.Managers;
using EmsPlus.Managers.Actions;
using Rage;
using RAGENativeUI.Elements;
using System.Linq;

namespace EmsPlus.UI.Native.PatientMenu
{
    public static partial class PatientMenuBuilder
    {
        #region IV Menu
        private static void BuildIvMenu()
        {
            IvMenu.Clear();
            var p = GameState.CurrentPatient;
            if (p == null) return;

            Vector3 patPos = p.Character.Position;
            bool hasTrauma = InventoryManager.IsKitAvailable("TRAUMABAG", patPos);

            if (p.IsIVEstablished)
            {
                var est = new UIMenuItem(Localization.Get("ITEM_IV_ESTABLISHED"), Localization.Get("DESC_IV_ESTABLISHED"));
                est.Enabled = false;
                est.SetRightBadge(UIMenuItem.BadgeStyle.Tick);
                IvMenu.AddItem(est);

                // Fluids
                if (!p.IsReceivingFluids)
                {
                    AddInteractiveItem(IvMenu, Localization.Get("ITEM_HANG_FLUIDS"), hasTrauma ? Localization.Get("DESC_HANG_FLUIDS") : Localization.Get("REQ_TRAUMA_BAG"), hasTrauma, () => { TreatmentActions.AdministerFluids(); MenuCore.CloseAll(); });
                }
                else
                {
                    AddInteractiveItem(IvMenu, Localization.Get("ITEM_STOP_FLUIDS"), Localization.Get("DESC_STOP_FLUIDS"), true, () => { p.IsReceivingFluids = false; Game.DisplayNotification(Localization.Get("NOTIF_FLUIDS_STOPPED")); BuildIvMenu(); });
                }
            }

            // IV Meds Dynamic Loop
            foreach (var med in EntryPoint.MedicationConfig.Definitions.Where(m => m.Categories.Contains("IV")))
            {
                string reqKit = med.RequiredKit;
                if (string.IsNullOrEmpty(reqKit) || reqKit == "NONE") reqKit = "TRAUMABAG";

                bool hasKit = reqKit == "NONE" || InventoryManager.IsKitAvailable(reqKit, patPos);
                bool canGive = hasKit && p.IsIVEstablished;

                string subText = !p.IsIVEstablished ? Localization.Get("REQ_IV") : (!hasKit ? string.Format(Localization.Get("REQ_GENERIC"), reqKit) : med.Description);

                AddInteractiveItem(IvMenu, med.Name, subText, canGive, () => { TreatmentActions.AdministerGeneric(med.Name); MenuCore.CloseAll(); });
            }

            IvMenu.RefreshIndex();
        }

        #endregion
    }
}