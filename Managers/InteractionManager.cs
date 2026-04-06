using EmsPlus.Core;
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
                                if (AmbulanceManager.IsStretcherLoaded)
                                {
                                    MenuCore.CloseAll();
                                    AmbulanceManager.EnterRearCabin();
                                }
                                //else { Game.DisplayNotification("~r~Cannot enter cabin.~w~ The stretcher must be loaded first."); }
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