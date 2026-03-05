using Rage;
using Rage.Native;

namespace EmsPlus.Managers
{
    public static class SceneManager
    {
        public static bool IsSceneSecured { get; private set; } = false;
        private static int _speedZoneId = -1;
        private static int _popZoneId = -1;

        public static void ToggleSceneSecurity()
        {
            if (IsSceneSecured)
            {
                ClearScene();
                Game.DisplayNotification("~g~Scene Unsecured:~w~ Traffic and pedestrians returning to normal.");
            }
            else
            {
                Vector3 p = Game.LocalPlayer.Character.Position;

                _speedZoneId = NativeFunction.Natives.ADD_ROAD_NODE_SPEED_ZONE<int>(p.X, p.Y, p.Z, 40.0f, 0.0f, false);

                _popZoneId = NativeFunction.Natives.ADD_POPULATION_BLOCKING_AREA<int>(p.X - 40f, p.Y - 40f, p.X + 40f, p.Y + 40f, false, false, false, false);

                IsSceneSecured = true;
                Game.DisplayNotification("~r~Scene Secured:~w~ Traffic stopped and pedestrians blocked.");
            }
        }

        public static void ClearScene()
        {
            if (!IsSceneSecured) return;

            if (_speedZoneId != -1)
            {
                NativeFunction.Natives.REMOVE_ROAD_NODE_SPEED_ZONE(_speedZoneId);
                _speedZoneId = -1;
            }
            if (_popZoneId != -1)
            {
                NativeFunction.Natives.REMOVE_POPULATION_BLOCKING_AREA(_popZoneId);
                _popZoneId = -1;
            }

            IsSceneSecured = false;
        }
    }
}