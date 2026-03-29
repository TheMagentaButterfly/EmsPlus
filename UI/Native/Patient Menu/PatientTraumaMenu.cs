using EmsPlus.Core;
using EmsPlus.Managers;
using EmsPlus.Managers.Actions;
using EmsPlus.Medical;
using RAGENativeUI.Elements;
using System.Linq;

namespace EmsPlus.UI.Native.PatientMenu
{
    public static partial class PatientMenuBuilder
    {
        #region Trauma Menu
        private static void BuildTraumaMenu()
        {
            TraumaMenu.Clear();
            var p = GameState.CurrentPatient;
            if (p == null) return;

            bool hasTraumaKit = InventoryManager.IsKitAvailable("TraumaBag", p.Character.Position);
            bool anyInjuries = false;

            // ONLY get injuries that are on inspected bones!
            var physicalInjuries = p.Conditions.OfType<PhysicalInjury>().Where(i => !i.IsTreated && p.IsBoneInspected(i.Bone));

            foreach (var injury in physicalInjuries)
            {
                anyInjuries = true;

                foreach (var requiredTreatment in injury.RequiredTreatments)
                {
                    string label = string.Format(Localization.Get("ITEM_APPLY_TREATMENT_FORMAT"), requiredTreatment);
                    string desc = hasTraumaKit ? string.Format(Localization.Get("DESC_TREATMENT_LOCATION_FORMAT"), injury.Bone) : Localization.Get("REQ_TRAUMA_BAG");

                    AddInteractiveItem(TraumaMenu, label, desc, hasTraumaKit, () => {
                        ActionsCore.Run(Localization.Get("NOTIF_TREATING"), 3000, EntryPoint.AnimationConfig.MedicTreatDict.Value, EntryPoint.AnimationConfig.MedicTreatName.Value, () => {
                            p.ApplyTreatment(requiredTreatment, injury.Bone);
                        });
                        MenuCore.CloseAll();
                    });
                }
            }

            if (!anyInjuries)
            {
                var none = new UIMenuItem(Localization.Get("ITEM_NO_INJURIES"), Localization.Get("DESC_NO_INJURIES"));
                none.Enabled = false;
                TraumaMenu.AddItem(none);
            }

            TraumaMenu.RefreshIndex();
        }

        #endregion
    }
}