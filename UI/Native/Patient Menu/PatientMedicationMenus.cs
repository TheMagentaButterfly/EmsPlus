using EmsPlus.Core;
using EmsPlus.Managers;
using EmsPlus.Managers.Actions;
using Rage;
using RAGENativeUI;
using System.Linq;

namespace EmsPlus.UI.Native.PatientMenu
{
    public static partial class PatientMenuBuilder
    {
        #region Medication Category Menus
        private static void BuildCategoryMenu(UIMenu menu, string categoryTag)
        {
            menu.Clear();
            var p = GameState.CurrentPatient;
            if (p == null) return;

            Vector3 patPos = p.Character.Position;

            foreach (var med in EntryPoint.MedicationConfig.Definitions.Where(m => m.Categories.Contains(categoryTag)))
            {
                string reqKit = med.RequiredKit;
                if (string.IsNullOrEmpty(reqKit) || reqKit == "NONE")
                {
                    if (categoryTag == "IM") reqKit = "TRAUMABAG";
                    else reqKit = "NONE";
                }

                bool hasKit = reqKit == "NONE" || InventoryManager.IsKitAvailable(reqKit, patPos);

                // COLORIZED
                string title = $"~g~{med.Name}";
                string sub = !hasKit ? $"~r~{string.Format(Localization.Get("REQ_GENERIC"), reqKit)}" : med.Description;
                string reqKitString = string.Format(Localization.Get("REQ_GENERIC_COLORED") ?? "~r~Requires {0}", reqKit);

                AddInteractiveItem(menu, title, sub, hasKit, () => {
                    TreatmentActions.AdministerGeneric(med.Name);
                    MenuCore.CloseAll();
                });
            }

            menu.RefreshIndex();
        }
        #endregion
    }
}