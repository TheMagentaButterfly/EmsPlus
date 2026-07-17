using EmsPlus.Core;
using EmsPlus.Medical;
using EmsPlus.UI.Custom.InspectMenu;
using EmsPlus.UI.Native;
using EmsPlus.UI.Native.PatientMenu;
using EmsPlus.UI.Native.BackupMenu;
using Rage;
using Rage.Native;
using System;
using System.Windows.Forms;

namespace EmsPlus.Managers
{
    public static class InteractionManager
    {
        private static DateTime _lastKeyPressTime = DateTime.MinValue;
        private static uint _lastCabinToggleTime = 0;
        private static uint _lastStretcherToggleTime = 0;

        private const float PatientInteractDistance = 2.5f;
        private const float PatientMarkDistance = 6.0f;
        private const float FacingAngleDegrees = 45.0f;

        private static uint _mdtHoldStartTime = 0;
        private static bool _mdtKeyWasDown = false;
        private static bool _mdtHoldTriggered = false;

        private static bool IsInteractionKeyDown()
        {
            if (EntryPoint.KeyConfig.InteractionKey != null)
                return EntryPoint.KeyConfig.InteractionKey.Value.IsPressed;

            return Game.IsKeyDown(Keys.E);
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

                bool isMdtDown = EntryPoint.KeyConfig.OpenMdtKey?.Value?.IsPressed ?? Game.IsKeyDown(Keys.F5);
                if (isMdtDown)
                {
                    if (!_mdtKeyWasDown)
                    {
                        _mdtKeyWasDown = true;
                        _mdtHoldStartTime = Game.GameTime;
                        _mdtHoldTriggered = false;
                    }
                    else if (!_mdtHoldTriggered && (Game.GameTime - _mdtHoldStartTime > 300)) // 300ms hold
                    {
                        _mdtHoldTriggered = true;
                        if (!MdtManager.IsVisible) MdtManager.Toggle(true);
                        MdtManager.SetMouseUnlocked(true);
                    }
                }
                else
                {
                    if (_mdtKeyWasDown)
                    {
                        _mdtKeyWasDown = false;
                        if (!_mdtHoldTriggered)
                        {
                            MdtManager.Toggle(!MdtManager.IsVisible);
                        }
                    }
                }

                if (MdtManager.IsVisible)
                {
                    MdtManager.Process();

                    if (_mdtHoldTriggered && MdtManager.IsVisible)
                    {
                        MdtManager.SetMouseUnlocked(true);
                    }
                }

                DialogueManager.Process();
                TutorialManager.Process();

                if (MenuCore.IsAnyMenuOpen || BodyInspectionManager.IsActive || DialogueManager.IsActive)
                    continue;

                StationManager.Process();
                InteriorManager.Process();
                BackupManager.Process();

                if (Game.LocalPlayer.Character.IsInAnyVehicle(false))
                {
                    HospitalManager.Process();
                }
                else
                {
                    AmbulanceManager.DrawInteractionMarkers();
                }

                bool promptShown = false;

                if (GameState.CurrentPatient != null && GameState.CurrentPatient.Character.Exists())
                {
                    float dist = Game.LocalPlayer.Character.DistanceTo(GameState.CurrentPatient.Character);
                    if (dist < PatientInteractDistance)
                    {
                        bool isPatientLoaded = GameState.CurrentPatient.IsOnStretcher && AmbulanceManager.IsStretcherLoaded;

                        if (!isPatientLoaded || AmbulanceManager.IsPlayerInRearCabin)
                        {
                            string keyName = EntryPoint.KeyConfig.InteractionKey.Value.ToString();
                            Game.DisplayHelp(Localization.GetFormat("HELP_INSPECT_PATIENT", "Press ~y~{0}~w~ to inspect the patient.", keyName));
                            promptShown = true;
                        }
                    }
                }

                if (!promptShown && GameState.CurrentBystander?.Character.Exists() == true && !GameState.CurrentBystander.HasBeenSpokenTo)
                {
                    if (Game.LocalPlayer.Character.DistanceTo(GameState.CurrentBystander.Character) < 3.0f)
                    {
                        Game.DisplayHelp(Localization.Get("HELP_TALK_BYSTANDER", "Press ~y~Y~w~ to talk to the witness."));
                        promptShown = true;
                    }
                }

                if (!promptShown && StretcherManager.Prop != null && StretcherManager.Prop.Exists() && !StretcherManager.IsAttachedToVehicle)
                {
                    StretcherManager.Prop.Model.GetDimensions(out Vector3 min, out Vector3 max);
                    float radius = Math.Max(max.X, max.Y) + 1.2f;

                    if (Game.LocalPlayer.Character.DistanceTo(StretcherManager.Prop) < radius)
                    {
                        string grabKey = EntryPoint.KeyConfig.StretcherGrabKey.Value.ToString();
                        string grabAction = StretcherManager.IsAttachedToPlayer ? "Release" : "Grab";
                        string fullPrompt = Localization.GetFormat("HELP_STRETCHER_CONTROL_GRAB", "~y~{0}~w~: {1} Stretcher", grabKey, grabAction);

                        string heightKey = EntryPoint.KeyConfig.StretcherHeightKey.Value.ToString();
                        string heightAction = (StretcherManager.CurrentState == StretcherManager.StretcherState.Low) ? (Localization.Get("ACTION_RAISE", "Raise")) : (Localization.Get("ACTION_LOWER", "Lower"));
                        fullPrompt += "\n" + Localization.GetFormat("HELP_STRETCHER_CONTROL_HEIGHT", "~y~{0}~w~: {1}", heightKey, heightAction);

                        if (StretcherManager.CurrentState != StretcherManager.StretcherState.Low)
                        {
                            string sitKey = EntryPoint.KeyConfig.StretcherSitKey.Value.ToString();
                            string sitAction = (StretcherManager.CurrentState == StretcherManager.StretcherState.Sitting) ? (Localization.Get("ACTION_LAY", "Lay Patient Down")) : (Localization.Get("ACTION_SIT", "Sit Patient Up"));
                            fullPrompt += "\n" + Localization.GetFormat("HELP_STRETCHER_CONTROL_SIT", "~y~{0}~w~: {1}", sitKey, sitAction);
                        }

                        Game.DisplayHelp(fullPrompt);
                        promptShown = true;
                    }
                }

                if (!promptShown && AmbulanceManager.IsPlayerNearInteractionPoint())
                {
                    string keyName = EntryPoint.KeyConfig.OpenAmbulanceMenuKey.Value.ToString();
                    Game.DisplayHelp(Localization.GetFormat("HELP_AMBULANCE_MENU", "Press ~y~{0}~w~ to open the equipment menu.", keyName));
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
                                    Game.DisplayNotification(Localization.Get("ERR_NO_CABIN", "~r~This vehicle does not have an accessible cabin."));
                                }
                                else if (AmbulanceManager.IsStretcherLoaded)
                                {
                                    MenuCore.CloseAll();
                                    AmbulanceManager.EnterRearCabin();
                                }
                                else
                                {
                                    Game.DisplayNotification(Localization.Get("ERR_NO_STRETCHER_FOR_CABIN", "~r~Cannot enter cabin.~w~ The stretcher must be loaded first."));
                                }
                            }
                        }
                    }
                    if (IsStretcherToggleKeyDown() && Game.GameTime > _lastStretcherToggleTime + 1000)
                    {
                        _lastStretcherToggleTime = Game.GameTime;
                        AmbulanceManager.QuickToggleStretcher();
                    }

                    if (!DialogueManager.IsActive && !MenuCore.IsAnyMenuOpen)
                    {
                        if (GameState.CurrentBystander?.Character.Exists() == true &&
                            !GameState.CurrentBystander.HasBeenSpokenTo &&
                            Game.LocalPlayer.Character.DistanceTo(GameState.CurrentBystander.Character) < 3.0f)
                        {
                            if (Game.IsKeyDown(Keys.Y))
                            {
                                DialogueManager.StartDialogue(
                                    GameState.CurrentBystander.Character,
                                    GameState.CurrentBystander.Dialogue
                                );
                            }
                        }
                        else if (GameState.CurrentPatient?.Character.Exists() == true &&
                                 !GameState.CurrentPatient.HasBeenSpokenTo &&
                                 GameState.CurrentPatient.Consciousness > ConsciousnessLevel.Pain &&
                                 Game.LocalPlayer.Character.DistanceTo(GameState.CurrentPatient.Character) < 2.0f)
                        {
                            if (Game.IsKeyDown(Keys.Y))
                            {
                                DialogueManager.StartDialogue(
                                    GameState.CurrentPatient.Character,
                                    GameState.CurrentPatient.Dialogue
                                );
                            }
                        }
                    }

                    if (EmsService.IsOnDuty && !MenuCore.IsAnyMenuOpen)
                    {
                        bool commandModPressed = EntryPoint.KeyConfig.OpenBackupManagerMenuKeyModifier?.Value?.IsPressed ?? Game.IsKeyDown(Keys.LShiftKey);
                        bool commandKeyPressed = EntryPoint.KeyConfig.OpenBackupManagerMenuKey?.Value?.IsPressed ?? Game.IsKeyDown(Keys.U);

                        if (commandModPressed && commandKeyPressed)
                        {
                            BackupManagerMenuBuilder.BackupManagerMenu.Visible = true;
                        }
                        else if (!commandModPressed && !commandKeyPressed && (EntryPoint.KeyConfig.OpenBackupMenuKey?.Value?.IsPressed ?? Game.IsKeyDown(Keys.B)))
                        {
                            BackupMenuBuilder.BackupMenu.Visible = true;
                        }
                    }

                    bool keyDown = IsInteractionKeyDown();
                    if (!keyDown) continue;
                    if (IsInteractionKeyDown())
                    {
                        if (GameState.CurrentPatient != null && Game.LocalPlayer.Character.DistanceTo(GameState.CurrentPatient.Character) < PatientInteractDistance)
                        {
                            bool isPatientLoaded = GameState.CurrentPatient.IsOnStretcher && AmbulanceManager.IsStretcherLoaded;

                            if (!isPatientLoaded || AmbulanceManager.IsPlayerInRearCabin)
                            {
                                if (EntryPoint.EmsPlusConfig.UseNativeUIPatientMenu.Value)
                                {
                                    InventoryManager.PlaceKitsOnGround(GameState.CurrentPatient.Character);
                                    PatientMenuBuilder.RefreshAll();
                                    PatientMenuBuilder.PatientMenu.Visible = true;
                                }
                                else
                                {
                                    BodyInspectionManager.StartInspection(GameState.CurrentPatient.Character);
                                }
                            }
                            else
                            {
                                Game.DisplayNotification(Localization.Get("ERR_MUST_ENTER_CABIN", "~r~You must enter the patient cabin to inspect the patient."));
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