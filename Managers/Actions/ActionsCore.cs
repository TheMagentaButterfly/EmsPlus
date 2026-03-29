using EmsPlus.Core;
using EmsPlus.UI.Native;
using Rage;
using Rage.Native;
using System;

namespace EmsPlus.Managers.Actions
{
    public static class ActionsCore
    {
        public static void Run(string subtitle, int durationMs, string animDict, string animName, Action onComplete)
        {
            if (GameState.IsPlayerBusy) return;
            GameState.IsPlayerBusy = true;

            MenuCore.CloseAll();

            GameFiber.StartNew(delegate
            {
                Ped player = Game.LocalPlayer.Character;
                bool inCabin = AmbulanceManager.IsPlayerInRearCabin;

                if (!string.IsNullOrEmpty(animDict))
                {
                    if (inCabin)
                    {
                        player.Tasks.PlayAnimation(animDict, animName, 8.0f, AnimationFlags.Loop | AnimationFlags.UpperBodyOnly | AnimationFlags.SecondaryTask);
                    }
                    else
                    {
                        player.Tasks.PlayAnimation(animDict, animName, 8.0f, AnimationFlags.Loop);
                    }
                }

                if (!string.IsNullOrEmpty(subtitle))
                {
                    Game.DisplaySubtitle(subtitle, durationMs);
                }

                GameFiber.Sleep(durationMs);

                if (inCabin)
                {
                    NativeFunction.Natives.CLEAR_PED_SECONDARY_TASK(player);
                }
                else
                {
                    player.Tasks.Clear();
                }

                onComplete?.Invoke();

                GameState.IsPlayerBusy = false;
            });
        }
    }
}