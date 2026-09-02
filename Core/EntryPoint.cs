using EmsPlus.Configuration;
using EmsPlus.Core;
using EmsPlus.Managers;
using EmsPlus.UI.Helpers;
using EmsPlus.UI.Native;
using EmsPlus.UI.Custom.InspectMenu;
using IPT.Common.API;
using IPT.Common.Handlers;
using Rage;
using Rage.Attributes;
using Rage.Native;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

[assembly: Plugin("EmsPlus", Description = "A EMS Plugin", Author = "Maggie Waggie")]

namespace EmsPlus
{
    public class EntryPoint
    {
        public static KeyConfig KeyConfig { get; private set; }
        public static PropConfig PropConfig { get; private set; }
        public static OffsetConfig OffsetConfig { get; private set; }
        public static LoadoutConfig LoadoutConfig;
        public static AnimationConfig AnimationConfig;
        public static EmsPlusConfig EmsPlusConfig;
        public static HospitalsConfig HospitalsConfig;
        public static StationsConfig StationsConfig;
        public static MedicationConfig MedicationConfig;
        public static InteriorConfig InteriorConfig;
        public static QuestionConfig QuestionConfig;
        public static BackupConfig BackupConfig;
        public static RanksConfig RanksConfig;

        private static InputHandler _inputHandler;
        private static GameFiber _mainLogicFiber, _uiLogicFiber, _simulationFiber, _calloutFiber, _stationFiber;

        public static bool IsRunning { get; private set; } = false;
        private static bool _hasUnloaded = false;

        private static OverlayForm _overlayForm;
        public static bool IsUiOpen { get; private set; } = false;
        private static ConcurrentQueue<string> _incomingMessages = new ConcurrentQueue<string>();
        private static bool _wasGamePaused = false;

        public static void Main()
        {
            InitializeDirectories();
            LoadConfigurations();

            // Initialize NativeUI and WebUI early so menus can be opened while off duty
            MenuCore.Initialize();
            WebUIManager.Initialize();

            _uiLogicFiber = new GameFiber(MenuCore.Process);
            _uiLogicFiber.Start();

            while (Game.IsLoading)
            {
                GameFiber.Sleep(200);
            }

            while (Game.LocalPlayer == null || Game.LocalPlayer.Character == null || !Game.LocalPlayer.Character.Exists())
            {
                GameFiber.Sleep(200);
            }

            GameFiber.Sleep(2000);

            StationManager.Initialize();
            HospitalManager.Initialize();
            _stationFiber = GameFiber.StartNew(StationManager.StationLoop);

            Game.Console.Print($"============[EmsPlus] v{Assembly.GetExecutingAssembly().GetName().Version} Initialized!============");
            Game.DisplayNotification("~b~E~p~m~r~s~g~Plus~w~ Loaded. Use ~y~ForceDuty~w~ to go on duty.");

            try { while (true) GameFiber.Yield(); }
            catch (System.Threading.ThreadAbortException) { OnUnload(false); }
        }

        public static void StartPluginLogic()
        {
            if (IsRunning) return;
            IsRunning = true;

            UpdateManager.CheckForUpdates();
            CalloutManager.Initialize();
            CalloutManager.RegisterCallout(typeof(Callouts.PaletoRescueCallout));
            CalloutManager.RegisterCallout(typeof(Callouts.ProceduralTraumaCallout));
            CalloutManager.RegisterCallout(typeof(Callouts.HomeEmergencyCallout));
            CalloutManager.RegisterCallout(typeof(Callouts.FallFromHeightCallout));
            CalloutManager.RegisterCallout(typeof(Callouts.PenetratingTraumaCallout));
            CalloutManager.RegisterCallout(typeof(Callouts.AnaphylaxisCallout));
            AddonManager.LoadAddons();

            BackupManager.Initialize();

            _inputHandler = new InputHandler(KeyConfig);
            _inputHandler.Start();
            Events.OnUserInputChanged += MenuCore.OnUserInputChanged;

            Game.FrameRender += OnGameFrameRender;

            _mainLogicFiber = new GameFiber(InteractionManager.MainLoop); _mainLogicFiber.Start();
            _simulationFiber = new GameFiber(SimulationLoop); _simulationFiber.Start();
            _calloutFiber = new GameFiber(CalloutManager.Process); _calloutFiber.Start();
        }

        public static void StopPluginLogic()
        {
            if (!IsRunning) return;
            IsRunning = false;
            Cleanup(false);
        }

