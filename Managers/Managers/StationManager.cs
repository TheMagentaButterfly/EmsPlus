using Rage;
using Rage.Native;
using EmsPlus.Framework;
using EmsPlus.Configuration;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace EmsPlus.Managers
{
    public static class StationManager
    {
        private static Dictionary<StationLocation, Blip> _blipMap = new Dictionary<StationLocation, Blip>();
        public static StationLocation ActiveStation { get; private set; }
        private static bool _canToggle = true;

        public static void StationLoop()
        {
            while (true)
            {
                GameFiber.Yield();
                Process();
            }
        }

        public static void Initialize()
        {
            Cleanup();
            foreach (var loc in EntryPoint.StationsConfig.Locations)
            {
                try
                {
                    Blip b = new Blip(loc.Position);
                    b.Sprite = (BlipSprite)61;
                    b.Color = Color.Red;
                    b.Name = loc.Name;
                    _blipMap.Add(loc, b);
                }
                catch (System.Exception ex)
                {
                    Game.Console.Print($"[EmsPlus] Warning: Could not create blip for {loc.Name}. {ex.Message}");
                }
            }
            UpdateBlipVisibility();
        }

        public static void Process()
        {
            Vector3 playerPos = Game.LocalPlayer.Character.Position;

            if (Game.FrameCount % 500 == 0) UpdateBlipVisibility();

            foreach (var station in EntryPoint.StationsConfig.Locations)
            {
                float dist = playerPos.DistanceTo(station.Position);
                if (dist < 50f)
                {
                    NativeFunction.Natives.DRAW_MARKER(1, station.Position.X, station.Position.Y, station.Position.Z - 1.0f, 0, 0, 0, 0, 0, 0, 1.5f, 1.5f, 1.0f, 255, 0, 0, 150, false, false, 2, false, 0, 0, false);

                    if (dist < 2.0f)
                    {
                        string dutyText = EmsService.IsOnDuty ? Localization.Get("TEXT_OFF_DUTY") : Localization.Get("TEXT_ON_DUTY");
                        Game.DisplayHelp(Localization.Get("HELP_TOGGLE_DUTY", dutyText));

                        if (Game.IsControlJustPressed(0, GameControl.Context) && _canToggle)
                        {
                            HandleDutyToggle(station);
                        }
                    }
                }
            }
        }

        private static void HandleDutyToggle(StationLocation station)
        {
            _canToggle = false;
            if (!EmsService.IsOnDuty)
            {
                ActiveStation = station;
                EmsService.ToggleDuty();
                EntryPoint.StartPluginLogic();
            }
            else
            {
                ActiveStation = null;
                EmsService.ToggleDuty();
                EntryPoint.StopPluginLogic();
            }

            UpdateBlipVisibility();

            GameFiber.StartNew(delegate {
                GameFiber.Sleep(2000);
                _canToggle = true;
            });
        }

        public static void UpdateBlipVisibility()
        {
            Vector3 playerPos = Game.LocalPlayer.Character.Position;

            var sorted = _blipMap.Keys.OrderBy(x => x.Position.DistanceTo(playerPos)).ToList();

            foreach (var entry in _blipMap)
            {
                StationLocation station = entry.Key;
                Blip blip = entry.Value;

                if (!blip.Exists()) continue;

                if (EmsService.IsOnDuty && ActiveStation != null)
                {
                    if (station == ActiveStation)
                        NativeFunction.Natives.SET_BLIP_DISPLAY(blip, 2);
                    else
                        NativeFunction.Natives.SET_BLIP_DISPLAY(blip, 3);
                }
                else
                {
                    var closest3 = sorted.Take(3).ToList();
                    if (closest3.Contains(station))
                        NativeFunction.Natives.SET_BLIP_DISPLAY(blip, 2);
                    else
                        NativeFunction.Natives.SET_BLIP_DISPLAY(blip, 3);
                }
            }
        }

        // --- Helpers ---
        public static void SetDutyAtClosest()
        {
            Vector3 playerPos = Game.LocalPlayer.Character.Position;
            var closest = EntryPoint.StationsConfig.Locations
                .OrderBy(x => x.Position.DistanceTo(playerPos))
                .FirstOrDefault();

            ActiveStation = closest;
            UpdateBlipVisibility();
        }

        public static void Cleanup()
        {
            foreach (var b in _blipMap.Values) if (b.Exists()) b.Delete();
            _blipMap.Clear();
            ActiveStation = null;
        }
    }
}