using EmsPlus.Core;
using Rage;
using Rage.Native;
using System.Drawing;
using System.IO;
using System.Linq;

namespace EmsPlus.Managers
{
    public enum MdtPage { Main, StatusOverlay }

    public static class MdtManager
    {
        public static bool IsVisible { get; private set; } = false;
        public static MdtPage CurrentPage { get; private set; } = MdtPage.Main;
        private static Texture _bgTexture;

        private static float MdtW => Game.Resolution.Width * 0.45f;
        private static float MdtH => MdtW * (1080f / 1920f);
        private static float MdtX => Game.Resolution.Width - MdtW - 25f;
        private static float MdtY => Game.Resolution.Height - MdtH - 25f;

        private static float FontScale => Game.Resolution.Height / 1080f;

        private static RectangleF BtnHome => new RectangleF(MdtX + (MdtW * 0.084f), MdtY + (MdtH * 0.849f), MdtW * 0.12f, MdtH * 0.06f);
        private static RectangleF BtnStatus => new RectangleF(MdtX + (MdtW * 0.796f), MdtY + (MdtH * 0.849f), MdtW * 0.12f, MdtH * 0.06f);

        private static RectangleF BtnAvailable => new RectangleF(MdtX + (MdtW * 0.3f), MdtY + (MdtH * 0.3f), MdtW * 0.18f, MdtH * 0.1f);
        private static RectangleF BtnEnRoute => new RectangleF(MdtX + (MdtW * 0.52f), MdtY + (MdtH * 0.3f), MdtW * 0.18f, MdtH * 0.1f);
        private static RectangleF BtnOnScene => new RectangleF(MdtX + (MdtW * 0.3f), MdtY + (MdtH * 0.45f), MdtW * 0.18f, MdtH * 0.1f);
        private static RectangleF BtnTransporting => new RectangleF(MdtX + (MdtW * 0.52f), MdtY + (MdtH * 0.45f), MdtW * 0.18f, MdtH * 0.1f);
        private static RectangleF BtnAtStation => new RectangleF(MdtX + (MdtW * 0.3f), MdtY + (MdtH * 0.6f), MdtW * 0.18f, MdtH * 0.1f);
        private static RectangleF BtnBusy => new RectangleF(MdtX + (MdtW * 0.52f), MdtY + (MdtH * 0.6f), MdtW * 0.18f, MdtH * 0.1f);
        private static RectangleF BtnStatusBack => new RectangleF(MdtX + (MdtW * 0.4f), MdtY + (MdtH * 0.8f), MdtW * 0.2f, MdtH * 0.08f);

        public static void Toggle(bool state)
        {
            if (IsVisible == state) return;

            IsVisible = state;
            if (state)
            {
                CurrentPage = MdtPage.Main;

                string texPath = Core.Assets.GetPath("MdtBackground.png");
                if (File.Exists(texPath) && _bgTexture == null)
                {
                    _bgTexture = Game.CreateTextureFromFile(texPath);
                }

                Game.FrameRender += OnRender;
            }
            else
            {
                Game.FrameRender -= OnRender;
                NativeFunction.Natives.SET_MOUSE_CURSOR_VISIBLE(false);
            }
        }

        public static void Process()
        {
            if (!IsVisible) return;

            NativeFunction.Natives.DISABLE_ALL_CONTROL_ACTIONS(0);
            NativeFunction.Natives.ENABLE_CONTROL_ACTION(0, 239, true);
            NativeFunction.Natives.ENABLE_CONTROL_ACTION(0, 240, true);
            NativeFunction.Natives.ENABLE_CONTROL_ACTION(0, 24, true);
            NativeFunction.Natives.ENABLE_CONTROL_ACTION(0, 237, true);

            NativeFunction.Natives.SET_MOUSE_CURSOR_VISIBLE(true);

            if (NativeFunction.Natives.IS_DISABLED_CONTROL_JUST_PRESSED<bool>(0, 24) ||
                NativeFunction.Natives.IS_DISABLED_CONTROL_JUST_PRESSED<bool>(0, 237))
            {
                HandleClick();
            }
        }

        private static PointF GetMousePosition()
        {
            float cX = NativeFunction.Natives.GET_CONTROL_NORMAL<float>(0, 239) * Game.Resolution.Width;
            float cY = NativeFunction.Natives.GET_CONTROL_NORMAL<float>(0, 240) * Game.Resolution.Height;
            return new PointF(cX, cY);
        }

