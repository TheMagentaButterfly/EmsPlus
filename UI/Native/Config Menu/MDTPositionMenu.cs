using EmsPlus.Managers;

namespace EmsPlus.UI.Native.ConfigMenu
{
    public static partial class ConfigMenuBuilder
    {
        private static void BuildMdtPosMenu()
        {
            MenuHelpers.AddListControl(MdtPosMenu, $"{C_HEADER}{Localization.Get("LABEL_SCALE", "Scale")} {C_INFO}(Size)", EntryPoint.OffsetConfig.MdtScale, v => {
                EntryPoint.OffsetConfig.MdtScale = v < 0.1f ? 0.1f : v;
                MdtManager.ForceUpdateLayout();
            }, null);

            MenuHelpers.AddListControl(MdtPosMenu, $"{C_HEADER}X {C_INFO}{Localization.Get("LABEL_LEFT_RIGHT", "(Left/Right)")}", EntryPoint.OffsetConfig.MdtOffsetX, v => {
                EntryPoint.OffsetConfig.MdtOffsetX = v;
                MdtManager.ForceUpdateLayout();
            }, null);

            MenuHelpers.AddListControl(MdtPosMenu, $"{C_HEADER}Y {C_INFO}{Localization.Get("LABEL_UP_DOWN", "(Up/Down)")}", EntryPoint.OffsetConfig.MdtOffsetY, v => {
                EntryPoint.OffsetConfig.MdtOffsetY = v;
                MdtManager.ForceUpdateLayout();
            }, null);

            MdtPosMenu.OnMenuOpen += (s) =>
            {
                if (!MdtManager.IsVisible) MdtManager.Toggle(true);
            };

            MdtPosMenu.OnMenuClose += (s) =>
            {
                if (MdtManager.IsVisible) MdtManager.Toggle(false);
            };
        }
    }
}