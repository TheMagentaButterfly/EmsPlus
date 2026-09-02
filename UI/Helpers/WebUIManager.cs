using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Windows.Forms;
using Rage;
using Rage.Native;

namespace EmsPlus.UI.Helpers
{
    public static class WebUIManager
    {
        private static Thread _uiThread;
        private static OverlayForm _overlayForm;
        private static ConcurrentQueue<string> _incomingMessages = new ConcurrentQueue<string>();

        public static WebMenu ActiveMenu { get; private set; }
        public static bool IsAnyMenuOpen => ActiveMenu != null;

        public static void Initialize()
        {
            if (_uiThread != null && _uiThread.IsAlive) return;

            _uiThread = new Thread(StartUIThread) { IsBackground = true };
            _uiThread.SetApartmentState(ApartmentState.STA);
            _uiThread.Start();
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
                Game.Console.Print($"[EmsPlus] WebUIManager Thread Error: {ex.Message}");
            }
        }

        private static void OnMessageFromWeb(string message)
        {
            _incomingMessages.Enqueue(message);
        }

        public static void OpenMenu(WebMenu menu)
        {
            if (menu == null) return;

            ActiveMenu = menu;

            if (_overlayForm != null && !_overlayForm.IsDisposed)
            {
                _overlayForm.CurrentWidth = menu.DefaultWidth;
                _overlayForm.CurrentHeight = menu.DefaultHeight;
                _overlayForm.CustomOffsetX = menu.OffsetX;
                _overlayForm.CustomOffsetY = menu.OffsetY;
                _overlayForm.ZoomFactor = (double)menu.Scale;

                _overlayForm.NavigateTo(menu.HtmlFileName);
                _overlayForm.SetVisibility(true, System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle);
                _overlayForm.SetMouseUnlocked(menu.MouseUnlocked, System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle);
            }

            menu.OnOpen();
        }

        public static void CloseMenu()
        {
            if (ActiveMenu == null) return;

            var closingMenu = ActiveMenu;
            ActiveMenu = null;

            closingMenu.OnClose();

            NativeFunction.Natives.SET_MOUSE_CURSOR_VISIBLE(false);

            if (_overlayForm != null && !_overlayForm.IsDisposed)
            {
                _overlayForm.SetMouseUnlocked(false, System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle);
                _overlayForm.SetVisibility(false, System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle);
            }
        }

        public static void ToggleMenu(WebMenu menu)
        {
            if (ActiveMenu == menu)
                CloseMenu();
            else
                OpenMenu(menu);
        }

        public static void SetMouseUnlocked(bool unlocked)
        {
            if (ActiveMenu != null)
            {
                ActiveMenu.MouseUnlocked = unlocked;
                ExecuteScript($"setMouseLockState({(unlocked ? "true" : "false")})");
            }

            if (!unlocked)
            {
                NativeFunction.Natives.SET_MOUSE_CURSOR_VISIBLE(false);
            }

            if (_overlayForm != null && !_overlayForm.IsDisposed)
            {
                _overlayForm.SetMouseUnlocked(unlocked, System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle);
            }
        }

        public static void SetOverlaySuspended(bool suspended)
        {
            if (_overlayForm != null && !_overlayForm.IsDisposed)
            {
                _overlayForm.SetSuspended(suspended, System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle);
            }
        }

        public static void ExecuteScript(string script)
        {
            if (_overlayForm != null && !_overlayForm.IsDisposed)
            {
                _overlayForm.ExecuteScript(script);
            }
        }

        public static void Process()
        {
            while (_incomingMessages.TryDequeue(out string rawAction))
            {
                try
                {
                    string action = rawAction.Trim('\"', '\'', ' ');

                    if (action == "close")
                    {
                        CloseMenu();
                    }
                    else if (action == "toggle_mouse_lock")
                    {
                        if (ActiveMenu != null)
                        {
                            SetMouseUnlocked(!ActiveMenu.MouseUnlocked);
                        }
                    }
                    else if (action.StartsWith("set_dimensions:"))
                    {
                        string dimStr = action.Substring(15);
                        string[] parts = dimStr.Split(',');
                        if (parts.Length == 2 && int.TryParse(parts[0], out int w) && int.TryParse(parts[1], out int h))
                        {
                            if (ActiveMenu != null)
                            {
                                ActiveMenu.CustomWidth = w;
                                ActiveMenu.CustomHeight = h;

                                int finalW = (int)(w * ActiveMenu.Scale);
                                int finalH = (int)(h * ActiveMenu.Scale);
                                _overlayForm?.SetDimensions(finalW, finalH, System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle);
                            }
                            else
                            {
                                _overlayForm?.SetDimensions(w, h, System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle);
                            }
                        }
                    }
                    else if (action.StartsWith("drag_window:"))
                    {
                        string coords = action.Substring(12);
                        string[] parts = coords.Split(',');
                        if (parts.Length == 2 && int.TryParse(parts[0], out int dx) && int.TryParse(parts[1], out int dy))
                        {
                            _overlayForm?.DragMove(dx, dy);
                            if (ActiveMenu != null)
                            {
                                ActiveMenu.OffsetX = _overlayForm.CustomOffsetX;
                                ActiveMenu.OffsetY = _overlayForm.CustomOffsetY;
                            }
                        }
                    }
                    else if (action == "save_position")
                    {
                        ActiveMenu?.OnSavePosition();
                    }
                    else
                    {
                        ActiveMenu?.OnMessage(action);
                    }
                }
                catch (Exception ex)
                {
                    Game.Console.Print($"[EmsPlus] Web UI Action Error: {ex.Message}");
                }
            }

            if (ActiveMenu != null && ActiveMenu.MouseUnlocked)
            {
                NativeFunction.Natives.SET_MOUSE_CURSOR_THIS_FRAME();
                NativeFunction.Natives.SET_MOUSE_CURSOR_VISIBLE(true);
                NativeFunction.Natives.SET_MOUSE_CURSOR_STYLE(0);
            }

            ActiveMenu?.OnProcess();
        }

        public static void Shutdown()
        {
            try
            {
                CloseMenu();

                if (_overlayForm != null && !_overlayForm.IsDisposed)
                {
                    _overlayForm.Shutdown();
                }

                if (_uiThread != null && _uiThread.IsAlive)
                {
                    _uiThread.Join(500);
                }
            }
            catch { }
        }
    }
}