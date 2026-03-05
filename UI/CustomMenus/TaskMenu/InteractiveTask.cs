using Rage;
using Rage.Native;
using System;
using System.Collections.Generic;
using System.Drawing;
using EmsPlus.UI.Helpers;
using EmsPlus.Core;

namespace EmsPlus.CustomUI.TaskMenu
{
    public abstract class InteractiveTask
    {
        private const int GroupPlayer = 0, GroupFrontend = 2;
        private const int CursorX = 239, CursorY = 240, Attack = 24;

        private GameFiber _logicFiber;
        private UIElement _draggedElement;
        private PointF _dragOffset, _originalPos;
        private float _alpha;

        public bool IsActive { get; private set; }
        protected List<UIElement> Elements { get; } = new List<UIElement>();
        protected PointF MousePosition { get; private set; }

        public event EventHandler OnTaskCompleted, OnTaskAborted;
        private RectangleF _exitButtonRect;

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

            _exitButtonRect = new RectangleF(Game.Resolution.Width - 220, 45, 160, 36);

            OnStart();
            _logicFiber = GameFiber.StartNew(ProcessLogic);
            Game.FrameRender += OnFrameRender;
        }

        public void Stop()
        {
            if (!IsActive) return;
            IsActive = false;

            Game.FrameRender -= OnFrameRender;

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

                float cX = NativeFunction.Natives.GET_CONTROL_NORMAL<float>(GroupFrontend, CursorX);
                float cY = NativeFunction.Natives.GET_CONTROL_NORMAL<float>(GroupFrontend, CursorY);
                MousePosition = new PointF(cX * Game.Resolution.Width, cY * Game.Resolution.Height);

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

        private void OnFrameRender(object sender, GraphicsEventArgs e)
        {
            if (!IsActive) return;
            var g = e.Graphics;
            int ba = UI.Helpers.MathHelper.Clamp((int)(200 * _alpha), 0, 255);
            int ta = UI.Helpers.MathHelper.Clamp((int)(255 * _alpha), 0, 255);


            NativeUITools.DrawNativeRect(0, 0, Game.Resolution.Width, Game.Resolution.Height, Color.FromArgb(160, 0, 0, 0));
            NativeUITools.DrawNativeRect(0, 0, Game.Resolution.Width, 140, Color.FromArgb(ba, 10, 15, 25));

            for (int i = 0; i < 3; i++)
                NativeUITools.DrawNativeRect(0, 136 + i, Game.Resolution.Width, 1, Color.FromArgb(UI.Helpers.MathHelper.Clamp(80 - i * 25, 0, 255), 0, 180, 255));

            bool hoverExit = _exitButtonRect.Contains(MousePosition);
            Color btnCol = hoverExit ? Color.FromArgb(ba, 220, 60, 60) : Color.FromArgb(ba / 2, 160, 40, 40);
            NativeUITools.DrawNativeRect(_exitButtonRect.X, _exitButtonRect.Y, _exitButtonRect.Width, _exitButtonRect.Height, btnCol);
            if (hoverExit) NativeUITools.DrawNativeRect(_exitButtonRect.X, _exitButtonRect.Y + _exitButtonRect.Height - 2, _exitButtonRect.Width, 2, Color.White);

            Elements.ForEach(el => el?.DrawNativeShadow());

            var hTxt = Localization.Get("TASK_HELP_TEXT");
            float hWidth = NativeUITools.MeasureNativeTextWidth(hTxt, 0.35f);
            float hX = (Game.Resolution.Width / 2f);
            float hY = Game.Resolution.Height - 60f;
            NativeUITools.DrawNativeRect(hX - (hWidth / 2f) - 15, hY - 5, hWidth + 30, 40, Color.FromArgb(180, 15, 20, 30));


            Elements.ForEach(el => el?.DrawGdiTexture(g));


            Elements.ForEach(el => el?.DrawNativeOverlays());

            string exitText = Localization.Get("BTN_EXIT");
            NativeUITools.DrawNativeText(exitText, _exitButtonRect.X + (_exitButtonRect.Width / 2), _exitButtonRect.Y + 6, 0.35f, Color.FromArgb(ta, Color.White), true);
            NativeUITools.DrawNativeText(hTxt, hX, hY, 0.35f, Color.FromArgb(ta, 180, 200, 220), true);

            OnRender(g);


            bool isHoveringAny = hoverExit || Elements.Exists(el => el.IsHovered && el.Enabled && !el.IsDropZone);
            float sz = isHoveringAny ? 7f : 5f;

            g.DrawRectangle(new RectangleF(MousePosition.X - sz, MousePosition.Y - sz, sz * 2, sz * 2), Color.FromArgb(100, 0, 200, 255));
            g.DrawRectangle(new RectangleF(MousePosition.X - sz / 2, MousePosition.Y - sz / 2, sz, sz), Color.White);
        }
    }
}