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

            bool hasTraumaKit = InventoryManager.IsKitAvailable("TRAUMABAG", p.Character.Position);
            bool anyInjuries = false;

            var physicalInjuries = p.Conditions.OfType<PhysicalInjury>().Where(i => !i.IsTreated && p.IsBoneInspected(i.Bone));

            foreach (var injury in physicalInjuries)
            {
                anyInjuries = true;

                foreach (var requiredTreatment in injury.RequiredTreatments)
                {
                    string label = $"~r~{string.Format(Localization.Get("ACT_APPLY_TREATMENT_FORMAT") ?? "Apply {0}", requiredTreatment)}";
                    string desc = hasTraumaKit ? string.Format(Localization.Get("DESC_INJURY_LOCATION") ?? "Location: ~y~{0}~w~ ({1})", injury.Bone, injury.Name) : $"~r~{Localization.Get("REQ_TRAUMA_BAG")}";
                    
                    AddInteractiveItem(TraumaMenu, label, desc, hasTraumaKit, () => {
                        ActionsCore.Run(Localization.Get("NOTIF_TREATING") ?? "Treating...", 3000, EntryPoint.AnimationConfig.MedicTreatDict.Value, EntryPoint.AnimationConfig.MedicTreatName.Value, () => {
                            p.ApplyTreatment(requiredTreatment, injury.Bone);
                        });
                        MenuCore.CloseAll();
                    });
                }
            }

            if (!anyInjuries)
            {
                var none = new UIMenuItem(Localization.Get("ITEM_NO_INJURIES_COLORED") ?? "~g~No Injuries Found", Localization.Get("DESC_NO_INJURIES_NATIVEUI") ?? "Patient has no visible untreated trauma. Make sure to perform a Trauma Sweep from the Diagnostics menu first.");
                none.Enabled = false;
                none.SetRightBadge(UIMenuItem.BadgeStyle.Tick);
                TraumaMenu.AddItem(none);
            }

            TraumaMenu.RefreshIndex();
        }
        #endregion
    }
}