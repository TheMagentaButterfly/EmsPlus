using RAGENativeUI;

namespace EmsPlus.UI.Native.ConfigMenu
{
    public static partial class ConfigMenuBuilder
    {
        #region Constants & Fields

        // Menu styling colors
        internal const string C_HEADER = "~b~";
        internal const string C_SUCCESS = "~g~";
        internal const string C_WARNING = "~o~";
        internal const string C_DANGER = "~r~";
        internal const string C_INFO = "~c~";
        internal const string C_NORMAL = "~w~";
        internal const string C_HIGHLIGHT = "~y~";
        internal const string BULLET = "~b~•~s~";

        internal static UIMenu OffsetsRootMenu;
        internal static UIMenu StretcherPosMenu, PatientPosMenu, VehiclePosMenu;//, StretcherDockingMenu;
        internal static UIMenu PropPosMenu;

        internal static string _editingKitType = "TRAUMABAG";
        internal static int _editingPatientMode = 0; // 0 = Normal, 1 = Lowered, 2 = Sitting
        internal static int _editingVehicleMode = 0;
        internal static int _editingInteractionPointIndex = -1;

        #endregion

        public static bool IsEditingInteractionPoint(int index) => _editingInteractionPointIndex == index;

        #region Initialization

        public static void Build()
        {
            BuildOffsetsRootMenu();
            BuildSettingsMenu();
            BuildStretcherCarryMenu();
            BuildPatientMenu();
            BuildVehicleMenu();
            BuildPropPosMenu();
        }

        #endregion
    }
}