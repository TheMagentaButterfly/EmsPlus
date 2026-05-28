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

            if (!p.IsIVEstablished)
            {
                AddInteractiveItem(IvMenu, $"~o~{Localization.Get("ACT_ESTABLISH_IV", "Establish IV")}", hasTrauma ? Localization.Get("ACT_START_LINE", "Start IV Line") : $"~r~{Localization.Get("REQ_TRAUMA_BAG", "Requires Trauma Bag")}", hasTrauma, () => {
                    TreatmentActions.EstablishIV(PedBoneId.RightForearm);
                });
            }
            else
            {
                var est = new UIMenuItem(Localization.Get("ITEM_IV_ESTABLISHED_COLORED", "~g~IV ESTABLISHED"), Localization.Get("DESC_IV_ESTABLISHED", "Access available"));
                est.Enabled = false;
                est.SetRightBadge(UIMenuItem.BadgeStyle.Tick);
                IvMenu.AddItem(est);

                if (!p.IsReceivingFluids)
                {
                    AddInteractiveItem(IvMenu, $"~b~{Localization.Get("ACT_HANG_FLUIDS", "Hang Fluids")}", hasTrauma ? Localization.Get("DESC_HANG_FLUIDS", "Administer IV fluids") : $"~r~{Localization.Get("REQ_TRAUMA_BAG", "Requires Trauma Bag")}", hasTrauma, () => {
                        TreatmentActions.AdministerFluids();
                    });
                }
                else
                {
                    AddInteractiveItem(IvMenu, $"~y~{Localization.Get("ACT_STOP_FLUIDS", "Stop Fluids")}", Localization.Get("DESC_STOP_FLUIDS", "Stop IV fluids"), true, () => {
                        p.IsReceivingFluids = false;
                        Game.DisplayNotification(Localization.Get("NOTIF_FLUIDS_STOPPED", "~y~Fluids stopped."));
                        BuildIvMenu();
                    });
                }

                AddMenuSeparator(IvMenu, Localization.Get("CAT_SEP_IV_MEDS", "~c~=== IV MEDS ==="));

                foreach (var med in EntryPoint.MedicationConfig.Definitions.Where(m => m.Categories.Contains("IV")))
                {
                    string reqKit = med.RequiredKit;
                    if (string.IsNullOrEmpty(reqKit) || reqKit == "NONE") reqKit = "TRAUMABAG";

                    bool hasKit = reqKit == "NONE" || InventoryManager.IsKitAvailable(reqKit, patPos);
                    bool canGive = hasKit && p.IsIVEstablished;

                    string reqIvString = $"~r~{Localization.Get("REQ_IV", "Requires IV")}";
                    string reqKitString = $"~r~{string.Format(Localization.GetFormat("REQ_GENERIC", "Requires {0}"), reqKit)}";
                    string subText = !p.IsIVEstablished ? reqIvString : (!hasKit ? reqKitString : med.Description);

                    AddInteractiveItem(IvMenu, $"~g~{med.Name}", subText, canGive, () => {
                        TreatmentActions.AdministerGeneric(med.Name);
                    });
                }
            }
            IvMenu.RefreshIndex();
        }
        #endregion
    }
}