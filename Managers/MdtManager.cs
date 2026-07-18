using EmsPlus.Core;
using EmsPlus.UI.Helpers;
using Rage;
using Rage.Native;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace EmsPlus.Managers
{
    public enum MdtPage { Main, StatusOverlay }

    public static class MdtManager
    {
        public static bool IsVisible { get; private set; } = false;
        public static MdtPage CurrentPage { get; private set; } = MdtPage.Main;
        private static Texture _bgTexture;

        private static float _mouseX = 0f;
        private static float _mouseY = 0f;

        private static float MdtX, MdtY, MdtW, MdtH, FontScale;
        private static RectangleF BtnNavHome, BtnNavStatus, BtnStatusBack;

        private struct MdtStatusButton
        {
            public EmsStatus Status;
            public string Label;
            public Color Color;
            public RectangleF Bounds;
        }

        private static readonly List<MdtStatusButton> _statusButtons = new List<MdtStatusButton>();

        public static void ShowCalloutPage()
        {
            if (!IsVisible) Toggle(true);
            CurrentPage = MdtPage.Main;
        }

        private static void UpdateLayout()
        {
            float w = Game.Resolution.Width;
            float h = Game.Resolution.Height;

            var c = EntryPoint.OffsetConfig;
            float scale = c != null ? c.MdtScale : 1.0f;
            float offX = c != null ? c.MdtOffsetX : 0.0f;
            float offY = c != null ? c.MdtOffsetY : 0.0f;

            MdtW = w * 0.45f * scale;
            MdtH = MdtW * (1080f / 1920f);

            MdtX = w - MdtW - 25f + (w * offX);
            MdtY = h - MdtH - 25f + (h * offY);

            FontScale = (h / 1080f) * scale;

            BtnNavHome = new RectangleF(MdtX + (MdtW * 0.084f), MdtY + (MdtH * 0.849f), MdtW * 0.12f, MdtH * 0.06f);
            BtnNavStatus = new RectangleF(MdtX + (MdtW * 0.796f), MdtY + (MdtH * 0.849f), MdtW * 0.12f, MdtH * 0.06f);
            BtnStatusBack = new RectangleF(MdtX + (MdtW * 0.4f), MdtY + (MdtH * 0.74f), MdtW * 0.2f, MdtH * 0.06f);

            _statusButtons.Clear();

            // All 11 registered statuses mapped cleanly with specific UI styling properties
            var statuses = new[]
            {
                new { Status = EmsStatus.Available, Label = "AVAILABLE", Color = Color.Green },
                new { Status = EmsStatus.AvailableAtStation, Label = "AT STATION", Color = Color.Green },
                new { Status = EmsStatus.EnRoute, Label = "EN ROUTE", Color = Color.Orange },
                new { Status = EmsStatus.OnScene, Label = "ON SCENE", Color = Color.Orange },
                new { Status = EmsStatus.Transporting, Label = "TRANSPORTING", Color = Color.Orange },
                new { Status = EmsStatus.AtDestination, Label = "AT DEST", Color = Color.Yellow },
                new { Status = EmsStatus.Busy, Label = "BUSY", Color = Color.Red },
                new { Status = EmsStatus.OffDuty, Label = "OFF DUTY", Color = Color.Gray },
                new { Status = EmsStatus.RequestToSpeak, Label = "REQ SPEAK", Color = Color.Orange },
                new { Status = EmsStatus.UrgentRequestToSpeak, Label = "URG SPEAK", Color = Color.Purple },
                new { Status = EmsStatus.Emergency, Label = "EMERGENCY", Color = Color.Purple }
            };

            float colWidth = MdtW * 0.20f;
            float rowHeight = MdtH * 0.09f;

            float startX = MdtX + (MdtW * 0.18f);
            float startY = MdtY + (MdtH * 0.25f);

            for (int i = 0; i < statuses.Length; i++)
            {
                int row = i / 3;
                int col = i % 3;

                // Center the last row which contains only 2 elements
                float xOffset = (row == 3) ? ((colWidth + (MdtW * 0.025f)) / 2f) : 0f;

                _statusButtons.Add(new MdtStatusButton
                {
                    Status = statuses[i].Status,
                    Label = statuses[i].Label,
                    Color = statuses[i].Color,
                    Bounds = new RectangleF(
                        startX + (col * (colWidth + (MdtW * 0.025f))) + xOffset,
                        startY + (row * (rowHeight + (MdtH * 0.02f))),
                        colWidth,
                        rowHeight
                    )
                });
            }
        }

        public static void ForceUpdateLayout() => UpdateLayout();

        public static bool IsMouseUnlocked = false;

        public static void Toggle(bool state)
        {
            if (IsVisible == state) return;

            IsVisible = state;
            if (state)
            {
                CurrentPage = MdtPage.Main;
                UpdateLayout();

                string texPath = Assets.GetPath("MdtBackground.png");
                if (File.Exists(texPath) && _bgTexture == null)
                {
                    _bgTexture = Game.CreateTextureFromFile(texPath);
                }

                Game.RawFrameRender += OnRawRender;
            }
            else
            {
                IsMouseUnlocked = false;
                NativeFunction.Natives.SET_MOUSE_CURSOR_VISIBLE(false);
                Game.RawFrameRender -= OnRawRender;
            }
        }

        public static void SetMouseUnlocked(bool state)
        {
            IsMouseUnlocked = state;
        }

        public static void Process()
        {
            if (!IsVisible) return;

            if (IsMouseUnlocked)
            {
                NativeFunction.Natives.DISABLE_CONTROL_ACTION(0, 1, true);   // LookLeftRight
                NativeFunction.Natives.DISABLE_CONTROL_ACTION(0, 2, true);   // LookUpDown
                NativeFunction.Natives.DISABLE_CONTROL_ACTION(0, 3, true);   // Look fly LR
                NativeFunction.Natives.DISABLE_CONTROL_ACTION(0, 4, true);   // Look fly UD
                NativeFunction.Natives.DISABLE_CONTROL_ACTION(0, 5, true);   // Look UI LR
                NativeFunction.Natives.DISABLE_CONTROL_ACTION(0, 6, true);   // Look UI UD

                NativeFunction.Natives.DISABLE_CONTROL_ACTION(0, 24, true);  // Attack (Left Click)
                NativeFunction.Natives.DISABLE_CONTROL_ACTION(0, 25, true);  // Aim (Right Click)
                NativeFunction.Natives.DISABLE_CONTROL_ACTION(0, 140, true); // Melee Attack Light
                NativeFunction.Natives.DISABLE_CONTROL_ACTION(0, 141, true); // Melee Attack Heavy
                NativeFunction.Natives.DISABLE_CONTROL_ACTION(0, 142, true); // Melee Attack Alternate
                NativeFunction.Natives.DISABLE_CONTROL_ACTION(0, 257, true); // Attack 2

                NativeFunction.Natives.DISABLE_CONTROL_ACTION(0, 68, true);  // Vehicle Aim
                NativeFunction.Natives.DISABLE_CONTROL_ACTION(0, 69, true);  // Vehicle Attack
                NativeFunction.Natives.DISABLE_CONTROL_ACTION(0, 70, true);  // Vehicle Attack 2
                NativeFunction.Natives.DISABLE_CONTROL_ACTION(0, 91, true);  // Vehicle Passenger Aim
                NativeFunction.Natives.DISABLE_CONTROL_ACTION(0, 92, true);  // Vehicle Passenger Attack

                NativeFunction.Natives.DISABLE_CONTROL_ACTION(0, 199, true); // Pause Menu (P)
                NativeFunction.Natives.DISABLE_CONTROL_ACTION(0, 200, true); // Pause Menu (Esc)
                NativeFunction.Natives.DISABLE_CONTROL_ACTION(0, 85, true);  // Radio Wheel

                _mouseX = NativeFunction.Natives.GET_CONTROL_NORMAL<float>(0, 239) * Game.Resolution.Width;
                _mouseY = NativeFunction.Natives.GET_CONTROL_NORMAL<float>(0, 240) * Game.Resolution.Height;

                if (NativeFunction.Natives.IS_DISABLED_CONTROL_JUST_PRESSED<bool>(0, 24) ||
                    NativeFunction.Natives.IS_DISABLED_CONTROL_JUST_PRESSED<bool>(0, 237))
                {
                    HandleClick();
                }
            }

            NativeFunction.Natives.SET_MOUSE_CURSOR_VISIBLE(IsMouseUnlocked);
        }

        private static void HandleClick()
        {
            PointF mouse = new PointF(_mouseX, _mouseY);

            if (CurrentPage == MdtPage.Main)
            {
                if (BtnNavHome.Contains(mouse)) { Toggle(false); AudioHelper.PlayBack(); }
                else if (BtnNavStatus.Contains(mouse)) { CurrentPage = MdtPage.StatusOverlay; AudioHelper.PlaySelect(); }
            }
            else if (CurrentPage == MdtPage.StatusOverlay)
            {
                if (BtnStatusBack.Contains(mouse)) { CurrentPage = MdtPage.Main; AudioHelper.PlayBack(); }
                else
                {
                    foreach (var btn in _statusButtons)
                    {
                        if (btn.Bounds.Contains(mouse))
                        {
                            EmsService.SetStatus(btn.Status);
                            Toggle(false);
                            AudioHelper.PlaySuccess();
                            break;
                        }
                    }
                }
            }
        }

        private static void OnRawRender(object sender, GraphicsEventArgs e)
        {
            if (!IsVisible) return;
            var g = e.Graphics;
            PointF mouse = new PointF(_mouseX, _mouseY);

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
                if (BtnNavHome.Contains(mouse)) g.DrawRectangle(BtnNavHome, Color.FromArgb(60, 255, 255, 255));
                if (BtnNavStatus.Contains(mouse)) g.DrawRectangle(BtnNavStatus, Color.FromArgb(60, 255, 255, 255));
            }

            if (CurrentPage == MdtPage.Main) DrawCalloutDataText(g);
            else if (CurrentPage == MdtPage.StatusOverlay) DrawStatusPanel(g, mouse);

            if (IsMouseUnlocked)
            {
                float sz = 6f;
                g.DrawRectangle(new RectangleF(mouse.X - (sz / 2f), mouse.Y - (sz / 2f), sz, sz), Color.White);
                g.DrawRectangle(new RectangleF(mouse.X - sz, mouse.Y - sz, sz * 2, sz * 2), Color.FromArgb(150, 0, 180, 255));
            }
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

            string dest = "TBD";
            if (EmsService.CurrentStatus == EmsStatus.Transporting)
            {
                var nearestHosp = EntryPoint.HospitalsConfig.Locations.OrderBy(l => Game.LocalPlayer.Character.Position.DistanceTo(l.Position)).FirstOrDefault();
                if (nearestHosp != null) dest = nearestHosp.Name;
            }

            float col1X = MdtX + (MdtW * 0.09f);
            g.DrawText($"{time}{incType}", "Arial", scale, new PointF(col1X, MdtY + (MdtH * 0.25f)), textColor);
            g.DrawText(loc, "Arial", scale, new PointF(col1X, MdtY + (MdtH * 0.385f)), textColor);
            g.DrawText(patFullName, "Arial", scale, new PointF(col1X, MdtY + (MdtH * 0.515f)), textColor);
            g.DrawText("LSPD Dispatch", "Arial", scale, new PointF(col1X, MdtY + (MdtH * 0.645f)), textColor);
            g.DrawText(dest, "Arial", scale, new PointF(col1X, MdtY + (MdtH * 0.775f)), textColor);

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

            foreach (var btn in _statusButtons)
            {
                DrawStatusBtn(g, btn.Bounds, btn.Label, btn.Color, mouse);
            }

            g.DrawRectangle(BtnStatusBack, Color.FromArgb(255, 50, 60, 70));
            DrawTextCentered(g, "BACK", BtnStatusBack.X + (BtnStatusBack.Width / 2f), BtnStatusBack.Y + (BtnStatusBack.Height / 2f) - (8f * FontScale), 16f * FontScale, Color.White);
        }

        private static void DrawStatusBtn(Rage.Graphics g, RectangleF rect, string text, Color accent, PointF mouse)
        {
            bool hover = rect.Contains(mouse);
            g.DrawRectangle(rect, hover ? Color.FromArgb(255, 50, 60, 70) : Color.FromArgb(255, 35, 45, 55));
            g.DrawRectangle(new RectangleF(rect.X, rect.Y + rect.Height - 4f, rect.Width, 4f), accent);

            // Scale down font slightly to fit longer button labels without wrapping
            DrawTextCentered(g, text, rect.X + (rect.Width / 2f), rect.Y + (rect.Height / 2f) - (6f * FontScale), 10f * FontScale, Color.White);
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