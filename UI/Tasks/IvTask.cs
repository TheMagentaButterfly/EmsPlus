using Rage;
using System;
using System.Drawing;
using EmsPlus.UI.Helpers;
using EmsPlus.Custom.TaskMenu;

namespace EmsPlus.UI.Tasks
{
    public class IvTask : InteractiveTask
    {
        public event EventHandler<bool> OnIvPlaced;

        private enum State { Cath, Vein, Fix, Complete }
        private State _state;

        private UIElement _arm, _cath, _tape;
        private SkillCheckTask _skillCheck;

        protected override void OnStart()
        {
            float w = Game.Resolution.Width, h = Game.Resolution.Height, cX = w * 0.5f, cY = h * 0.5f;

            _arm = new UIElement("arm", "arm.png", new RectangleF(cX + 50, cY - 50, 512, 256));
            _cath = new UIElement("catheter", "catheter.png", new RectangleF(cX - 400, cY + 100, 128, 64));
            _tape = new UIElement("tape", "tape.png", new RectangleF(cX - 250, cY + 100, 128, 64));

            Elements.AddRange(new[] { _arm, _cath, _tape });

            _skillCheck = new SkillCheckTask();

            SetState(State.Cath);
        }

        protected override void OnUpdate()
        {
            if (_state == State.Vein)
            {
                bool? result = _skillCheck.Process();

                if (result.HasValue)
                {
                    if (result.Value == true)
                    {
                        AudioHelper.PlaySuccess();
                        _arm.ChangeTexture("catheter_in_arm.png");
                        SetState(State.Fix);
                    }
                    else
                    {
                        AudioHelper.PlayError();
                    }
                }
            }
        }

        protected override void OnRender(Rage.Graphics g)
        {
            float cX = Game.Resolution.Width / 2f;
            float hY = Game.Resolution.Height * 0.15f;

            string title = Localization.Get("TASK_IV_TITLE", "IV PLACEMENT");
            string instr = GetInstruction();

            NativeUITools.DrawNativeText(title, cX, hY, 0.5f, Color.FromArgb(255, 0, 200, 255), true);
            NativeUITools.DrawNativeText(instr, cX, hY + 45, 0.35f, Color.FromArgb(200, 200, 220, 240), true);
        }

        protected override void OnElementDropped(UIElement d, UIElement t)
        {
            if (_state == State.Cath && d == _cath && t == _arm)
            {
                Elements.Remove(_cath);
                AudioHelper.PlaySuccess();
                SetState(State.Vein);
            }
            else if (_state == State.Fix && d == _tape && t == _arm)
            {
                Elements.Remove(_tape);
                AudioHelper.PlaySuccess();
                SetState(State.Complete);
            }
        }

        private void SetState(State s)
        {
            _state = s;
            Elements.ForEach(e => e.IsDropZone = false);

            switch (s)
            {
                case State.Cath:
                case State.Fix:
                    _arm.IsDropZone = true;
                    break;
                case State.Vein:
                    break;
            }

            if (s == State.Complete)
            {
                OnIvPlaced?.Invoke(this, true);
                GameFiber.Sleep(1500);
                CompleteTask();
            }
        }

        private string GetInstruction()
        {
            switch (_state)
            {
                case State.Cath: return Localization.Get("TASK_IV_STEP_CATH", "Insert the catheter.");
                case State.Vein: return Localization.Get("TASK_IV_STEP_VEIN", "Locate the vein.");
                case State.Fix: return Localization.Get("TASK_IV_STEP_FIX", "Secure the IV with tape.");
                default: return Localization.Get("TASK_IV_STEP_COMPLETE", "IV placement complete.");
            }
        }
    }
}