using Rage;
using Rage.Native;
using System;
using System.Collections.Generic;
using System.Drawing;
using EmsPlus.UI.Helpers;
using EmsPlus.Core;

namespace EmsPlus.Custom.TaskMenu
{
    public abstract class InteractiveTask
    {
        private const int GroupPlayer = 0, GroupFrontend = 2;
        private const int CursorX = 239, CursorY = 240, Attack = 24;

        private GameFiber _logicFiber;
        private UIElement _draggedElement;
        private PointF _dragOffset, _originalPos;
        private float _alpha;

        private float _mouseX = 0f;
        private float _mouseY = 0f;

        public bool IsActive { get; private set; }
        protected List<UIElement> Elements { get; } = new List<UIElement>();
        protected PointF MousePosition => new PointF(_mouseX, _mouseY);

        public event EventHandler OnTaskCompleted, OnTaskAborted;
        private RectangleF _exitButtonRect;

        protected float FontScale => Game.Resolution.Height / 1080f;

        protected abstract void OnStart();
        protected abstract void OnUpdate();
        protected abstract void OnRender(Rage.Graphics g);
        protected abstract void OnElementDropped(UIElement dragged, UIElement target);

        public void Start()
        {
            if (IsActive) return;
            IsActive = true;
            _alpha = 0f;
            Elements.Clear();

            _exitButtonRect = new RectangleF(Game.Resolution.Width - 220f, 45f, 160f, 36f);

            OnStart();
            _logicFiber = GameFiber.StartNew(ProcessLogic);

            Game.RawFrameRender += OnRawFrameRender;
        }

        public void Stop()
        {
            if (!IsActive) return;
            IsActive = false;

            Game.RawFrameRender -= OnRawFrameRender;

            GameFiber.StartNew(delegate
            {
                uint timeout = Game.GameTime + 250;
                while (Game.GameTime < timeout)
                {
                    GameFiber.Yield();
                    if (NativeFunction.Natives.IS_PAUSE_MENU_ACTIVE<bool>())
                        NativeFunction.Natives.SET_PAUSE_MENU_ACTIVE(false);
                    NativeFunction.Natives.DISABLE_CONTROL_ACTION(0, 199, true);
                    NativeFunction.Natives.DISABLE_CONTROL_ACTION(0, 200, true);
                }
            });

            Elements.ForEach(el => el?.Dispose());
            Elements.Clear();

            OnTaskAborted?.Invoke(this, EventArgs.Empty);
            GameState.IsPlayerBusy = false;
        }

        protected void CompleteTask()
        {
            OnTaskCompleted?.Invoke(this, EventArgs.Empty);
            Stop();
        }

        private void ProcessLogic()
        {
            while (IsActive)
            {
                GameFiber.Yield();

                NativeFunction.Natives.DISABLE_CONTROL_ACTION(0, 199, true);
                NativeFunction.Natives.DISABLE_CONTROL_ACTION(0, 200, true);
                if (NativeFunction.Natives.IS_PAUSE_MENU_ACTIVE<bool>())
                    NativeFunction.Natives.SET_PAUSE_MENU_ACTIVE(false);

                NativeFunction.Natives.DISABLE_ALL_CONTROL_ACTIONS(GroupPlayer);
                int[] enabled = { CursorX, CursorY, Attack };
                foreach (int ctrl in enabled) NativeFunction.Natives.ENABLE_CONTROL_ACTION(GroupPlayer, ctrl, true);

                // Fetch mouse position safely
                _mouseX = NativeFunction.Natives.GET_CONTROL_NORMAL<float>(GroupFrontend, CursorX) * Game.Resolution.Width;
                _mouseY = NativeFunction.Natives.GET_CONTROL_NORMAL<float>(GroupFrontend, CursorY) * Game.Resolution.Height;

                _alpha = Math.Min(_alpha + 0.05f, 1f);

                if (_draggedElement == null) Elements.ForEach(el => el?.Update(MousePosition));

                OnUpdate();
                HandleDragDrop();

                if (NativeFunction.Natives.IS_DISABLED_CONTROL_JUST_PRESSED<bool>(0, 200)) { Stop(); break; }

                if (NativeFunction.Natives.IS_DISABLED_CONTROL_JUST_PRESSED<bool>(0, Attack))
                {
                    if (_exitButtonRect.Contains(MousePosition)) { AudioHelper.PlayBack(); Stop(); break; }
                }
            }
        }

