using EmsPlus.Core;
using EmsPlus.Medical;
using EmsPlus.UI.Custom.InspectMenu;
using EmsPlus.UI.Native;
using EmsPlus.UI.Native.PatientMenu;
using Rage;
using Rage.Native;
using System;
using System.Windows.Forms;

namespace EmsPlus.Managers
{
    public static class InteractionManager
    {
        private static Keys DefaultInteractionKey => Keys.E;

        private static DateTime _lastKeyPressTime = DateTime.MinValue;
        private static uint _lastCabinToggleTime = 0;
        private static uint _lastStretcherToggleTime = 0;

        private const float PatientInteractDistance = 2.5f;
        private const float PatientMarkDistance = 6.0f;
        private const float FacingAngleDegrees = 45.0f;

        private static bool IsInteractionKeyDown()
        {
            if (EntryPoint.KeyConfig.InteractionKey != null)
                return EntryPoint.KeyConfig.InteractionKey.Value.IsPressed;

            return Game.IsKeyDown(DefaultInteractionKey);
        }

        private static bool IsCabinToggleKeyDown()
        {
            if (EntryPoint.KeyConfig.ToggleCabinKey != null)
                return EntryPoint.KeyConfig.ToggleCabinKey.Value.IsPressed;

            return Game.IsKeyDown(Keys.X);
        }

        private static bool IsStretcherToggleKeyDown()
        {
            if (EntryPoint.KeyConfig.ToggleStretcherKey != null)
                return EntryPoint.KeyConfig.ToggleStretcherKey.Value.IsPressed;

            return Game.IsKeyDown(Keys.C);
        }

