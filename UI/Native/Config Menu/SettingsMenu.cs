using EmsPlus.Managers;
using IPT.Common.User.Settings;
using Rage;
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
                $"{C_HEADER}{Localization.Get("MENU_CONFIG_TITLE", "Configuration")}",
                Localization.Get("MENU_CONFIG_SUBTITLE", "Settings")
            );
            MenuCore.AddMenu(MenuCore.SettingsMenu);

            var btnForceCallout = new UIMenuItem(
                $"{BULLET} {Localization.Get("ITEM_FORCE_CALLOUT", "Force Callout")}",
                Localization.Get("ITEM_FORCE_CALLOUT_DESC", "Force a callout to occur")
            );
            var chkTutorial = new UIMenuCheckboxItem(
                $"{BULLET} {Localization.Get("ITEM_ENABLE_TUTORIAL", "Enable Tutorial")}",
                EntryPoint.EmsPlusConfig.EnableTutorial.Value,
                Localization.Get("DESC_ENABLE_TUTORIAL", "Enable or disable the tutorial")
            );
            var btnOffsets = new UIMenuItem(
                $"{BULLET} {Localization.Get("ITEM_OFFSETS_POSITIONS", "Offsets & Positions")}",
                Localization.Get("ITEM_OFFSETS_POSITIONS_DESC", "Configure offsets and positions")
            );
            var btnSave = new UIMenuItem(
                $"{C_SUCCESS}{Localization.Get("ITEM_SAVE_SETTINGS", "Save Settings")}",
                Localization.Get("ITEM_SAVE_SETTINGS_DESC", "Save the current settings")
            );

            MenuCore.SettingsMenu.AddItem(btnForceCallout);
            MenuCore.SettingsMenu.AddItem(chkTutorial);
            MenuCore.SettingsMenu.AddItem(btnOffsets);
            MenuCore.SettingsMenu.AddItem(btnSave);
            MenuCore.SettingsMenu.BindMenuToItem(ForceCalloutMenu, btnForceCallout);
            MenuCore.SettingsMenu.BindMenuToItem(OffsetsRootMenu, btnOffsets);

            MenuCore.SettingsMenu.OnItemSelect += (s, item, i) =>
            {
                if (item != btnSave) return;

                EntryPoint.OffsetConfig.Save();

                if (AmbulanceManager.CurrentConfig == null)
                {
                    Game.DisplayNotification($"{C_SUCCESS}{Localization.Get("NOTIF_SETTINGS_SAVED_GENERIC", "Settings saved successfully.")}");
                    return;
                }

                if (EntryPoint.EmsPlusConfig.IsAllowed(AmbulanceManager.CurrentConfig.ModelName))
                {
                    AmbulanceManager.CurrentConfig.Save();
                    Game.DisplayNotification($"{C_SUCCESS}{string.Format(Localization.Get("NOTIF_SETTINGS_SAVED_VEHICLE", "Settings saved successfully for {0}."), AmbulanceManager.CurrentConfig.ModelName)}");
                }
                else
                {
                    Game.DisplayNotification($"{C_WARNING}{Localization.Get("NOTIF_CONFIG_NOT_SAVED", "Configuration not saved.")}");
                }
            };

            chkTutorial.CheckboxEvent += (s, isChecked) =>
            {
                EntryPoint.EmsPlusConfig.EnableTutorial = new SettingBool("Tutorial", "EnableTutorial", "Set to false to skip all tutorials.", isChecked);

                EntryPoint.EmsPlusConfig.Save();
            };
        }

        #endregion
    }
}