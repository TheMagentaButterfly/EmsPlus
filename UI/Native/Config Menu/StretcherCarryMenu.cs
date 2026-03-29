using EmsPlus.Managers;

namespace EmsPlus.UI.Native.ConfigMenu
{
    public static partial class ConfigMenuBuilder
    {
        #region Stretcher Carry Menu

        private static void BuildStretcherCarryMenu()
        {
            MenuHelpers.AddListControl(StretcherPosMenu, $"{C_HEADER}X {C_INFO}{Localization.Get("LABEL_LEFT_RIGHT")}", EntryPoint.OffsetConfig.StretcherAttachOffsetX, v => EntryPoint.OffsetConfig.StretcherAttachOffsetX = v, StretcherGhostManager.UpdateCarryGhost);
            MenuHelpers.AddListControl(StretcherPosMenu, $"{C_HEADER}Y {C_INFO}{Localization.Get("LABEL_FORWARD_BACK")}", EntryPoint.OffsetConfig.StretcherAttachOffsetY, v => EntryPoint.OffsetConfig.StretcherAttachOffsetY = v, StretcherGhostManager.UpdateCarryGhost);
            MenuHelpers.AddListControl(StretcherPosMenu, $"{C_HEADER}Z {C_INFO}{Localization.Get("LABEL_UP_DOWN")}", EntryPoint.OffsetConfig.StretcherAttachOffsetZ, v => EntryPoint.OffsetConfig.StretcherAttachOffsetZ = v, StretcherGhostManager.UpdateCarryGhost);

            MenuHelpers.AddDegreeListControl(StretcherPosMenu, $"{C_HEADER}{Localization.Get("LABEL_PITCH")} {C_INFO}{Localization.Get("LABEL_TILT")}", EntryPoint.OffsetConfig.StretcherAttachPitch, v => EntryPoint.OffsetConfig.StretcherAttachPitch = v, StretcherGhostManager.UpdateCarryGhost);
            MenuHelpers.AddDegreeListControl(StretcherPosMenu, $"{C_HEADER}{Localization.Get("LABEL_ROLL")} {C_INFO}{Localization.Get("LABEL_LEAN")}", EntryPoint.OffsetConfig.StretcherAttachRoll, v => EntryPoint.OffsetConfig.StretcherAttachRoll = v, StretcherGhostManager.UpdateCarryGhost);
            MenuHelpers.AddDegreeListControl(StretcherPosMenu, $"{C_HEADER}{Localization.Get("LABEL_YAW")} {C_INFO}{Localization.Get("LABEL_ROTATE")}", EntryPoint.OffsetConfig.StretcherAttachYaw, v => EntryPoint.OffsetConfig.StretcherAttachYaw = v, StretcherGhostManager.UpdateCarryGhost);

            StretcherPosMenu.OnMenuOpen += (s) => StretcherGhostManager.UpdateCarryGhost();
            StretcherPosMenu.OnMenuClose += (s) => StretcherGhostManager.DeleteGhosts();
        }

        #endregion
    }
}