        private static void Cleanup(bool abortFibers)
        {
            try { Game.FrameRender -= OnGameFrameRender; } catch { }

            // Station and Hospital static blips are only deleted when the plugin completely unloads
            if (abortFibers)
            {
                try { StationManager.Cleanup(); } catch { }
                try { HospitalManager.CleanupStaticBlips(); } catch { }
            }

            try { CalloutManager.ForceCleanUp(); } catch { }
            try { StretcherManager.Cleanup(); } catch { }
            try { StretcherGhostManager.DeleteGhosts(); } catch { }
            try { InventoryManager.Cleanup(); } catch { }
            try { DialogueManager.Cleanup(); } catch { }
            try { BackupManager.Shutdown(); } catch { }

            try { MenuCore.CloseAll(); } catch { }
            try { BodyInspectionManager.Cleanup(); } catch { }
            try { MdtManager.Cleanup(); } catch { }

            if (_inputHandler != null)
            {
                try { _inputHandler.Stop(); } catch { }
                try { Events.OnUserInputChanged -= MenuCore.OnUserInputChanged; } catch { }
                _inputHandler = null;
            }

            try
            {
                if (Game.LocalPlayer != null && Game.LocalPlayer.Character != null && Game.LocalPlayer.Character.Exists())
                {
                    Game.LocalPlayer.Character.Tasks.ClearImmediately();
                }
            }
            catch { }

            try { GameState.Clear(); } catch { }
            try { AmbulanceManager.Cleanup(); } catch { }
            try { InteriorManager.ForceClearInterior(); } catch { }

            AbortFiberSafe(_mainLogicFiber);
            AbortFiberSafe(_simulationFiber);
            AbortFiberSafe(_calloutFiber);

            if (abortFibers)
            {
                AbortFiberSafe(_uiLogicFiber);
                AbortFiberSafe(_stationFiber);
            }
        }

        private static void StartUIThread()
        {
            try
            {
                _overlayForm = new OverlayForm(OnMessageFromWeb);
                IntPtr forceHandle = _overlayForm.Handle;
                Application.Run();
            }
            catch (Exception ex)
            {
                Game.Console.Print($"[EmsPlus] WebView2 UI Thread Error: {ex.Message}");
            }
        }

        private static void OnMessageFromWeb(string message)
        {
            _incomingMessages.Enqueue(message);
        }

        public static void ToggleUI(bool? state = null)
        {
            if (_overlayForm == null || _overlayForm.IsDisposed) return;

            if (state.HasValue)
                IsUiOpen = state.Value;
            else
                IsUiOpen = !IsUiOpen;

            _overlayForm.SetVisibility(IsUiOpen, Process.GetCurrentProcess().MainWindowHandle);
        }

        public static void SetMouseUnlocked(bool unlocked)
        {
            if (_overlayForm != null && !_overlayForm.IsDisposed)
            {
                _overlayForm.SetMouseUnlocked(unlocked, Process.GetCurrentProcess().MainWindowHandle);
            }
        }

        public static void SetOverlaySuspended(bool suspended)
        {
            if (_overlayForm != null && !_overlayForm.IsDisposed)
            {
                _overlayForm.SetSuspended(suspended, Process.GetCurrentProcess().MainWindowHandle);
            }
        }

        public static void NavigateUI(string fileName)
        {
            if (_overlayForm != null && !_overlayForm.IsDisposed)
            {
                _overlayForm.NavigateTo(fileName);
            }
        }

        public static void ExecuteScriptOnUI(string script)
        {
            if (_overlayForm != null && !_overlayForm.IsDisposed)
            {
                _overlayForm.ExecuteScript(script);
            }
        }

