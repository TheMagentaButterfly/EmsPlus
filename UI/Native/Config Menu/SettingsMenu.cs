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
                $"{C_HEADER}{Localization.Get("MENU_CONFIG_TITLE")}",
                Localization.Get("MENU_CONFIG_SUBTITLE")
            );
            MenuCore.AddMenu(MenuCore.SettingsMenu);

            var btnForceCallout = new UIMenuItem(
                $"{BULLET} {Localization.Get("ITEM_FORCE_CALLOUT")}",
                Localization.Get("ITEM_FORCE_CALLOUT_DESC")
            );
            var chkTutorial = new UIMenuCheckboxItem(
                $"{BULLET} {Localization.Get("ITEM_ENABLE_TUTORIAL")}",
                EntryPoint.EmsPlusConfig.EnableTutorial.Value,
                Localization.Get("DESC_ENABLE_TUTORIAL")
            );
            var btnOffsets = new UIMenuItem(
                $"{BULLET} {Localization.Get("ITEM_OFFSETS_POSITIONS")}",
                Localization.Get("ITEM_OFFSETS_POSITIONS_DESC")
            );
            var btnSave = new UIMenuItem(
                $"{C_SUCCESS}{Localization.Get("ITEM_SAVE_SETTINGS")}",
                Localization.Get("ITEM_SAVE_SETTINGS_DESC")
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

            chkTutorial.CheckboxEvent += (s, isChecked) =>
            {
                EntryPoint.EmsPlusConfig.EnableTutorial = new SettingBool("Tutorial", "EnableTutorial", "Set to false to skip all tutorials.", isChecked);

                EntryPoint.EmsPlusConfig.Save();
            };
        }

        #endregion
    }
}