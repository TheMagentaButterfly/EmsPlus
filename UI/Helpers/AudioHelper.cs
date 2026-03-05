using Rage.Native;

namespace EmsPlus.UI.Helpers
{
    public static class AudioHelper
    {
        public static void PlaySelect() => Play("SELECT", "HUD_FRONTEND_DEFAULT_SOUNDSET");
        public static void PlayError() => Play("ERROR", "HUD_FRONTEND_DEFAULT_SOUNDSET");
        public static void PlayBack() => Play("BACK", "HUD_FRONTEND_DEFAULT_SOUNDSET");
        public static void PlaySuccess() => Play("CONFIRM_BEEP", "HUD_MINI_GAME_SOUNDSET");

        private static void Play(string name, string soundSet) =>
            NativeFunction.Natives.PLAY_SOUND_FRONTEND(-1, name, soundSet, true);
    }
}