using EmsPlus.UI.Native;
using EmsPlus.UI.Tasks;
using Rage;
using EmsPlus.Core;
using EmsPlus.Medical;

namespace EmsPlus.Managers.Actions
{
    public static class DiagnosticActions
    {
        public static void TalkToBystander()
        {
            if (GameState.CurrentBystander == null)
            {
                Game.DisplayNotification(Localization.Get("NOTIF_NO_BYSTANDER"));
                return;
            }
            Game.DisplayNotification("char_call911", "char_call911", Localization.Get("NOTIF_TITLE_BYSTANDER"), Localization.Get("NOTIF_SUBTITLE_PATIENT_HISTORY"), GameState.CurrentPatient.GetSamplersString());
        }

        public static void CheckHistory()
        {
            ActionsCore.Run(Localization.Get("ACTION_CHECKING_ALERT"), 4000, EntryPoint.AnimationConfig.MedicNoteDict.Value, EntryPoint.AnimationConfig.MedicNoteName.Value, () => {
                Game.DisplayNotification("char_call911", "char_call911", Localization.Get("NOTIF_TITLE_MEDIC_ALERT"), Localization.Get("NOTIF_SUBTITLE_INFO"), GameState.CurrentPatient.History.GetLimitedHistoryString());
            });
        }

        public static void CheckBGL()
        {
            if (GameState.CurrentPatient == null) return;

            GameState.IsPlayerBusy = true;
            MenuCore.CloseAll();

            var task = new BglTask();

            task.OnBglMeasured += (s, stateResult) => {
                if (GameState.CurrentPatient != null)
                {
                    GameState.CurrentPatient.BloodGlucose = stateResult;
                    GameState.CurrentPatient.IsBglChecked = true;

                    if (stateResult == VitalState.CriticalLow || stateResult == VitalState.Low)
                    {
                        Game.DisplayNotification("~y~Result: LOW.");
                    }

                    else if (stateResult == VitalState.CriticalHigh || stateResult == VitalState.Elevated)
                    {
                        Game.DisplayNotification("~r~Result: HIGH.");
                    }
                }
            };

            task.OnTaskCompleted += (s, e) => {
                GameState.IsPlayerBusy = false;
            };

            task.OnTaskAborted += (s, e) => {
                GameState.IsPlayerBusy = false;
            };

            task.Start();
        }
    }
}