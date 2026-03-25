using EmsPlus.Configuration;
using EmsPlus.Core;
using EmsPlus.Framework;
using EmsPlus.Managers;
using EmsPlus.UI.NativeMenus;
using IPT.Common.API;
using IPT.Common.Handlers;
using Rage;
using Rage.Attributes;
using Rage.Native;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

[assembly: Plugin("EmsPlus", Description = "A EMS Plugin", Author = "Maggie Waggie")]

namespace EmsPlus
{
    public class EntryPoint
    {
        // Configs
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

        private static InputHandler _inputHandler;
        private static GameFiber _mainLogicFiber, _uiLogicFiber, _simulationFiber, _calloutFiber, _stationFiber;

        public static bool IsRunning { get; private set; } = false;

        public static void Main()
        {
            InitializeDirectories();
            LoadConfigurations();

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
            _stationFiber = GameFiber.StartNew(StationManager.StationLoop);

            Game.Console.Print($"============[EmsPlus] v{Assembly.GetExecutingAssembly().GetName().Version} Initialized!============");
            Game.DisplayNotification("~b~E~p~m~r~s~g~Plus~w~ Loaded. Use ~y~ForceDuty~w~ to go on duty.");

            try { while (true) GameFiber.Yield(); }
            catch (System.Threading.ThreadAbortException) { OnUnload(); }
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
                Path.Combine(basePlugin, "Settings", "Vehicles")
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
            HospitalsConfig = new HospitalsConfig(); HospitalsConfig.Load();
            StationsConfig = new StationsConfig(); StationsConfig.Load();
            MedicationConfig = new MedicationConfig(); MedicationConfig.Load();
            InteriorConfig = new InteriorConfig(); InteriorConfig.Load();
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

            Game.DisplayNotification(Localization.Get("NOTIF_CONFIGSRELOADED"));
        }

        private static void OnUnload()
        {
            Cleanup(false);
            StationManager.Cleanup();
            Game.Console.Print("[EmsPlus] Unloaded.");
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
            CalloutManager.RegisterCallout(typeof(Callouts.CardiacArrestCallout));
            CalloutManager.RegisterCallout(typeof(Callouts.FallFromHeightCallout));
            CalloutManager.RegisterCallout(typeof(Callouts.PenetratingTraumaCallout));
            CalloutManager.RegisterCallout(typeof(Callouts.AnaphylaxisCallout));
            AddonManager.LoadAddons();

            MenuCore.Initialize();

            _inputHandler = new InputHandler(KeyConfig);
            _inputHandler.Start();
            Events.OnUserInputChanged += MenuCore.OnUserInputChanged;

            Game.FrameRender += OnGameFrameRender;

            _mainLogicFiber = new GameFiber(InteractionManager.MainLoop); _mainLogicFiber.Start();
            _uiLogicFiber = new GameFiber(MenuCore.Process); _uiLogicFiber.Start();
            _simulationFiber = new GameFiber(SimulationLoop); _simulationFiber.Start();
            _calloutFiber = new GameFiber(CalloutManager.Process); _calloutFiber.Start();
        }

        public static void StopPluginLogic()
        {
            if (!IsRunning) return;
            IsRunning = false;
            Cleanup(true);
        }

        private static void Cleanup(bool abortFibers)
        {
            Game.FrameRender -= OnGameFrameRender;

            if (_inputHandler != null)
            {
                _inputHandler.Stop();
                Events.OnUserInputChanged -= MenuCore.OnUserInputChanged;
                _inputHandler = null;
            }

            Ped player = Game.LocalPlayer.Character;
            if (player.Exists())
            {
                player.Tasks.ClearImmediately();

                NativeFunction.Natives.CLEAR_PED_SECONDARY_TASK(player);

                player.IsPositionFrozen = false;
                player.IsCollisionEnabled = true;
            }

            CalloutManager.ForceCleanUp();
            StretcherManager.Cleanup();
            StretcherGhostManager.DeleteGhosts();
            InventoryManager.Cleanup();
            MenuCore.CloseAll();
            GameState.Clear();
            UI.CustomMenus.InspectMenu.Managers.BodyInspectionManager.Cleanup();
            AmbulanceManager.Cleanup();
            InteriorManager.ForceClearInterior();

            if (abortFibers)
            {
                _mainLogicFiber?.Abort();
                _uiLogicFiber?.Abort();
                _simulationFiber?.Abort();
                _calloutFiber?.Abort();
                _stationFiber?.Abort();
            }
        }

        private static void SimulationLoop()
        {
            while (true)
            {
                GameFiber.Sleep(1000); // 1 second update tick is sufficient
                if (GameState.CurrentPatient?.Character.Exists() ?? false)
                {
                    GameState.CurrentPatient.Update();
                }
            }
        }

        private static void OnGameFrameRender(object sender, GraphicsEventArgs e)
        {
            if (GameState.IsPlayerBusy || MenuCore.IsAnyMenuOpen || GameState.SuppressPrompts) return;

            StretcherManager.DrawPrompt(e.Graphics);
        }
    }
}