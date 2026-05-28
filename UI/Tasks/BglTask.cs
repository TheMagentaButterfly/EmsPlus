using Rage;
using System;
using System.Drawing;
using EmsPlus.UI.Helpers;
using EmsPlus.Core;
using EmsPlus.Medical;
using EmsPlus.Custom.TaskMenu;

namespace EmsPlus.UI.Tasks
{
    public class BglTask : InteractiveTask
    {
        public event EventHandler<VitalState> OnBglMeasured;

        private enum State { NeedsPrick, NeedsSample, NeedsReading, Complete }
        private State _state;
        private UIElement _hand, _lancet, _strip, _glucometer;

        protected override void OnStart()
        {
            float w = Game.Resolution.Width, h = Game.Resolution.Height, cX = w * 0.5f, cY = h * 0.5f;
            _hand = new UIElement("hand", "task_bgl_finger.png", new RectangleF(cX + 100, cY - 50, 256, 256));
            _lancet = new UIElement("lancet", "task_bgl_lancet.png", new RectangleF(cX - 300, cY - 150, 64, 128));
            _strip = new UIElement("strip", "task_bgl_strip.png", new RectangleF(cX - 300, cY, 32, 128));
            _glucometer = new UIElement("glucometer", "task_bgl_meter.png", new RectangleF(cX - 300, cY + 150, 128, 192));
            Elements.AddRange(new[] { _hand, _lancet, _strip, _glucometer });
            SetState(State.NeedsPrick);
        }

        protected override void OnUpdate() { }

        protected override void OnRender(Rage.Graphics g)
        {
            float cX = Game.Resolution.Width / 2f;
            float hY = Game.Resolution.Height * 0.15f;
            string title = Localization.Get("TASK_BGL_TITLE", "BLOOD GLUCOSE TEST");
            string instr = GetInstruction();
            NativeUITools.DrawNativeText(title, cX, hY, 0.5f, Color.FromArgb(255, 0, 200, 255), true);
            NativeUITools.DrawNativeText(instr, cX, hY + 45, 0.35f, Color.FromArgb(200, 200, 220, 240), true);
        }

        protected override void OnElementDropped(UIElement d, UIElement t)
        {
            if (_state == State.NeedsPrick && d == _lancet && t == _hand)
            {
                _hand.ChangeTexture("task_bgl_finger_blood.png");
                Elements.Remove(_lancet); _lancet.Dispose();
                AudioHelper.PlaySuccess();
                SetState(State.NeedsSample);
            }
            else if (_state == State.NeedsSample && d == _strip && t == _hand)
            {
                _strip.ChangeTexture("task_bgl_strip_blood.png");
                AudioHelper.PlaySuccess();
                SetState(State.NeedsReading);
            }
            else if (_state == State.NeedsReading && d == _strip && t == _glucometer)
            {
                Elements.Remove(_strip); _strip.Dispose();
                AudioHelper.PlaySuccess();
                SetState(State.Complete);
            }
        }

        private void SetState(State s)
        {
            _state = s; _hand.IsDropZone = s != State.NeedsReading && s != State.Complete; _glucometer.IsDropZone = s == State.NeedsReading;

            if (s == State.Complete)
            {
                VitalState result = VitalState.Normal;

                if (GameState.CurrentPatient != null)
                {
                    result = GameState.CurrentPatient.BloodGlucose;
                }

                OnBglMeasured?.Invoke(this, result);

                GameFiber.Sleep(1500);
                CompleteTask();
            }
        }

        private string GetInstruction()
        {
            switch (_state)
            {
                case State.NeedsPrick: return Localization.Get("TASK_BGL_STEP_PRICK", "Prick the finger.");
                case State.NeedsSample: return Localization.Get("TASK_BGL_STEP_SAMPLE", "Collect the blood sample.");
                case State.NeedsReading: return Localization.Get("TASK_BGL_STEP_INSERT", "Insert the strip into the glucometer.");
                default: return Localization.Get("TASK_BGL_STEP_COMPLETE", "Test complete!");
            }
        }
    }
}