        public static void MainLoop()
        {
            while (true)
            {
                GameFiber.Yield();

                DialogueManager.Process();
                TutorialManager.Process();

                if (MenuCore.IsAnyMenuOpen || BodyInspectionManager.IsActive || DialogueManager.IsActive)
                    continue;

                StationManager.Process();
                InteriorManager.Process();

                if (Game.LocalPlayer.Character.IsInAnyVehicle(false))
                {
                    HospitalManager.Process();
                }
                else
                {
                    AmbulanceManager.DrawInteractionMarkers();
                }

                // =======================================================
                // TOP-LEFT PROMPT LOGIC
                // =======================================================
                bool promptShown = false;

                // 1. Patient Prompt
                if (GameState.CurrentPatient != null && GameState.CurrentPatient.Character.Exists())
                {
                    float dist = Game.LocalPlayer.Character.DistanceTo(GameState.CurrentPatient.Character);
                    if (dist < PatientInteractDistance)
                    {
                        string keyName = EntryPoint.KeyConfig.InteractionKey.Value.ToString();
                        Game.DisplayHelp(string.Format(Localization.Get("HELP_INSPECT_PATIENT"), keyName));
                        promptShown = true;
                    }
                }

                // 2. Stretcher Prompt
                if (!promptShown && StretcherManager.Prop != null && StretcherManager.Prop.Exists() && !StretcherManager.IsAttachedToVehicle)
                {
                    StretcherManager.Prop.Model.GetDimensions(out Vector3 min, out Vector3 max);
                    float radius = Math.Max(max.X, max.Y) + 1.2f;

                    if (Game.LocalPlayer.Character.DistanceTo(StretcherManager.Prop) < radius)
                    {
                        string grabKey = EntryPoint.KeyConfig.StretcherGrabKey.Value.ToString();
                        string grabAction = StretcherManager.IsAttachedToPlayer ? "Release" : "Grab";
                        string fullPrompt = string.Format(Localization.Get("HELP_STRETCHER_CONTROL_GRAB"), grabKey, grabAction);

                        string heightKey = EntryPoint.KeyConfig.StretcherHeightKey.Value.ToString();
                        string heightAction = (StretcherManager.CurrentState == StretcherManager.StretcherState.Low)
                            ? Localization.Get("ACTION_RAISE")
                            : Localization.Get("ACTION_LOWER");
                        fullPrompt += "\n" + string.Format(Localization.Get("HELP_STRETCHER_CONTROL_HEIGHT"), heightKey, heightAction);

                        if (StretcherManager.CurrentState != StretcherManager.StretcherState.Low)
                        {
                            string sitKey = EntryPoint.KeyConfig.StretcherSitKey.Value.ToString();
                            string sitAction = (StretcherManager.CurrentState == StretcherManager.StretcherState.Sitting)
                                ? Localization.Get("ACTION_LAY")
                                : Localization.Get("ACTION_SIT");
                            fullPrompt += "\n" + string.Format(Localization.Get("HELP_STRETCHER_CONTROL_SIT"), sitKey, sitAction);
                        }

                        Game.DisplayHelp(fullPrompt);
                        promptShown = true;
                    }
                }

                // 3. Ambulance Prompt
                if (!promptShown && AmbulanceManager.IsPlayerNearInteractionPoint())
                {
                    string keyName = EntryPoint.KeyConfig.OpenAmbulanceMenuKey.Value.ToString();
                    Game.DisplayHelp(string.Format(Localization.Get("HELP_OPEN_AMBULANCE_MENU"), keyName));
                    promptShown = true;
                }

                if (EmsService.IsOnDuty)
                {
                    if (GameState.CurrentPatient == null)
                        NativeFunction.Natives.DISABLE_CONTROL_ACTION(0, 86, true);

                    if (Game.LocalPlayer.WantedLevel > 0)
                        Game.LocalPlayer.WantedLevel = 0;

                    NativeFunction.Natives.CLEAR_PLAYER_WANTED_LEVEL(Game.LocalPlayer);
                }

                try
                {
                    StretcherManager.Process();

                    // =======================================================
                    // QUICK ACTION: CABIN TOGGLE LOGIC
                    // =======================================================
                    if (IsCabinToggleKeyDown() && Game.GameTime > _lastCabinToggleTime + 1000)
                    {
                        _lastCabinToggleTime = Game.GameTime;

                        if (AmbulanceManager.IsPlayerInRearCabin)
                        {
                            AmbulanceManager.ExitRearCabin();
                        }
                        else
                        {
                            if (AmbulanceManager.TryGetClosestAmbulance(out Vehicle veh))
                            {
                                if (AmbulanceManager.CurrentConfig != null && !AmbulanceManager.CurrentConfig.CanEnterCabin)
                                {
                                    Game.DisplayNotification(Localization.Get("ERR_NO_CABIN") ?? "~r~This vehicle does not have an accessible cabin.");
                                }
                                else if (AmbulanceManager.IsStretcherLoaded)
                                {
                                    MenuCore.CloseAll();
                                    AmbulanceManager.EnterRearCabin();
                                }
                                else
                                {
                                    Game.DisplayNotification("~r~Cannot enter cabin.~w~ The stretcher must be loaded first.");
                                }
                            }
                        }
                    }
                    // =======================================================
                    // QUICK ACTION: STRETCHER TOGGLE LOGIC
                    // =======================================================
                    if (IsStretcherToggleKeyDown() && Game.GameTime > _lastStretcherToggleTime + 1000)
                    {
                        _lastStretcherToggleTime = Game.GameTime;
                        AmbulanceManager.QuickToggleStretcher();
                    }

                    // =======================================================
                    // DIALOGUE INITIATION LOGIC
                    // =======================================================
                    if (!DialogueManager.IsActive && !MenuCore.IsAnyMenuOpen)
                    {
                        // Check for Bystander
                        if (GameState.CurrentBystander?.Character.Exists() == true &&
                            !GameState.CurrentBystander.HasBeenSpokenTo &&
                            Game.LocalPlayer.Character.DistanceTo(GameState.CurrentBystander.Character) < 3.0f)
                        {
                            Game.DisplayHelp($"Press ~y~Y~w~ to talk to the witness.");
                            if (Game.IsKeyDown(Keys.Y))
                            {
                                DialogueManager.StartDialogue(
                                    GameState.CurrentBystander.Character,
                                    GameState.CurrentBystander.Dialogue
                                );
                            }
                        }
                        // Check for Conscious Patient
                        else if (GameState.CurrentPatient?.Character.Exists() == true &&
                                 !GameState.CurrentPatient.HasBeenSpokenTo &&
                                 GameState.CurrentPatient.Consciousness > ConsciousnessLevel.Pain &&
                                 Game.LocalPlayer.Character.DistanceTo(GameState.CurrentPatient.Character) < 2.0f)
                        {
                            Game.DisplayHelp($"Press ~y~Y~w~ to talk to the patient.");
                            if (Game.IsKeyDown(Keys.Y))
                            {
                                DialogueManager.StartDialogue(
                                    GameState.CurrentPatient.Character,
                                    GameState.CurrentPatient.Dialogue
                                );
                            }
                        }
                    }

                    // =======================================================
                    // MAIN INTERACTION LOGIC
                    // =======================================================
                    bool keyDown = IsInteractionKeyDown();
                    if (!keyDown) continue;

                    if (GameState.CurrentPatient != null && GameState.CurrentPatient.Character.Exists())
                    {
                        Ped patientPed = GameState.CurrentPatient.Character;
                        float dist = Game.LocalPlayer.Character.DistanceTo(patientPed);

                        if (dist < PatientInteractDistance)
                        {
                            if (MenuCore.IsAnyMenuOpen) continue;

                            if (EntryPoint.EmsPlusConfig.UseNativeUIPatientMenu.Value)
                            {
                                InventoryManager.PlaceKitOnGround(patientPed);
                                PatientMenuBuilder.RefreshAll();
                                PatientMenuBuilder.PatientMenu.Visible = true;
                            }
                            else
                            {
                                try
                                {
                                    BodyInspectionManager.StartInspection(patientPed);
                                }
                                catch (Exception ex)
                                {
                                    Game.Console.Print($"[EmsPlus] CRITICAL ERROR starting Inspection Menu: {ex}");
                                    Game.DisplayNotification("~r~Error opening menu. Check Console (F4).");
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Game.Console.Print($"[EmsPlus] InteractionLoop Error: {ex.Message}");
                    GameFiber.Sleep(1000);
                }
            }
        }
    }
}