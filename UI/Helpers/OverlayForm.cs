using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace EmsPlus.UI.Helpers
{
    public class OverlayForm : Form
    {
        private WebView2 webView;
        private Action<string> onMessageReceived;
        private string currentLoadedFile = "";
        private bool isWebViewInitialized = false;

        public int CurrentWidth { get; set; } = 900;
        public int CurrentHeight { get; set; } = 580;
        public int CustomOffsetX { get; set; } = 0;
        public int CustomOffsetY { get; set; } = 0;

        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_TRANSPARENT = 0x00000020;
        private const int WS_EX_LAYERED = 0x00080000;
        private const int WS_EX_NOACTIVATE = 0x08000000;

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        private static extern bool ClientToScreen(IntPtr hWnd, ref POINT lpPoint);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT { public int Left; public int Top; public int Right; public int Bottom; }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT { public int X; public int Y; }

        protected override bool ShowWithoutActivation => true;

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= WS_EX_NOACTIVATE | WS_EX_LAYERED;
                return cp;
            }
        }

        public double ZoomFactor
        {
            get => webView != null ? webView.ZoomFactor : 1.0;
            set
            {
                if (this.IsDisposed || !this.IsHandleCreated || webView == null) return;

                if (this.InvokeRequired)
                {
                    this.BeginInvoke(new Action(() => ZoomFactor = value));
                    return;
                }

                try
                {
                    double clamped = Math.Max(0.2, Math.Min(3.0, value));
                    webView.ZoomFactor = clamped;
                }
                catch { }
            }
        }

        public OverlayForm(Action<string> messageCallback)
        {
            this.onMessageReceived = messageCallback;
            InitializeComponent();
            SetupForm();
            InitializeWebView();
        }

        private void SetupForm()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.TopMost = true;
            this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.Manual;
            this.Visible = false;
            this.Size = new Size(CurrentWidth, CurrentHeight);

            this.AllowTransparency = true;
            this.BackColor = Color.Magenta;
            this.TransparencyKey = Color.Magenta;
        }

        private void InitializeComponent()
        {
            this.webView = new WebView2();
            this.webView.Dock = DockStyle.Fill;
            this.Controls.Add(this.webView);
            this.Size = new Size(CurrentWidth, CurrentHeight);
        }

        private async void InitializeWebView()
        {
            try
            {
                string userDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RPH_WebView2_Cache");
                var env = await CoreWebView2Environment.CreateAsync(null, userDataFolder);

                await webView.EnsureCoreWebView2Async(env);

                webView.DefaultBackgroundColor = Color.Transparent;

                webView.CoreWebView2.Settings.AreBrowserAcceleratorKeysEnabled = false;
                webView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
                webView.CoreWebView2.Settings.AreDevToolsEnabled = false;
                webView.CoreWebView2.Settings.IsZoomControlEnabled = false;

                isWebViewInitialized = true;

                if (!string.IsNullOrEmpty(currentLoadedFile))
                {
                    NavigateTo(currentLoadedFile);
                }

                webView.WebMessageReceived += (s, e) => onMessageReceived?.Invoke(e.TryGetWebMessageAsString());
            }
            catch (Exception ex)
            {
                Console.WriteLine("[EmsPlus] WebView2 Init Error: " + ex.Message);
            }
        }

        public void DragMove(int dx, int dy)
        {
            if (this.IsDisposed || !this.IsHandleCreated) return;

            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() => DragMove(dx, dy)));
                return;
            }

            CustomOffsetX += dx;
            CustomOffsetY += dy;
            this.Left += dx;
            this.Top += dy;
        }

        public void SetMouseUnlocked(bool unlocked, IntPtr gameWindowHandle)
        {
            if (this.IsDisposed || !this.IsHandleCreated) return;

            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() => SetMouseUnlocked(unlocked, gameWindowHandle)));
                return;
            }

            int exStyle = GetWindowLong(this.Handle, GWL_EXSTYLE);

            if (unlocked)
            {
                SetWindowLong(this.Handle, GWL_EXSTYLE, (exStyle & ~WS_EX_TRANSPARENT) | WS_EX_LAYERED | WS_EX_NOACTIVATE);
            }
            else
            {
                SetWindowLong(this.Handle, GWL_EXSTYLE, exStyle | WS_EX_TRANSPARENT | WS_EX_LAYERED | WS_EX_NOACTIVATE);
            }

            if (gameWindowHandle != IntPtr.Zero)
            {
                SetForegroundWindow(gameWindowHandle);
            }
        }

        public void NavigateTo(string fileName)
        {
            if (this.IsDisposed || !this.IsHandleCreated) return;

            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() => NavigateTo(fileName)));
                return;
            }

            currentLoadedFile = fileName;

            if (!isWebViewInitialized || webView == null || webView.CoreWebView2 == null) return;

            string htmlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins", "EmsPlus", "UI", fileName);
            if (File.Exists(htmlPath))
            {
                webView.CoreWebView2.Navigate(new Uri(htmlPath).AbsoluteUri);
            }
            else
            {
                webView.CoreWebView2.NavigateToString($"<html><body style='background:transparent; color:white;'><h1>{fileName} missing in Plugins/EmsPlus/UI/</h1></body></html>");
            }
        }

        public void ExecuteScript(string script)
        {
            if (this.IsDisposed || !this.IsHandleCreated) return;

            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() => ExecuteScript(script)));
                return;
            }

            if (!isWebViewInitialized || webView == null || webView.CoreWebView2 == null) return;

            webView.ExecuteScriptAsync(script);
        }

        public void SetVisibility(bool visible, IntPtr gameWindowHandle)
        {
            if (this.IsDisposed || !this.IsHandleCreated) return;

            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() => SetVisibility(visible, gameWindowHandle)));
                return;
            }

            if (visible)
            {
                UpdateOverlayBounds(gameWindowHandle);
                this.Show();
            }
            else
            {
                this.Hide();
            }

            if (gameWindowHandle != IntPtr.Zero)
            {
                SetForegroundWindow(gameWindowHandle);
            }
        }

        public void SetSuspended(bool suspended, IntPtr gameWindowHandle)
        {
            if (this.IsDisposed || !this.IsHandleCreated) return;

            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() => SetSuspended(suspended, gameWindowHandle)));
                return;
            }

            if (suspended)
            {
                this.Hide();
            }
            else
            {
                UpdateOverlayBounds(gameWindowHandle);
                this.Show();
            }
        }

        public void UpdateOverlayBounds(IntPtr gameWindowHandle)
        {
            if (gameWindowHandle == IntPtr.Zero || this.IsDisposed || !this.IsHandleCreated) return;

            if (GetClientRect(gameWindowHandle, out RECT rect))
            {
                POINT pt = new POINT { X = 0, Y = 0 };
                ClientToScreen(gameWindowHandle, ref pt);

                int gameWidth = rect.Right - rect.Left;
                int gameHeight = rect.Bottom - rect.Top;

                int posX = pt.X + (gameWidth - CurrentWidth) / 2 + CustomOffsetX;
                int posY = pt.Y + (gameHeight - CurrentHeight) / 2 + CustomOffsetY;

                if (this.InvokeRequired)
                {
                    this.BeginInvoke(new Action(() =>
                    {
                        if (this.Left != posX || this.Top != posY || this.Width != CurrentWidth || this.Height != CurrentHeight)
                        {
                            this.SetBounds(posX, posY, CurrentWidth, CurrentHeight);
                        }
                    }));
                }
                else
                {
                    if (this.Left != posX || this.Top != posY || this.Width != CurrentWidth || this.Height != CurrentHeight)
                    {
                        this.SetBounds(posX, posY, CurrentWidth, CurrentHeight);
                    }
                }
            }
        }

        public void SetDimensions(int width, int height, IntPtr gameWindowHandle)
        {
            if (this.IsDisposed || !this.IsHandleCreated) return;

            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() => SetDimensions(width, height, gameWindowHandle)));
                return;
            }

            CurrentWidth = width;
            CurrentHeight = height;
            UpdateOverlayBounds(gameWindowHandle);
        }

        public void Shutdown()
        {
            try
            {
                if (this.IsDisposed) return;

                if (this.IsHandleCreated && this.InvokeRequired)
                {
                    this.Invoke(new Action(Shutdown));
                    return;
                }

                this.Visible = false;
                this.Hide();

                if (webView != null)
                {
                    webView.Dispose();
                    webView = null;
                }

                this.Close();
                this.Dispose();
                Application.ExitThread();
            }
            catch { }
        }
    }
}