using EmsPlus.Callouts;
using EmsPlus.Core;
using Rage;
using Rage.Native;
using System;
using System.Linq;

namespace EmsPlus.Managers
{
    public static class MdtManager
    {
        public static bool IsVisible => EntryPoint.IsUiOpen;
        public static bool IsMouseUnlocked { get; set; } = false;

        public static void Toggle(bool state)
        {
            if (state)
            {
                EntryPoint.NavigateUI("mdt.html");
            }

            EntryPoint.ToggleUI(state);

            if (state)
            {
                SetMouseUnlocked(IsMouseUnlocked);
                PushCurrentStateToWeb();
            }
            else
            {
                SetMouseUnlocked(false);
            }
        }

        public static void SetMouseUnlocked(bool state)
        {
            IsMouseUnlocked = state;
            EntryPoint.SetMouseUnlocked(state);
            EntryPoint.ExecuteScriptOnUI($"setMouseLockState({(state ? "true" : "false")})");
        }

        public static void ShowCalloutPage()
        {
            IsMouseUnlocked = false;
            Toggle(true);
            PushCurrentStateToWeb();
        }

        public static void ForceUpdateLayout()
        {
            if (IsVisible)
            {
                PushCurrentStateToWeb();
            }
        }

        public static void Process()
        {
            if (!IsVisible) return;

            EntryPoint.ProcessIncomingMessages();

            if (IsMouseUnlocked)
            {
                NativeFunction.Natives.DISABLE_CONTROL_ACTION(0, 1, true);   // LookLeftRight
                NativeFunction.Natives.DISABLE_CONTROL_ACTION(0, 2, true);   // LookUpDown
                NativeFunction.Natives.DISABLE_CONTROL_ACTION(0, 3, true);   // Look fly LR
                NativeFunction.Natives.DISABLE_CONTROL_ACTION(0, 4, true);   // Look fly UD
                NativeFunction.Natives.DISABLE_CONTROL_ACTION(0, 5, true);   // Look UI LR
                NativeFunction.Natives.DISABLE_CONTROL_ACTION(0, 6, true);   // Look UI UD

                NativeFunction.Natives.DISABLE_CONTROL_ACTION(0, 66, true);  // Vehicle Look Behind
                NativeFunction.Natives.DISABLE_CONTROL_ACTION(0, 67, true);  // Vehicle Look Left/Right
                NativeFunction.Natives.DISABLE_CONTROL_ACTION(0, 106, true); // Vehicle Mouse Control Override
                NativeFunction.Natives.DISABLE_CONTROL_ACTION(0, 107, true); // Vehicle Fly Mouse Control Override
                NativeFunction.Natives.DISABLE_CONTROL_ACTION(0, 108, true); // Vehicle Sub Mouse Control Override

                NativeFunction.Natives.DISABLE_CONTROL_ACTION(0, 24, true);  // Attack
                NativeFunction.Natives.DISABLE_CONTROL_ACTION(0, 25, true);  // Aim
                NativeFunction.Natives.DISABLE_CONTROL_ACTION(0, 140, true); // Melee Light
                NativeFunction.Natives.DISABLE_CONTROL_ACTION(0, 141, true); // Melee Heavy
                NativeFunction.Natives.DISABLE_CONTROL_ACTION(0, 142, true); // Melee Alt
                NativeFunction.Natives.DISABLE_CONTROL_ACTION(0, 257, true); // Attack 2

                NativeFunction.Natives.DISABLE_CONTROL_ACTION(0, 68, true);  // Vehicle Aim
                NativeFunction.Natives.DISABLE_CONTROL_ACTION(0, 69, true);  // Vehicle Attack
                NativeFunction.Natives.DISABLE_CONTROL_ACTION(0, 70, true);  // Vehicle Attack 2
                NativeFunction.Natives.DISABLE_CONTROL_ACTION(0, 91, true);  // Vehicle Passenger Aim
                NativeFunction.Natives.DISABLE_CONTROL_ACTION(0, 92, true);  // Vehicle Passenger Attack

                NativeFunction.Natives.DISABLE_CONTROL_ACTION(0, 199, true); // Pause
                NativeFunction.Natives.DISABLE_CONTROL_ACTION(0, 200, true); // Esc
                NativeFunction.Natives.DISABLE_CONTROL_ACTION(0, 85, true);  // Radio Wheel
            }
        }

        public static void Cleanup()
        {
            if (IsVisible)
            {
                Toggle(false);
            }
        }

        public static void PushCurrentStateToWeb()
        {
            EmsCallout currentCall = CalloutManager.ActiveCallout ?? CalloutManager.PendingCallout;

            string calloutName = currentCall != null ? currentCall.CalloutName : "Standby / No Active Call";
            string location = currentCall != null ? CalloutManager.CalloutLocationString : "---";
            string acceptTime = currentCall != null ? CalloutManager.CalloutAcceptTime : "";

            var p = GameState.CurrentPatient;
            string patientName = p != null ? p.Details.FullName : (currentCall != null ? "Assigned / En Route..." : "---");
            string patientDob = p != null ? $"{p.Details.DateOfBirth} ({p.Details.Age}y)" : (currentCall != null ? "Pending on-scene..." : "---");
            string patientGender = p != null ? p.Details.Gender : (currentCall != null ? "Pending..." : "---");

            string dest = "TBD";
            if (EmsService.CurrentStatus == EmsStatus.Transporting)
            {
                var nearestHosp = EntryPoint.HospitalsConfig.Locations
                    .OrderBy(l => Game.LocalPlayer.Character.Position.DistanceTo(l.Position))
                    .FirstOrDefault();
                if (nearestHosp != null) dest = nearestHosp.Name;
            }

            string currentStatusEnum = EmsService.CurrentStatus.ToString();
            string localizedStatus = Localization.Get($"STATUS_{currentStatusEnum.ToUpperInvariant()}", currentStatusEnum);

            string payload = "{" +
                $"\"calloutName\":\"{EscapeJsString(calloutName)}\"," +
                $"\"location\":\"{EscapeJsString(location)}\"," +
                $"\"acceptTime\":\"{EscapeJsString(acceptTime)}\"," +
                $"\"patientName\":\"{EscapeJsString(patientName)}\"," +
                $"\"patientDob\":\"{EscapeJsString(patientDob)}\"," +
                $"\"patientGender\":\"{EscapeJsString(patientGender)}\"," +
                $"\"destination\":\"{EscapeJsString(dest)}\"," +
                $"\"status\":\"{EscapeJsString(localizedStatus)}\"," +
                $"\"statusEnum\":\"{EscapeJsString(currentStatusEnum)}\"" +
            "}";

            EntryPoint.ExecuteScriptOnUI($"updateMdtData({payload})");
        }

        private static string EscapeJsString(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
            return input.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "");
        }
    }
}