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
        #region Medication Category Menus (Airway, Oral, IM)
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
                    else if (categoryTag == "AIRWAY" && med.Name == "Oxygen") reqKit = "OXYGENBAG";
                    else reqKit = "NONE";
                }

                bool hasKit = reqKit == "NONE" || InventoryManager.IsKitAvailable(reqKit, patPos);

                bool alreadyApplied = (med.Name == "Oxygen" && p.IsReceivingOxygen);
                string title = alreadyApplied ? string.Format(Localization.Get("ACT_APPLIED_PREFIX"), med.Name) : med.Name;
                string sub = !hasKit ? string.Format(Localization.Get("REQ_GENERIC"), reqKit) : med.Description;

                AddInteractiveItem(menu, title, sub, hasKit && !alreadyApplied, () => {
                    if (med.Name == "Oxygen") p.IsReceivingOxygen = true;
                    TreatmentActions.AdministerGeneric(med.Name);
                    MenuCore.CloseAll();
                });
            }

            menu.RefreshIndex();
        }

        #endregion
    }
}