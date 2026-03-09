using EmsPlus.Core;
using EmsPlus.Managers;
using Rage;
using Rage.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace EmsPlus.Framework
{
    public static class CalloutManager
    {
        private static List<Type> RegisteredCallouts = new List<Type>();
        private static EmsCallout CurrentCallout;
        private static bool IsCalloutDisplayed = false;
        private static uint NextCalloutTime;
        private static uint CalloutExpireTime;
        private const int TimeoutDurationMs = 30000;
        private static Random Rnd = new Random();

        public static void Initialize()
        {
            RegisteredCallouts.Clear();
            CurrentCallout = null;
            IsCalloutDisplayed = false;
            SetNextCalloutTime(45000, 120000);
        }

        public static void RegisterCallout(Type calloutType)
        {
            if (!RegisteredCallouts.Contains(calloutType) && calloutType.IsSubclassOf(typeof(EmsCallout)))
            {
                RegisteredCallouts.Add(calloutType);
                Game.Console.Print($"[EmsPlus] Registered callout: {calloutType.Name}");
            }
        }

        public static void Process()
        {
            while (true)
            {
                GameFiber.Yield();

                if (CurrentCallout != null && Game.IsKeyDown(System.Windows.Forms.Keys.End))
                {
                    EndCurrent();
                }

                try
                {
                    if (CurrentCallout != null)
                    {
                        if (CurrentCallout.Finished)
                        {
                            CurrentCallout = null;
                            EmsService.SetStatus(EmsStatus.Available);
                            SetNextCalloutTime(30000, 90000);
                        }
                        else if (CurrentCallout.Accepted)
                        {
                            CurrentCallout.Process();
                        }
                        else if (IsCalloutDisplayed)
                        {
                            if (Game.GameTime > CalloutExpireTime) { DismissCurrent(); continue; }

                            if (Game.IsKeyDown(Keys.Y))
                            {
                                if (CurrentCallout.OnCalloutAccepted())
                                {
                                    IsCalloutDisplayed = false;
                                    EmsService.SetStatus(EmsStatus.EnRoute);
                                }
                                else EndCurrent();
                            }
                            else if (Game.IsKeyDown(Keys.N)) { EndCurrent(); }
                        }
                        continue;
                    }

                    if (EmsService.CurrentStatus == EmsStatus.Available && Game.GameTime > NextCalloutTime)
                    {
                        if (EntryPoint.EmsPlusConfig.CalloutMultiplier.Value > 0)
                        {
                            StartRandomCallout();
                            SetNextCalloutTime(15000, 30000);
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    Game.Console.Print($"[EmsPlus] CRITICAL ERROR: {ex}");
                    EndCurrent();
                }
            }
        }

        public static void StartRandomCallout()
        {
            if (RegisteredCallouts.Count == 0) return;

            var activeStation = Managers.StationManager.ActiveStation;
            string currentStationID = activeStation?.ID ?? "NONE";

            var eligibleTypes = RegisteredCallouts.Where(type =>
            {
                var tempCallout = (EmsCallout)Activator.CreateInstance(type);

                if (tempCallout.AllowedStationIDs.Count == 0) return true;

                bool isEligible = tempCallout.AllowedStationIDs.Any(id =>
                    id.Equals(currentStationID, StringComparison.OrdinalIgnoreCase));

                return isEligible;
            }).ToList();

            if (eligibleTypes.Count == 0) return;

            var selectedType = eligibleTypes[Rnd.Next(eligibleTypes.Count)];
            CreateCallout(selectedType);
        }

        public static void ForceCallout(string calloutName)
        {
            var type = RegisteredCallouts.FirstOrDefault(x =>
                x.Name.Equals(calloutName, StringComparison.OrdinalIgnoreCase) ||
                x.Name.Equals(calloutName + "Callout", StringComparison.OrdinalIgnoreCase)
            );

            if (type != null) CreateCallout(type);
        }

        private static void CreateCallout(Type type)
        {
            if (CurrentCallout != null) return;

            try
            {
                var callout = (EmsCallout)Activator.CreateInstance(type);
                if (callout.OnBeforeCalloutDisplayed())
                {
                    CurrentCallout = callout;
                    IsCalloutDisplayed = true;
                    CalloutExpireTime = Game.GameTime + TimeoutDurationMs;

                    GameFiber.StartNew(delegate {
                        NativeFunction.Natives.PLAY_SOUND_FRONTEND(-1, "Radio_Chatter_01", "MPC_RADIO_CHIPS_SOUNDSET", true);
                        GameFiber.Sleep(500);
                        NativeFunction.Natives.PLAY_SOUND_FRONTEND(-1, "Scanner_Resident_Report_Sec_01", "General_Resident_Scanner_Alarms", true);
                    });

                    NativeFunction.Natives.GET_STREET_NAME_AT_COORD(callout.CalloutPosition.X, callout.CalloutPosition.Y, callout.CalloutPosition.Z, out uint sHash, out uint cHash);
                    string street = NativeFunction.Natives.GET_STREET_NAME_FROM_HASH_KEY<string>(sHash);
                    string zone = NativeFunction.Natives.GET_NAME_OF_ZONE<string>(callout.CalloutPosition.X, callout.CalloutPosition.Y, callout.CalloutPosition.Z);
                    string loc = string.IsNullOrEmpty(street) ? Game.GetLocalizedString(zone) : $"{street}, {Game.GetLocalizedString(zone)}";

                    string fullText = $"~w~Call: ~b~{callout.CalloutName}~n~~w~Location: ~y~{loc}~n~~c~{callout.CalloutMessage}~n~~w~{Localization.Get("NOTIF_CALLOUT_ACCEPT_PROMPT")}";
                    Game.DisplayNotification("char_call911", "char_call911", "DISPATCH", "EMERGENCY CALL", fullText);
                }
            }
            catch (Exception e) { Game.Console.Print($"[EmsPlus] Error creating callout: {e.Message}"); }
        }

        public static void DismissCurrent()
        {
            if (CurrentCallout != null) { CurrentCallout.End(); CurrentCallout = null; }
            IsCalloutDisplayed = false;
            EmsService.SetStatus(EmsStatus.Available);
            SetNextCalloutTime(30000, 90000);
        }

        public static void EndCurrent()
        {
            if (GameState.CurrentPatient != null && GameState.CurrentPatient.Character.Exists())
            {
                if (GameState.CurrentPatient.IsOnStretcher)
                {
                    GameState.CurrentPatient.Character.Detach();
                    GameState.CurrentPatient.Character.Delete();
                }
            }

            InventoryManager.Cleanup();
            HospitalManager.CleanupBlip();
            InteriorManager.DisableTargetEntrance();

            UI.CustomMenus.InspectMenu.Managers.BodyInspectionManager.Cleanup();

            SceneManager.ClearScene();
            DismissCurrent();
        }

        private static void SetNextCalloutTime(int min, int max)
        {
            float mult = EntryPoint.EmsPlusConfig.CalloutMultiplier.Value;
            if (mult <= 0) { NextCalloutTime = uint.MaxValue; return; }
            NextCalloutTime = Game.GameTime + (uint)(Rnd.Next(min, max) / mult);
        }

        public static void ForceCleanUp() => EndCurrent();
    }
}