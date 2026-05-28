using EmsPlus.Configuration;
using EmsPlus.Core;
using Rage;
using Rage.Native;

namespace EmsPlus.Managers
{
    public static class InteriorManager
    {
        public static InteriorDefinition CurrentInterior { get; private set; }
        public static EntranceDefinition LastUsedEntrance { get; private set; }

        public static InteriorDefinition TargetInterior { get; private set; }
        public static EntranceDefinition TargetEntrance { get; private set; }

        private static bool _isTeleporting = false;

        public static void EnableTargetEntrance(InteriorDefinition interior, EntranceDefinition entrance)
        {
            TargetInterior = interior;
            TargetEntrance = entrance;
        }

        public static void DisableTargetEntrance()
        {
            TargetInterior = null;
            TargetEntrance = null;
        }

        public static void ForceClearInterior()
        {
            if (CurrentInterior != null)
            {
                foreach (var ipl in CurrentInterior.RequiredIPLs) NativeFunction.Natives.REMOVE_IPL(ipl);
            }
            CurrentInterior = null;
            LastUsedEntrance = null;
            DisableTargetEntrance();
        }

        public static void Process()
        {
            if (_isTeleporting) return;

            Ped player = Game.LocalPlayer.Character;
            Vector3 pPos = player.Position;

            if (CurrentInterior == null)
            {
                // Player is outside: Only check the TARGET entrance assigned by the active callout
                if (TargetEntrance != null && TargetInterior != null)
                {
                    float dist = pPos.DistanceTo(TargetEntrance.Coords);
                    if (dist < 20f)
                    {
                        NativeFunction.Natives.DRAW_MARKER(1, TargetEntrance.Coords.X, TargetEntrance.Coords.Y, TargetEntrance.Coords.Z - 1.0f, 0, 0, 0, 0, 0, 0, 1.5f, 1.5f, 1.0f, 0, 200, 255, 150, false, false, 2, false, 0, 0, false);
                        if (dist < 1.5f)
                        {
                            Game.DisplayHelp(Localization.GetFormat("HELP_ENTER_INTERIOR", "Press ~INPUT_CONTEXT~ to enter {0}.", TargetEntrance.Name));
                            if (Game.IsControlJustPressed(0, GameControl.Context))
                            {
                                EnterInterior(TargetInterior, TargetEntrance);
                            }
                        }
                    }
                }
            }
            else
            {
                // Player is inside: Check the interior's exit door
                float dist = pPos.DistanceTo(CurrentInterior.ExitCoords);
                if (dist < 20f)
                {
                    NativeFunction.Natives.DRAW_MARKER(1, CurrentInterior.ExitCoords.X, CurrentInterior.ExitCoords.Y, CurrentInterior.ExitCoords.Z - 1.0f, 0, 0, 0, 0, 0, 0, 1.5f, 1.5f, 1.0f, 0, 200, 255, 150, false, false, 2, false, 0, 0, false);
                    if (dist < 1.5f)
                    {
                        Game.DisplayHelp(Localization.GetFormat("HELP_EXIT_INTERIOR", "Press ~INPUT_CONTEXT~ to exit."));
                        if (Game.IsControlJustPressed(0, GameControl.Context))
                        {
                            ExitInterior();
                        }
                    }
                }
            }
        }

        private static void EnterInterior(InteriorDefinition interior, EntranceDefinition entrance)
        {
            _isTeleporting = true;
            GameState.IsPlayerBusy = true;
            CurrentInterior = interior;
            LastUsedEntrance = entrance;

            foreach (var ipl in interior.RequiredIPLs) NativeFunction.Natives.REQUEST_IPL(ipl);

            bool wasHolding = StretcherManager.IsAttachedToPlayer;
            if (wasHolding) StretcherManager.DetachFromPlayer();

            GameFiber.StartNew(delegate {
                Game.FadeScreenOut(500);
                GameFiber.Sleep(600);

                Game.LocalPlayer.Character.Position = interior.ExitCoords;

                if (wasHolding && StretcherManager.Prop != null && StretcherManager.Prop.Exists())
                {
                    StretcherManager.Prop.Position = interior.ExitCoords + (Game.LocalPlayer.Character.ForwardVector * 1.0f);
                    StretcherManager.AttachToPlayer();
                }

                GameFiber.Sleep(500);
                Game.FadeScreenIn(500);
                GameState.IsPlayerBusy = false;
                _isTeleporting = false;
            });
        }

        private static void ExitInterior()
        {
            _isTeleporting = true;
            GameState.IsPlayerBusy = true;

            var exitTo = LastUsedEntrance;
            bool wasHolding = StretcherManager.IsAttachedToPlayer;
            if (wasHolding) StretcherManager.DetachFromPlayer();

            GameFiber.StartNew(delegate {
                Game.FadeScreenOut(500);
                GameFiber.Sleep(600);

                if (exitTo != null)
                {
                    Game.LocalPlayer.Character.Position = exitTo.Coords;

                    if (wasHolding && StretcherManager.Prop != null && StretcherManager.Prop.Exists())
                    {
                        StretcherManager.Prop.Position = exitTo.Coords + (Game.LocalPlayer.Character.ForwardVector * 1.0f);
                        StretcherManager.AttachToPlayer();
                    }
                }

                foreach (var ipl in CurrentInterior.RequiredIPLs) NativeFunction.Natives.REMOVE_IPL(ipl);

                CurrentInterior = null;
                LastUsedEntrance = null;

                GameFiber.Sleep(500);
                Game.FadeScreenIn(500);
                GameState.IsPlayerBusy = false;
                _isTeleporting = false;
            });
        }
    }
}