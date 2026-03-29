using Rage;
using EmsPlus.Managers;
using RAGENativeUI;
using RAGENativeUI.Elements;

namespace EmsPlus.UI.Native.ConfigMenu
{
    public static partial class ConfigMenuBuilder
    {
        #region Settings Menu

        private static void BuildSettingsMenu()
        {
            MenuCore.SettingsMenu = new UIMenu(
                $"{C_HEADER}{Localization.Get("MENU_CONFIG_TITLE")}",
                Localization.Get("MENU_CONFIG_SUBTITLE")
            );
            MenuCore.AddMenu(MenuCore.SettingsMenu);

            var btnOffsets = new UIMenuItem(
                $"{BULLET} {Localization.Get("ITEM_OFFSETS_POSITIONS")}",
                Localization.Get("ITEM_OFFSETS_POSITIONS_DESC")
            );
            var btnSave = new UIMenuItem(
                $"{C_SUCCESS}{Localization.Get("ITEM_SAVE_SETTINGS")}",
                Localization.Get("ITEM_SAVE_SETTINGS_DESC")
            );

            MenuCore.SettingsMenu.AddItem(btnOffsets);
            MenuCore.SettingsMenu.AddItem(btnSave);
            MenuCore.SettingsMenu.BindMenuToItem(OffsetsRootMenu, btnOffsets);

            MenuCore.SettingsMenu.OnItemSelect += (s, item, i) =>
            {
                if (item != btnSave) return;

                EntryPoint.OffsetConfig.Save();

                if (AmbulanceManager.CurrentConfig == null)
                {
                    Game.DisplayNotification($"{C_SUCCESS}{Localization.Get("NOTIF_SETTINGS_SAVED_GENERIC")}");
                    return;
                }

                if (EntryPoint.EmsPlusConfig.IsAllowed(AmbulanceManager.CurrentConfig.ModelName))
                {
                    AmbulanceManager.CurrentConfig.Save();
                    Game.DisplayNotification($"{C_SUCCESS}{string.Format(Localization.Get("NOTIF_SETTINGS_SAVED_VEHICLE"), AmbulanceManager.CurrentConfig.ModelName)}");
                }
                else
                {
                    Game.DisplayNotification($"{C_WARNING}{Localization.Get("NOTIF_CONFIG_NOT_SAVED")}");
                }
            };
        }

        #endregion
    }
}