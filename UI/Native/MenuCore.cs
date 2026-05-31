using EmsPlus.Managers;
using EmsPlus.UI.Custom.InspectMenu;
using EmsPlus.UI.Native.ConfigMenu;
using EmsPlus.UI.Native.PatientMenu;
using EmsPlus.UI.Native.BackupMenu;
using IPT.Common.User.Inputs;
using Rage;
using RAGENativeUI;

namespace EmsPlus.UI.Native
{
    public static class MenuCore
    {
        private static MenuPool _menuPool = new MenuPool();
        public static UIMenu ActiveMenu { get; private set; }
        private static uint _lastMenuCloseTime = 0;
        public static UIMenu MainMenu, DiagnosticsMenu, TreatmentsMenu, IvMenu, StretcherMenu;
        public static UIMenu PatientMenu => PatientMenuBuilder.PatientMenu;
        public static UIMenu AmbulanceMenu;
        public static UIMenu SettingsMenu;

        public static bool IsAnyMenuOpen => _menuPool.IsAnyMenuOpen() || Rage.Game.GameTime < _lastMenuCloseTime + 250;

        public static void Initialize()
        {
            _menuPool.Clear();

            MenuHelpers.Initialize();

            PatientMenuBuilder.Build();
            Native.AmbulanceMenu.Build();
            ConfigMenuBuilder.Build();
            BackupMenuBuilder.Build();
            BackupManagerMenuBuilder.Build();

            AmbulanceManager.OnStateUpdate += Native.AmbulanceMenu.RefreshState;
            StretcherManager.OnUpdate += Native.AmbulanceMenu.RefreshState;
        }

        public static void Process()
        {
            while (true)
            {
                GameFiber.Yield();

                try
                {
                    _menuPool.ProcessMenus();
                }
                catch (System.Threading.ThreadAbortException)
                {
                    break;
                }
                catch (System.InvalidOperationException)
                {
                }
                catch (System.Exception ex)
                {
                    Game.Console.Print($"[EmsPlus] MenuCore Warning: {ex.Message}");
                }
            }
        }

        public static void CloseAll() => _menuPool.CloseAllMenus();

        public static void AddMenu(UIMenu menu)
        {
            menu.MouseControlsEnabled = false;
            _menuPool.Add(menu);

            menu.OnMenuOpen += (s) => ActiveMenu = s;

            menu.OnMenuClose += (s) => {
                if (ActiveMenu == s) ActiveMenu = null;
                _lastMenuCloseTime = Rage.Game.GameTime;
            };
        }

        public static void OnUserInputChanged(GenericCombo combo)
        {
            bool inVehicle = Game.LocalPlayer.Character.IsInAnyVehicle(false);

            if (BodyInspectionManager.IsActive) return;
            // Settings Menu Logic
            if (EntryPoint.KeyConfig.OpenMenuKey != null && combo == EntryPoint.KeyConfig.OpenMenuKey.Value)
            {
                if (combo.IsPressed)
                {
                    if (AmbulanceMenu != null && AmbulanceMenu.Visible) AmbulanceMenu.Visible = false;

                    if (SettingsMenu != null)
                    {
                        SettingsMenu.Visible = !SettingsMenu.Visible;
                        if (!SettingsMenu.Visible)
                        {
                            StretcherGhostManager.DeleteGhosts();
                            _menuPool.CloseAllMenus();
                        }
                    }
                }
                return;
            }

            // Ambulance Menu Logic
            if (EntryPoint.KeyConfig.OpenAmbulanceMenuKey != null && combo == EntryPoint.KeyConfig.OpenAmbulanceMenuKey.Value)
            {
                if (combo.IsPressed)
                {
                    bool canInteract = false;

                    if (AmbulanceManager.IsPlayerInRearCabin)
                    {
                        canInteract = true;
                    }
                    else if (EntryPoint.EmsPlusConfig.UseCustomInteractionPoints.Value)
                    {
                        canInteract = AmbulanceManager.IsPlayerNearInteractionPoint();
                    }
                    else
                    {
                        canInteract = AmbulanceManager.IsPlayerNearAmbulance();
                    }

                    if (canInteract)
                    {
                        CloseAll();
                        Native.AmbulanceMenu.RefreshState();
                        if (AmbulanceMenu != null) AmbulanceMenu.Visible = true;
                    }
                    else
                    {
                        if (AmbulanceMenu != null) AmbulanceMenu.Visible = false;
                    }
                }
            }
        }
    }
}