        private static void HandleClick()
        {
            PointF mouse = GetMousePosition();

            if (CurrentPage == MdtPage.Main)
            {
                if (BtnHome.Contains(mouse)) { Toggle(false); UI.Helpers.AudioHelper.PlayBack(); }
                else if (BtnStatus.Contains(mouse)) { CurrentPage = MdtPage.StatusOverlay; UI.Helpers.AudioHelper.PlaySelect(); }
            }
            else if (CurrentPage == MdtPage.StatusOverlay)
            {
                if (BtnStatusBack.Contains(mouse)) { CurrentPage = MdtPage.Main; UI.Helpers.AudioHelper.PlayBack(); }
                else if (BtnAvailable.Contains(mouse)) { EmsService.SetStatus(EmsStatus.Available); Toggle(false); UI.Helpers.AudioHelper.PlaySuccess(); }
                else if (BtnEnRoute.Contains(mouse)) { EmsService.SetStatus(EmsStatus.EnRoute); Toggle(false); UI.Helpers.AudioHelper.PlaySuccess(); }
                else if (BtnOnScene.Contains(mouse)) { EmsService.SetStatus(EmsStatus.OnScene); Toggle(false); UI.Helpers.AudioHelper.PlaySuccess(); }
                else if (BtnTransporting.Contains(mouse)) { EmsService.SetStatus(EmsStatus.Transporting); Toggle(false); UI.Helpers.AudioHelper.PlaySuccess(); }
                else if (BtnAtStation.Contains(mouse)) { EmsService.SetStatus(EmsStatus.AvailableAtStation); Toggle(false); UI.Helpers.AudioHelper.PlaySuccess(); }
                else if (BtnBusy.Contains(mouse)) { EmsService.SetStatus(EmsStatus.Busy); Toggle(false); UI.Helpers.AudioHelper.PlaySuccess(); }
            }
        }

        private static void OnRender(object sender, GraphicsEventArgs e)
        {
            if (!IsVisible) return;
            var g = e.Graphics;
            PointF mouse = GetMousePosition();

            if (_bgTexture != null)
            {
                g.DrawTexture(_bgTexture, new RectangleF(MdtX, MdtY, MdtW, MdtH));
            }
            else
            {
                g.DrawRectangle(new RectangleF(MdtX, MdtY, MdtW, MdtH), Color.FromArgb(240, 20, 20, 20));
                DrawTextCentered(g, "Missing MdtBackground.png in Assets folder!", MdtX + (MdtW / 2), MdtY + 20f, 16f * FontScale, Color.Red);
            }

            if (CurrentPage == MdtPage.Main)
            {
                if (BtnHome.Contains(mouse)) g.DrawRectangle(BtnHome, Color.FromArgb(60, 255, 255, 255));
                if (BtnStatus.Contains(mouse)) g.DrawRectangle(BtnStatus, Color.FromArgb(60, 255, 255, 255));
            }
            if (CurrentPage == MdtPage.Main) DrawCalloutDataText(g);
            else if (CurrentPage == MdtPage.StatusOverlay) DrawStatusPanel(g, mouse);

            float sz = 6f;
            g.DrawRectangle(new RectangleF(mouse.X - (sz / 2f), mouse.Y - (sz / 2f), sz, sz), Color.White);
            g.DrawRectangle(new RectangleF(mouse.X - sz, mouse.Y - sz, sz * 2, sz * 2), Color.FromArgb(150, 0, 180, 255));
        }

