using EmsPlus.Managers;
using EmsPlus.UI.NativeMenus.ConfigMenu;
using EmsPlus.UI.NativeMenus.PatientMenu;
using IPT.Common.User.Inputs;
using Rage;
using RAGENativeUI;

namespace EmsPlus.UI.NativeMenus
{
    public static class MenuCore
    {
        private static MenuPool _menuPool = new MenuPool();

        public static UIMenu MainMenu, DiagnosticsMenu, TreatmentsMenu, IvMenu, StretcherMenu;
        public static UIMenu PatientMenu => PatientMenuBuilder.PatientMenu;
        public static UIMenu AmbulanceMenu;
        public static UIMenu SettingsMenu;

        public static bool IsAnyMenuOpen => _menuPool.IsAnyMenuOpen();

        public static void Initialize()
        {
            _menuPool.Clear();

            MenuHelpers.Initialize();

            PatientMenuBuilder.Build();
            AmbulanceMenuBuilder.Build();
            ConfigMenuBuilder.Build();

            AmbulanceManager.OnStateUpdate += AmbulanceMenuBuilder.RefreshState;
            StretcherManager.OnUpdate += AmbulanceMenuBuilder.RefreshState;
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
        }

        public static void OnUserInputChanged(GenericCombo combo)
        {
            bool inVehicle = Game.LocalPlayer.Character.IsInAnyVehicle(false);

            if (CustomMenus.InspectMenu.Managers.BodyInspectionManager.IsActive) return;
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
                        AmbulanceMenuBuilder.RefreshState();
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