        public static void ProcessIncomingMessages()
        {
            while (_incomingMessages.TryDequeue(out string rawAction))
            {
                try
                {
                    string action = rawAction.Trim('\"', '\'', ' ');

                    if (action == "close")
                    {
                        MdtManager.Toggle(false);
                    }
                    else if (action == "get_mdt_data" || action == "refresh")
                    {
                        MdtManager.PushCurrentStateToWeb();
                    }
                    else if (action == "toggle_mouse_lock")
                    {
                        MdtManager.SetMouseUnlocked(!MdtManager.IsMouseUnlocked);
                    }
                    else if (action == "save_mdt_position")
                    {
                        if (_overlayForm != null && !_overlayForm.IsDisposed && OffsetConfig != null)
                        {
                            OffsetConfig.MdtOffsetX = _overlayForm.CustomOffsetX;
                            OffsetConfig.MdtOffsetY = _overlayForm.CustomOffsetY;
                            OffsetConfig.Save();
                        }
                    }
                    else if (action.StartsWith("drag_window:"))
                    {
                        string coords = action.Substring(12);
                        string[] parts = coords.Split(',');
                        if (parts.Length == 2 && int.TryParse(parts[0], out int dx) && int.TryParse(parts[1], out int dy))
                        {
                            _overlayForm?.DragMove(dx, dy);
                        }
                    }
                    else if (action.StartsWith("set_status:"))
                    {
                        string statusString = action.Substring(11);
                        if (Enum.TryParse(statusString, out EmsStatus parsedStatus))
                        {
                            EmsService.SetStatus(parsedStatus);
                            MdtManager.PushCurrentStateToWeb();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Game.Console.Print($"[EmsPlus] Web UI Message Error: {ex.Message}");
                }
            }
        }

        private static void InitializeDirectories()
        {
            string basePlugin = Path.Combine(Application.StartupPath, "Plugins", "EmsPlus");
            string audioBase = Path.Combine(basePlugin, "Audio", "Dispatch");
            string[] paths = {
                basePlugin,
                audioBase,
                Path.Combine(audioBase, "STREETS"),
                Path.Combine(audioBase, "ZONES"),
                Path.Combine(audioBase, "CALLOUTS"),
                Path.Combine(audioBase, "GENERAL"),
                Path.Combine(basePlugin, "Assets"),
                Path.Combine(basePlugin, "Plugins"),
                Path.Combine(basePlugin, "Settings"),
                Path.Combine(basePlugin, "Settings", "Data"),
                Path.Combine(basePlugin, "Settings", "Localization"),
                Path.Combine(basePlugin, "Settings", "Vehicles"),
                Path.Combine(basePlugin, "UI")
            };

            foreach (var path in paths)
            {
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            }
        }

        private static void LoadConfigurations()
        {
            KeyConfig = new KeyConfig(); KeyConfig.Load();
            PropConfig = new PropConfig(); PropConfig.Load();
            OffsetConfig = new OffsetConfig(); OffsetConfig.Load();
            LoadoutConfig = new LoadoutConfig(); LoadoutConfig.Load();
            AnimationConfig = new AnimationConfig(); AnimationConfig.Load();
            EmsPlusConfig = new EmsPlusConfig(); EmsPlusConfig.Load();

            foreach (string modelName in EmsPlusConfig.ValidAmbulanceModels)
            {
                var vehCfg = new VehicleConfig(modelName);
                vehCfg.Load();
            }

            HospitalsConfig = new HospitalsConfig(); HospitalsConfig.Load();
            StationsConfig = new StationsConfig(); StationsConfig.Load();
            MedicationConfig = new MedicationConfig(); MedicationConfig.Load();
            InteriorConfig = new InteriorConfig(); InteriorConfig.Load();
            QuestionConfig = new QuestionConfig(); QuestionConfig.Load();
            BackupConfig = new BackupConfig(); BackupConfig.Load();
            RanksConfig = new RanksConfig(); RanksConfig.Load();
            Localization.Load();
        }

        public static void ReloadAllConfigs()
        {
            Game.Console.Print("[EmsPlus] Reloading all configurations...");

            LoadConfigurations();

            StationManager.Initialize();

            if (IsRunning)
            {
                if (_inputHandler != null)
                {
                    _inputHandler.Stop();
                    Events.OnUserInputChanged -= MenuCore.OnUserInputChanged;
                    _inputHandler = new InputHandler(KeyConfig);
                    _inputHandler.Start();
                    Events.OnUserInputChanged += MenuCore.OnUserInputChanged;
                }

                MenuCore.Initialize();
            }

            Game.DisplayNotification(Localization.Get("NOTIF_CONFIGSRELOADED", "~b~EmsPlus~w~: All configurations reloaded!"));
        }

        public static void OnUnload(bool isTerminating)
        {
            if (_hasUnloaded) return;
            _hasUnloaded = true;

            Cleanup(true);
            WebUIManager.Shutdown();

            Game.Console.Print("[EmsPlus] Unloaded.");
        }

        private static void AbortFiberSafe(GameFiber fiber)
        {
            try
            {
                if (fiber != null && fiber.IsAlive)
                {
                    fiber.Abort();
                }
            }
            catch { }
        }

        private static void SimulationLoop()
        {
            while (true)
            {
                GameFiber.Sleep(1000);
                if (GameState.CurrentPatient?.Character.Exists() ?? false)
                {
                    GameState.CurrentPatient.Update();
                }
            }
        }

        private static void OnGameFrameRender(object sender, GraphicsEventArgs e)
        {
            bool isPaused = Game.IsPaused || NativeFunction.Natives.IS_PAUSE_MENU_ACTIVE<bool>();
            if (isPaused != _wasGamePaused)
            {
                _wasGamePaused = isPaused;
                if (IsUiOpen)
                {
                    SetOverlaySuspended(isPaused);
                }
            }

            if (GameState.IsPlayerBusy || MenuCore.IsAnyMenuOpen || GameState.SuppressPrompts) return;
        }
    }
}