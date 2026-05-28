using EmsPlus.UI.Native;
using EmsPlus.UI.Tasks;
using Rage;
using EmsPlus.Core;
using EmsPlus.Medical;

namespace EmsPlus.Managers.Actions
{
    public static class DiagnosticActions
    {
        public static void CheckBGL()
        {
            if (GameState.CurrentPatient == null) return;

            // NATIVE-UI BYPASS
            if (EntryPoint.EmsPlusConfig.UseNativeUIPatientMenu.Value)
            {
                ActionsCore.Run(Localization.Get("ACT_CHECKING_BGL", "Checking Blood Glucose..."), 4000,
                    EntryPoint.AnimationConfig.MedicTreatDict.Value,
                    EntryPoint.AnimationConfig.MedicTreatName.Value,
                    () => {
                        if (GameState.CurrentPatient != null)
                        {
                            GameState.CurrentPatient.IsBglChecked = true;
                            var stateResult = GameState.CurrentPatient.BloodGlucose;

                            if (stateResult == VitalState.CriticalLow || stateResult == VitalState.Low)
                                Rage.Game.DisplayNotification(Localization.Get("NOTIF_BGL_LOW", "~y~BGL Result: LOW."));
                            else if (stateResult == VitalState.CriticalHigh || stateResult == VitalState.Elevated)
                                Rage.Game.DisplayNotification(Localization.Get("NOTIF_BGL_HIGH", "~r~BGL Result: HIGH."));
                            else
                                Rage.Game.DisplayNotification(Localization.Get("NOTIF_BGL_NORMAL", "~g~BGL Result: NORMAL."));
                        }
                    });
                return;
            }

            // CUSTOM 3D MENU
            GameState.IsPlayerBusy = true;
            MenuCore.CloseAll();

            var task = new BglTask();

            task.OnBglMeasured += (s, stateResult) => {
                if (GameState.CurrentPatient != null)
                {
                    GameState.CurrentPatient.BloodGlucose = stateResult;
                    GameState.CurrentPatient.IsBglChecked = true;

                    if (stateResult == VitalState.CriticalLow || stateResult == VitalState.Low)
                        Rage.Game.DisplayNotification(Localization.Get("NOTIF_BGL_LOW", "~y~BGL Result: LOW."));
                    else if (stateResult == VitalState.CriticalHigh || stateResult == VitalState.Elevated)
                        Rage.Game.DisplayNotification(Localization.Get("NOTIF_BGL_HIGH", "~r~BGL Result: HIGH."));
                    else
                        Rage.Game.DisplayNotification(Localization.Get("NOTIF_BGL_NORMAL", "~g~BGL Result: NORMAL."));
                }
            };

            task.OnTaskCompleted += (s, e) => { GameState.IsPlayerBusy = false; };
            task.OnTaskAborted += (s, e) => { GameState.IsPlayerBusy = false; };

            task.Start();
        }
    }
}