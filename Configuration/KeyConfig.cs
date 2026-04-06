using IPT.Common.User.Settings;
using System.IO;

namespace EmsPlus.Configuration
{
    public class KeyConfig : IPT.Common.User.Configuration
    {
        private const string IniFilePath = "Plugins/EmsPlus/Settings/Keys.ini";

        public SettingKeyCombo InteractionKey = new SettingKeyCombo("Interaction", "InteractionKey", "The key used for interactions.");

        public SettingKeyCombo OpenMenuKey = new SettingKeyCombo("Settings Menu", "OpenSettingsMenu", "The key used to open the EmsPlus settings menu.");
        public SettingKeyCombo OpenMenuKeyModifier = new SettingKeyCombo("Settings Menu", "OpenSettingsMenuModifier", "The Modifier key used to open the EmsPlus settings menu.");

        public SettingKeyCombo OpenAmbulanceMenuKey = new SettingKeyCombo("Ambulance Menu", "OpenAmbulanceMenu", "The key used to open the Ambulance Interaction menu.");
        public SettingKeyCombo OpenAmbulanceMenuKeyModifier = new SettingKeyCombo("Ambulance Menu", "OpenAmbulanceMenuModifier", "The Modifier key used to open the Ambulance Interaction menu.");

        public SettingKeyCombo StretcherGrabKey = new SettingKeyCombo("Stretcher", "StretcherGrabKey", "Key to grab or release the stretcher.");
        public SettingKeyCombo StretcherHeightKey = new SettingKeyCombo("Stretcher", "StretcherHeightKey", "Key to raise or lower the stretcher.");
        public SettingKeyCombo StretcherSitKey = new SettingKeyCombo("Stretcher", "StretcherSitKey", "Key to toggle patient sitting.");


        public SettingKeyCombo ToggleCabinKey = new SettingKeyCombo("Cabin Toggle", "ToggleCabinKey", "The key used to quickly enter or exit the patient cabin.");
        public SettingKeyCombo ToggleCabinKeyModifier = new SettingKeyCombo("Cabin Toggle", "ToggleCabinKeyModifier", "Modifier for cabin toggle.");

        public SettingKeyCombo ToggleStretcherKey = new SettingKeyCombo("Stretcher Toggle", "ToggleStretcherKey", "The key used to quickly load or unload the stretcher.");
        public SettingKeyCombo ToggleStretcherKeyModifier = new SettingKeyCombo("Stretcher Toggle", "ToggleStretcherKeyModifier", "Modifier for stretcher toggle.");

        public override void Load()
        {
            if (!File.Exists(IniFilePath))
            {
                CreateDefaultFile();
            }

            LoadINI(IniFilePath);
        }

        public void Save()
        {
            SaveINI(IniFilePath);
        }

        private void CreateDefaultFile()
        {
            try
            {
                using (StreamWriter w = new StreamWriter(IniFilePath))
                {
                    w.WriteLine("; =========================================================");
                    w.WriteLine("; EmsPlus Key Configuration");
                    w.WriteLine("; =========================================================");
                    w.WriteLine("; Use this file to change all the keys used by EmsPlus!");
                    w.WriteLine("");

                    w.WriteLine("[Interaction]");
                    w.WriteLine("; Used for general interactions, such as picking up patients or interacting with stations.");
                    w.WriteLine($"InteractionKey=T");
                    w.WriteLine("");

                    w.WriteLine("[Settings Menu]");
                    w.WriteLine("; Used to open the live ingame settings menu.");
                    w.WriteLine($"OpenSettingsMenu=F10");
                    w.WriteLine($"OpenSettingsMenuModifier=None");
                    w.WriteLine("");

                    w.WriteLine("[Ambulance Menu]");
                    w.WriteLine("; Used to open the menu at the rear of your ambulance.");
                    w.WriteLine($"OpenAmbulanceMenu=T");
                    w.WriteLine($"OpenAmbulanceMenuModifier=LMenu");
                    w.WriteLine("");
                    w.WriteLine("");

                    w.WriteLine("; =========================================================");
                    w.WriteLine("; Stretcher Controls");
                    w.WriteLine("; =========================================================");
                    w.WriteLine("[Stretcher]");
                    w.WriteLine($"StretcherGrabKey=G");
                    w.WriteLine($"StretcherHeightKey=H");
                    w.WriteLine($"StretcherSitKey=J");
                    w.WriteLine("");

                    w.WriteLine("; =========================================================");
                    w.WriteLine("; EmsPlus Quick-Action Key Configuration");
                    w.WriteLine("; =========================================================");
                    w.WriteLine("[Cabin Toggle]");
                    w.WriteLine("; This key combo allows you to quickly enter or exit the patient cabin of your ambulance.");
                    w.WriteLine($"ToggleCabinKey=X");
                    w.WriteLine($"ToggleCabinKeyModifier=LMenu");

                    w.WriteLine("[Stretcher Toggle]");
                    w.WriteLine("; This key combo allows you to quickly load or unload the stretcher. (Will open doors automatically)");
                    w.WriteLine($"ToggleStretcherKey=C");
                    w.WriteLine($"ToggleStretcherKeyModifier=LMenu");
                }
            }
            catch { }
        }
    }
}