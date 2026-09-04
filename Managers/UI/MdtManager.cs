using System;
using System.Linq;
using EmsPlus.Callouts;
using EmsPlus.Core;
using EmsPlus.UI.Helpers;
using Rage;
using Rage.Native;

namespace EmsPlus.Managers
{
    public class MdtManager : WebMenu
    {
        private static MdtManager _instance;
        public static MdtManager Instance => _instance ?? (_instance = new MdtManager());

        public override string HtmlFileName => "mdt.html";

        public override float Scale => EntryPoint.OffsetConfig != null ? Math.Max(0.3f, EntryPoint.OffsetConfig.MdtScale) : 0.8f;


        public override int OffsetX
        {
            get => EntryPoint.OffsetConfig != null ? (int)EntryPoint.OffsetConfig.MdtOffsetX : 0;
            set
            {
                if (EntryPoint.OffsetConfig != null)
                {
                    EntryPoint.OffsetConfig.MdtOffsetX = value;
                }
            }
        }

        public override int OffsetY
        {
            get => EntryPoint.OffsetConfig != null ? (int)EntryPoint.OffsetConfig.MdtOffsetY : 0;
            set
            {
                if (EntryPoint.OffsetConfig != null)
                {
                    EntryPoint.OffsetConfig.MdtOffsetY = value;
                }
            }
        }

        public static bool IsVisible => Instance.IsActiveMenu;
        public static bool IsMouseUnlocked
        {
            get => Instance.MouseUnlocked;
            set => Instance.MouseUnlocked = value;
        }

        public static void Toggle(bool? state = null)
        {
            bool targetState = state ?? !IsVisible;

            if (targetState)
            {
                if (EntryPoint.EmsPlusConfig.RequireAmbulanceForMdt.Value && !AmbulanceManager.IsPlayerInsideAmbulance())
                {
                    return;
                }

                WebUIManager.OpenMenu(Instance);
            }
            else
            {
                WebUIManager.CloseMenu();
            }
        }

        public static void ShowCalloutPage()
        {
            if (EntryPoint.EmsPlusConfig.RequireAmbulanceForMdt.Value && !AmbulanceManager.IsPlayerInsideAmbulance())
            {
                return;
            }

            Instance.MouseUnlocked = false;
            WebUIManager.OpenMenu(Instance);
        }

        public override void OnProcess()
        {
            if (EntryPoint.EmsPlusConfig.RequireAmbulanceForMdt.Value && !AmbulanceManager.IsPlayerInsideAmbulance())
            {
                Close();
                return;
            }

            if (IsMouseUnlocked)
            {
                NativeFunction.Natives.DISABLE_CONTROL_ACTION(0, 1, true);
                NativeFunction.Natives.DISABLE_CONTROL_ACTION(0, 2, true);
                NativeFunction.Natives.DISABLE_CONTROL_ACTION(0, 3, true);
                NativeFunction.Natives.DISABLE_CONTROL_ACTION(0, 4, true);
                NativeFunction.Natives.DISABLE_CONTROL_ACTION(0, 5, true);
                NativeFunction.Natives.DISABLE_CONTROL_ACTION(0, 6, true);

                NativeFunction.Natives.DISABLE_CONTROL_ACTION(0, 66, true);
                NativeFunction.Natives.DISABLE_CONTROL_ACTION(0, 67, true);
                NativeFunction.Natives.DISABLE_CONTROL_ACTION(0, 106, true);
                NativeFunction.Natives.DISABLE_CONTROL_ACTION(0, 107, true);
                NativeFunction.Natives.DISABLE_CONTROL_ACTION(0, 108, true);

                NativeFunction.Natives.DISABLE_CONTROL_ACTION(0, 24, true);
                NativeFunction.Natives.DISABLE_CONTROL_ACTION(0, 25, true);
                NativeFunction.Natives.DISABLE_CONTROL_ACTION(0, 140, true);
                NativeFunction.Natives.DISABLE_CONTROL_ACTION(0, 141, true);
                NativeFunction.Natives.DISABLE_CONTROL_ACTION(0, 142, true);
                NativeFunction.Natives.DISABLE_CONTROL_ACTION(0, 257, true);

                NativeFunction.Natives.DISABLE_CONTROL_ACTION(0, 68, true);
                NativeFunction.Natives.DISABLE_CONTROL_ACTION(0, 69, true);
                NativeFunction.Natives.DISABLE_CONTROL_ACTION(0, 70, true);
                NativeFunction.Natives.DISABLE_CONTROL_ACTION(0, 91, true);
                NativeFunction.Natives.DISABLE_CONTROL_ACTION(0, 92, true);

                NativeFunction.Natives.DISABLE_CONTROL_ACTION(0, 199, true);
                NativeFunction.Natives.DISABLE_CONTROL_ACTION(0, 200, true);
                NativeFunction.Natives.DISABLE_CONTROL_ACTION(0, 85, true);
            }
        }

        public static void SetMouseUnlocked(bool unlocked)
        {
            WebUIManager.SetMouseUnlocked(unlocked);
        }


        public static void ForceUpdateLayout()
        {
            if (IsVisible)
            {
                WebUIManager.OpenMenu(Instance);
                PushCurrentStateToWeb();
            }
        }

        public static void Cleanup()
        {
            if (IsVisible)
            {
                Toggle(false);
            }
        }

        public override void OnOpen()
        {
            PushCurrentStateToWeb();
        }

        public override void OnSavePosition()
        {
            EntryPoint.OffsetConfig?.Save();
        }

        public override void OnMessage(string action)
        {
            if (action == "get_mdt_data" || action == "refresh")
            {
                PushCurrentStateToWeb();
            }
            else if (action == "save_mdt_position")
            {
                OnSavePosition();
            }
            else if (action.StartsWith("set_status:"))
            {
                string statusString = action.Substring(11);
                if (Enum.TryParse(statusString, out EmsStatus parsedStatus))
                {
                    EmsService.SetStatus(parsedStatus);
                    PushCurrentStateToWeb();
                }
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

            Instance.ExecuteScript($"updateMdtData({payload})");
        }

        private static string EscapeJsString(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
            return input.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "");
        }
    }
}