        private void HandleDragDrop()
        {
            bool pressed = NativeFunction.Natives.IS_DISABLED_CONTROL_JUST_PRESSED<bool>(GroupPlayer, Attack);
            bool released = NativeFunction.Natives.IS_DISABLED_CONTROL_JUST_RELEASED<bool>(GroupPlayer, Attack);

            if (pressed && !_exitButtonRect.Contains(MousePosition))
            {
                for (int i = Elements.Count - 1; i >= 0; i--)
                {
                    var el = Elements[i];
                    if (el != null && el.IsHovered && !el.IsDropZone && el.Enabled)
                    {
                        _draggedElement = el;
                        _draggedElement.IsBeingDragged = true;
                        _originalPos = el.Bounds.Location;
                        _dragOffset = new PointF(MousePosition.X - el.Bounds.X, MousePosition.Y - el.Bounds.Y);
                        break;
                    }
                }
            }

            if (_draggedElement != null)
            {
                if (released)
                {
                    var target = Elements.Find(t => t != null && t.IsDropZone && t.Bounds.Contains(MousePosition));
                    if (target != null) OnElementDropped(_draggedElement, target);
                    else
                    {
                        _draggedElement.Bounds = new RectangleF(_originalPos, _draggedElement.Bounds.Size);
                        AudioHelper.PlayError();
                    }
                    _draggedElement.IsBeingDragged = false;
                    _draggedElement = null;
                }
                else
                {
                    _draggedElement.Bounds = new RectangleF(MousePosition.X - _dragOffset.X, MousePosition.Y - _dragOffset.Y,
                        _draggedElement.Bounds.Width, _draggedElement.Bounds.Height);
                }
            }
        }

        private void OnRawFrameRender(object sender, GraphicsEventArgs e)
        {
            if (!IsActive) return;
            var g = e.Graphics;
            int ba = UI.Helpers.MathHelper.Clamp((int)(200 * _alpha), 0, 255);
            int ta = UI.Helpers.MathHelper.Clamp((int)(255 * _alpha), 0, 255);

            // 1. Backgrounds
            g.DrawRectangle(new RectangleF(0, 0, Game.Resolution.Width, Game.Resolution.Height), Color.FromArgb(160, 0, 0, 0));
            g.DrawRectangle(new RectangleF(0, 0, Game.Resolution.Width, 140), Color.FromArgb(ba, 10, 15, 25));

            for (int i = 0; i < 3; i++)
            {
                g.DrawRectangle(new RectangleF(0, 136 + i, Game.Resolution.Width, 1), Color.FromArgb(UI.Helpers.MathHelper.Clamp(80 - i * 25, 0, 255), 0, 180, 255));
            }

            Elements.ForEach(el => el?.Draw(g));

            bool hoverExit = _exitButtonRect.Contains(MousePosition);
            Color btnCol = hoverExit ? Color.FromArgb(ba, 220, 60, 60) : Color.FromArgb(ba / 2, 160, 40, 40);
            g.DrawRectangle(_exitButtonRect, btnCol);
            if (hoverExit) g.DrawRectangle(new RectangleF(_exitButtonRect.X, _exitButtonRect.Y + _exitButtonRect.Height - 2, _exitButtonRect.Width, 2), Color.White);

            string exitText = Localization.Get("BTN_EXIT", "EXIT");
            DrawTextCentered(g, exitText, _exitButtonRect.X + (_exitButtonRect.Width / 2f), _exitButtonRect.Y + 8f, 16f * FontScale, Color.FromArgb(ta, Color.White));

            var hTxt = Localization.Get("TASK_HELP_TEXT", "Drag items to complete the procedure");
            float hX = Game.Resolution.Width / 2f;
            float hY = Game.Resolution.Height - 60f;

            SizeF hSize = Rage.Graphics.MeasureText(hTxt, "Arial", 16f * FontScale);
            g.DrawRectangle(new RectangleF(hX - (hSize.Width / 2f) - 15f, hY - 5f, hSize.Width + 30f, 30f * FontScale), Color.FromArgb(180, 15, 20, 30));
            DrawTextCentered(g, hTxt, hX, hY, 16f * FontScale, Color.FromArgb(ta, 180, 200, 220));

            OnRender(g);

            bool isHoveringAny = hoverExit || Elements.Exists(el => el.IsHovered && el.Enabled && !el.IsDropZone);
            float sz = isHoveringAny ? 7f : 5f;

            g.DrawRectangle(new RectangleF(MousePosition.X - (sz / 2f), MousePosition.Y - (sz / 2f), sz, sz), Color.White);
            g.DrawRectangle(new RectangleF(MousePosition.X - sz, MousePosition.Y - sz, sz * 2, sz * 2), Color.FromArgb(100, 0, 200, 255));
        }

        protected void DrawTextCentered(Rage.Graphics g, string text, float x, float y, float size, Color color)
        {
            SizeF textSize = Rage.Graphics.MeasureText(text, "Arial", size);
            g.DrawText(text, "Arial", size, new PointF(x - (textSize.Width / 2f), y), color);
        }
    }
}