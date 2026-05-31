using EmsPlus.Core;
using Rage;
using Rage.Native;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace EmsPlus.Managers
{
    public static class HospitalManager
    {
        private static Blip _routingBlip;
        private static List<Blip> _staticBlips = new List<Blip>();

        public static void Initialize()
        {
            CleanupStaticBlips();
            foreach (var loc in EntryPoint.HospitalsConfig.Locations)
            {
                try
                {
                    Blip b = new Blip(loc.Position);
                    b.Sprite = loc.IsHelipad ? BlipSprite.Helipad : BlipSprite.Hospital;
                    b.Color = Color.Green;
                    b.Name = loc.Name;

                    NativeFunction.Natives.SET_BLIP_AS_SHORT_RANGE(b, true);

                    _staticBlips.Add(b);
                }
                catch (System.Exception ex)
                {
                    Game.Console.Print($"[EmsPlus] Warning: Could not create hospital blip for {loc.Name}. {ex.Message}");
                }
            }
        }

        public static void SetWaypointToClosest()
        {
            var locs = EntryPoint.HospitalsConfig.Locations;
            if (locs.Count == 0) return;

            Vector3 playerPos = Game.LocalPlayer.Character.Position;
            var closest = locs.OrderBy(loc => loc.Position.DistanceTo(playerPos)).FirstOrDefault();

            if (closest != null)
            {
                CleanupBlip();

                _routingBlip = new Blip(closest.Position);
                _routingBlip.Color = Color.Green;
                _routingBlip.Name = $"Routing: {closest.Name}";
                _routingBlip.IsRouteEnabled = true;

                Game.DisplayNotification(Localization.Get("NOTIF_HOSPITAL_WAYPOINT_SET", "~b~Dispatch:~w~ Route to nearest hospital set."));
            }
        }

        public static void Process()
        {
            bool hasPatient = GameState.CurrentPatient != null && GameState.CurrentPatient.IsOnStretcher;
            if (!hasPatient) return;

            Vehicle drivingVeh = Game.LocalPlayer.Character.CurrentVehicle;
            if (drivingVeh == null || !drivingVeh.Exists()) return;

            foreach (var hospital in EntryPoint.HospitalsConfig.Locations)
            {
                float dist2D = drivingVeh.DistanceTo2D(hospital.Position);
                float zDiff = System.Math.Abs(drivingVeh.Position.Z - hospital.Position.Z);

                if (dist2D < 80.0f && zDiff < 15.0f)
                {
                    NativeFunction.Natives.DRAW_MARKER(
                        1, hospital.Position.X, hospital.Position.Y, hospital.Position.Z - 1.0f,
                        0, 0, 0, 0, 0, 0,
                        6.0f, 6.0f, 2.0f,
                        0, 255, 0, 100,
                        false, false, 2, false, 0, 0, false
                    );

                    if (dist2D < 6.0f)
                    {
                        Game.DisplayHelp(Localization.Get("HELP_HANDOVER_PATIENT", "Press ~INPUT_CONTEXT~ to handover patient."));

                        if (Game.IsControlJustPressed(0, GameControl.Context))
                        {
                            CompleteTransport(drivingVeh);
                        }
                    }
                }
            }
        }

        private static void CompleteTransport(Vehicle v)
        {
            CleanupBlip();

            Game.FadeScreenOut(1000);
            GameFiber.Sleep(1500);

            if (GameState.CurrentPatient != null && GameState.CurrentPatient.Character.Exists())
            {
                GameState.CurrentPatient.Character.Detach();
                GameState.CurrentPatient.Character.Delete();
            }

            GameState.Clear();
            StretcherManager.Cleanup();
            InventoryManager.Cleanup();
            InventoryManager.RestockSupplies(true);
            AmbulanceManager.ResetVehicleState(v);

            CalloutManager.EndCurrent();
            EmsService.SetStatus(EmsStatus.Available);

            Game.FadeScreenIn(1000);
            Game.DisplayNotification(Localization.Get("NOTIF_PATIENT_HANDED_OVER", "~g~Transport Complete:~w~ Patient handed over to hospital staff."));
        }

        public static void CleanupBlip()
        {
            if (_routingBlip != null && _routingBlip.Exists())
            {
                _routingBlip.Delete();
            }
            _routingBlip = null;
        }

        public static void CleanupStaticBlips()
        {
            var blips = _staticBlips.ToList();
            foreach (var b in blips)
            {
                try
                {
                    if (b != null && b.Exists()) b.Delete();

                    uint handle = b.Handle;
                    if (handle != 0) NativeFunction.Natives.REMOVE_BLIP(ref handle);
                }
                catch { }
            }
            _staticBlips.Clear();
        }
    }
}