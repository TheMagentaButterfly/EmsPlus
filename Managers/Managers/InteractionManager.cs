using EmsPlus.Core;
using EmsPlus.Framework;
using EmsPlus.UI.NativeMenus;
using EmsPlus.UI.NativeMenus.PatientMenu;
using Rage;
using Rage.Native;
using System;
using System.Linq;
using System.Windows.Forms;

namespace EmsPlus.Managers
{
    public static class InteractionManager
    {
        private static Keys DefaultInteractionKey => Keys.E;

        private static DateTime _lastKeyPressTime = DateTime.MinValue;
        private static readonly double DoubleTapWindowMs = 400.0;

        private const float PatientInteractDistance = 2.5f;
        private const float PatientMarkDistance = 6.0f;
        private const float FacingAngleDegrees = 45.0f;

        private static bool IsInteractionKeyDown()
        {
            if (EntryPoint.KeyConfig.InteractionKey != null)
                return EntryPoint.KeyConfig.InteractionKey.Value.IsPressed;

            return Game.IsKeyDown(DefaultInteractionKey);
        }

        /// <summary>
        /// Returns true on the tick the key goes down, and whether it was a double-tap.
        /// </summary>
        private static bool CheckDoubleTap()
        {
            if (!IsInteractionKeyDown()) return false;

            DateTime now = DateTime.UtcNow;
            double elapsed = (now - _lastKeyPressTime).TotalMilliseconds;
            _lastKeyPressTime = now;

            return elapsed <= DoubleTapWindowMs;
        }

        /// <summary>
        /// Finds the closest valid ped within <see cref="PatientMarkDistance"/> that
        /// the player is roughly facing. Returns null if none found.
        /// </summary>
        private static Ped GetFacedNearbyPed()
        {
            Ped player = Game.LocalPlayer.Character;
            Vector3 playerPos = player.Position;
            Vector3 playerForward = player.ForwardVector;

            Ped[] nearby = World.GetAllPeds()
                .Where(p => p.Exists()
                         && p != player
                         && !p.IsDead
                         && p.DistanceTo(playerPos) <= PatientMarkDistance)
                .ToArray();

            Ped best = null;
            float bestDot = -1f;

            foreach (Ped ped in nearby)
            {
                Vector3 toTarget = (ped.Position - playerPos);
                toTarget.Normalize();

                float dot = Vector3.Dot(playerForward, toTarget);

                float threshold = (float)Math.Cos(FacingAngleDegrees * Math.PI / 180.0);

                if (dot >= threshold && dot > bestDot)
                {
                    bestDot = dot;
                    best = ped;
                }
            }

            return best;
        }

        private static void MarkAsPatient(Ped ped)
        {
            if (ped == null || !ped.Exists()) return;

            if (GameState.CurrentPatient != null && GameState.CurrentPatient.Character == ped)
            {
                return;
            }

            var newPatient = new Patient(ped);
            GameState.CurrentPatient = newPatient;
        }

        public static void MainLoop()
        {
            while (true)
            {
                GameFiber.Yield();

                StationManager.Process();

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

                    bool keyDown = IsInteractionKeyDown();
                    if (!keyDown) continue;

                    bool isDoubleTap = CheckDoubleTap();

                    if (isDoubleTap && !MenuCore.IsAnyMenuOpen)
                    {
                        Ped faced = GetFacedNearbyPed();
                        if (faced != null)
                        {
                            MarkAsPatient(faced);
                            continue;
                        }
                        continue;
                    }

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
                                    UI.CustomMenus.InspectMenu.Managers.BodyInspectionManager.StartInspection(patientPed);
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