        private static void DrawCalloutDataText(Rage.Graphics g)
        {
            float scale = 15f * FontScale;
            Color textColor = Color.Black;

            string incType = CalloutManager.ActiveCallout != null ? CalloutManager.ActiveCallout.CalloutName : "Standby";
            string loc = CalloutManager.ActiveCallout != null ? Truncate(CalloutManager.CalloutLocationString, 38) : "---";
            string time = CalloutManager.ActiveCallout != null ? $"[{CalloutManager.CalloutAcceptTime}] " : "";

            var p = GameState.CurrentPatient;
            string patFullName = p != null ? Truncate(p.Details.FullName, 38) : "Pending...";
            string patLastName = p != null ? Truncate(p.Details.LastName, 15) : "---";
            string patFirstName = p != null ? Truncate(p.Details.FirstName, 15) : "---";
            string patDOB = p != null ? p.Details.DateOfBirth : "---";
            string patGender = p != null ? p.Details.Gender : "---";
            string patAge = p != null ? p.Details.Age.ToString() : "---";

            float col1X = MdtX + (MdtW * 0.09f);
            g.DrawText($"{time}{incType}", "Arial", scale, new PointF(col1X, MdtY + (MdtH * 0.25f)), textColor);
            g.DrawText(loc, "Arial", scale, new PointF(col1X, MdtY + (MdtH * 0.385f)), textColor);
            g.DrawText(patFullName, "Arial", scale, new PointF(col1X, MdtY + (MdtH * 0.515f)), textColor);
            g.DrawText("LSPD Dispatch", "Arial", scale, new PointF(col1X, MdtY + (MdtH * 0.645f)), textColor);

            float col2X = MdtX + (MdtW * 0.53f);
            float col3X = MdtX + (MdtW * 0.75f);

            g.DrawText(patLastName, "Arial", scale, new PointF(col2X, MdtY + (MdtH * 0.25f)), textColor);
            g.DrawText(patFirstName, "Arial", scale, new PointF(col3X, MdtY + (MdtH * 0.25f)), textColor);
            g.DrawText("Medical Emergency", "Arial", scale, new PointF(col2X, MdtY + (MdtH * 0.385f)), textColor);
            g.DrawText(loc, "Arial", scale, new PointF(col2X, MdtY + (MdtH * 0.515f)), textColor);
            g.DrawText(patDOB, "Arial", scale, new PointF(col2X, MdtY + (MdtH * 0.645f)), textColor);
            g.DrawText(patGender, "Arial", scale, new PointF(col3X, MdtY + (MdtH * 0.645f)), textColor);
            g.DrawText(patAge, "Arial", scale, new PointF(col2X, MdtY + (MdtH * 0.775f)), textColor);
        }

        private static void DrawStatusPanel(Rage.Graphics g, PointF mouse)
        {
            g.DrawRectangle(new RectangleF(MdtX, MdtY, MdtW, MdtH), Color.FromArgb(200, 10, 10, 10));

            g.DrawRectangle(new RectangleF(MdtX + (MdtW * 0.15f), MdtY + (MdtH * 0.15f), MdtW * 0.7f, MdtH * 0.7f), Color.FromArgb(255, 20, 25, 30));

            DrawTextCentered(g, "SET EMS STATUS", MdtX + (MdtW * 0.5f), MdtY + (MdtH * 0.18f), 20f * FontScale, Color.DeepSkyBlue);

            DrawStatusBtn(g, BtnAvailable, "AVAILABLE", Color.Green, mouse);
            DrawStatusBtn(g, BtnEnRoute, "EN ROUTE", Color.Orange, mouse);
            DrawStatusBtn(g, BtnOnScene, "ON SCENE", Color.Orange, mouse);
            DrawStatusBtn(g, BtnTransporting, "TRANSPORTING", Color.Orange, mouse);
            DrawStatusBtn(g, BtnAtStation, "AT STATION", Color.Green, mouse);
            DrawStatusBtn(g, BtnBusy, "BUSY", Color.Red, mouse);

            g.DrawRectangle(BtnStatusBack, Color.FromArgb(255, 50, 60, 70));
            DrawTextCentered(g, "BACK", BtnStatusBack.X + (BtnStatusBack.Width / 2f), BtnStatusBack.Y + (BtnStatusBack.Height / 2f) - (8f * FontScale), 16f * FontScale, Color.White);
        }

        private static void DrawStatusBtn(Rage.Graphics g, RectangleF rect, string text, Color accent, PointF mouse)
        {
            bool hover = rect.Contains(mouse);
            g.DrawRectangle(rect, hover ? Color.FromArgb(255, 50, 60, 70) : Color.FromArgb(255, 35, 45, 55));
            g.DrawRectangle(new RectangleF(rect.X, rect.Y + rect.Height - 4f, rect.Width, 4f), accent);

            DrawTextCentered(g, text, rect.X + (rect.Width / 2f), rect.Y + (rect.Height / 2f) - (8f * FontScale), 14f * FontScale, Color.White);
        }

        private static void DrawTextCentered(Rage.Graphics g, string text, float x, float y, float size, Color color)
        {
            SizeF textSize = Rage.Graphics.MeasureText(text, "Arial", size);
            g.DrawText(text, "Arial", size, new PointF(x - (textSize.Width / 2f), y), color);
        }

        private static string Truncate(string text, int maxChars)
        {
            if (string.IsNullOrEmpty(text)) return "";
            return text.Length > maxChars ? text.Substring(0, maxChars) + "..." : text;
        }

        public static void Cleanup()
        {
            if (IsVisible) Toggle(false);
            if (_bgTexture != null)
            {
                _bgTexture = null;
            }
        }
    }
}