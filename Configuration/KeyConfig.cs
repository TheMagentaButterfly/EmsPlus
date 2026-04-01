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

        public SettingKeyCombo ToggleCabinKey = new SettingKeyCombo("Ambulance Menu", "ToggleCabinKey", "The key used to quickly enter or exit the patient cabin.");
        public SettingKeyCombo ToggleCabinKeyModifier = new SettingKeyCombo("Ambulance Menu", "ToggleCabinKeyModifier", "Modifier for cabin toggle.");

        public SettingKeyCombo StretcherGrabKey = new SettingKeyCombo("Stretcher", "StretcherGrabKey", "Key to grab or release the stretcher.");
        public SettingKeyCombo StretcherHeightKey = new SettingKeyCombo("Stretcher", "StretcherHeightKey", "Key to raise or lower the stretcher.");
        public SettingKeyCombo StretcherSitKey = new SettingKeyCombo("Stretcher", "StretcherSitKey", "Key to toggle patient sitting.");

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
                    w.WriteLine($"StretcherGrabKey={StretcherGrabKey.Value}");
                    w.WriteLine($"StretcherHeightKey={StretcherHeightKey.Value}");
                    w.WriteLine($"StretcherSitKey={StretcherSitKey.Value}");
                    w.WriteLine("");

                    w.WriteLine("; =========================================================");
                    w.WriteLine("; EmsPlus Quick-Action Key Configuration");
                    w.WriteLine("; =========================================================");
                    w.WriteLine("[Cabin Toggle]");
                    w.WriteLine("; This key combo allows you to quickly enter or exit the patient cabin of your ambulance.");
                    w.WriteLine($"ToggleCabinKey={ToggleCabinKey.Value}");
                    w.WriteLine($"ToggleCabinKeyModifier={ToggleCabinKeyModifier.Value}");
                }
            }
            catch { }
        }
    }
}