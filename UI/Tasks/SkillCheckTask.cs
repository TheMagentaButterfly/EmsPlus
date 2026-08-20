using Rage;
using Rage.Native;
using System;

namespace EmsPlus.UI.Tasks
{
    public class SkillCheckTask
    {
        private float _pos, _spd = 0.8f, _zS = 0.4f, _zW = 0.2f;
        private bool _dir = true;

        public bool? Process()
        {
            DisableControls();

            NativeFunction.Natives.ENABLE_CONTROL_ACTION(0, (int)GameControl.Attack, true);
            NativeFunction.Natives.ENABLE_CONTROL_ACTION(0, (int)GameControl.Jump, true);
            NativeFunction.Natives.ENABLE_CONTROL_ACTION(0, (int)GameControl.FrontendPause, true);

            NativeFunction.Natives.DRAW_RECT(0.5f, 0.9f, 0.3f, 0.03f, 30, 30, 30, 220);

            float zoneX = 0.5f - 0.15f + (_zS * 0.3f) + (_zW * 0.15f);
            NativeFunction.Natives.DRAW_RECT(zoneX, 0.9f, _zW * 0.3f, 0.03f, 0, 200, 100, 255);

            float moveStep = _spd * Game.FrameTime;
            _pos += _dir ? moveStep : -moveStep;

            if (_pos >= 1.0f) { _pos = 1.0f; _dir = false; }
            if (_pos <= 0.0f) { _pos = 0.0f; _dir = true; }

            float pointerX = 0.5f - 0.15f + (_pos * 0.3f);
            NativeFunction.Natives.DRAW_RECT(pointerX, 0.9f, 0.005f, 0.05f, 255, 255, 255, 255);

            if (Game.IsControlJustPressed(0, GameControl.Jump) || Game.IsControlJustPressed(0, GameControl.Attack))
            {
                bool win = _pos >= _zS && _pos <= (_zS + _zW);
                return win;
            }

            return null;
        }

        private static void DisableControls()
        {
            // Disable all inputs natively across all three control channels
            NativeFunction.Natives.DISABLE_ALL_CONTROL_ACTIONS(0);
            NativeFunction.Natives.DISABLE_ALL_CONTROL_ACTIONS(1);
            NativeFunction.Natives.DISABLE_ALL_CONTROL_ACTIONS(2);

            // Re-enable mouse camera controls
            NativeFunction.Natives.ENABLE_CONTROL_ACTION(0, 1, true); // LookLeftRight
            NativeFunction.Natives.ENABLE_CONTROL_ACTION(0, 2, true); // LookUpDown
            NativeFunction.Natives.ENABLE_CONTROL_ACTION(1, 1, true);
            NativeFunction.Natives.ENABLE_CONTROL_ACTION(1, 2, true);
            NativeFunction.Natives.ENABLE_CONTROL_ACTION(2, 1, true);
            NativeFunction.Natives.ENABLE_CONTROL_ACTION(2, 2, true);
        }
    }
}