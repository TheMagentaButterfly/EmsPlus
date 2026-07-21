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

        public SettingKeyCombo OpenBackupMenuKey = new SettingKeyCombo("Backup Menu", "OpenBackupMenu", "The key used to open the Backup menu.");
        public SettingKeyCombo OpenBackupMenuKeyModifier = new SettingKeyCombo("Backup Menu", "OpenBackupMenuModifier", "The Modifier key used to open the Backup menu.");
        public SettingKeyCombo OpenBackupManagerMenuKey = new SettingKeyCombo("Backup Menu", "OpenBackupManagerMenu", "The key used to open the Backup Manager menu.");
        public SettingKeyCombo OpenBackupManagerMenuKeyModifier = new SettingKeyCombo("Backup Menu", "OpenBackupManagerMenuModifier", "The Modifier key used to open the Backup Manager menu.");

        public SettingKeyCombo StretcherGrabKey = new SettingKeyCombo("Stretcher", "StretcherGrabKey", "Key to grab or release the stretcher.");
        public SettingKeyCombo StretcherHeightKey = new SettingKeyCombo("Stretcher", "StretcherHeightKey", "Key to raise or lower the stretcher.");
        public SettingKeyCombo StretcherSitKey = new SettingKeyCombo("Stretcher", "StretcherSitKey", "Key to toggle patient sitting.");


        public SettingKeyCombo ToggleCabinKey = new SettingKeyCombo("Cabin Toggle", "ToggleCabinKey", "The key used to quickly enter or exit the patient cabin.");
        public SettingKeyCombo ToggleCabinKeyModifier = new SettingKeyCombo("Cabin Toggle", "ToggleCabinKeyModifier", "Modifier for cabin toggle.");

        public SettingKeyCombo ToggleStretcherKey = new SettingKeyCombo("Stretcher Toggle", "ToggleStretcherKey", "The key used to quickly load or unload the stretcher.");
        public SettingKeyCombo ToggleStretcherKeyModifier = new SettingKeyCombo("Stretcher Toggle", "ToggleStretcherKeyModifier", "Modifier for stretcher toggle.");

        public SettingKeyCombo OpenMdtKey = new SettingKeyCombo("MDT", "OpenMdtKey", "Hold to open MDT, tap to close.");

        public override void Load()
        {
            if (!File.Exists(IniFilePath))
            {
                CreateDefaultFile();
            }

            LoadINI(IniFilePath);
            Save();
        }

        private void Save()
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
                    w.WriteLine($"InteractionKey={GetPrimaryKeyOnly(InteractionKey.Value, "T")}");
                    w.WriteLine("");

                    w.WriteLine("[Settings Menu]");
                    w.WriteLine("; Used to open the live ingame settings menu.");
                    w.WriteLine($"OpenSettingsMenu={GetPrimaryKeyOnly(OpenMenuKey.Value, "F10")}");
                    w.WriteLine($"OpenSettingsMenuModifier={GetModifierOnly(OpenMenuKeyModifier.Value, "None")}");
                    w.WriteLine("");

                    w.WriteLine("[Ambulance Menu]");
                    w.WriteLine("; Used to open the menu at the rear of your ambulance.");
                    w.WriteLine($"OpenAmbulanceMenu={GetPrimaryKeyOnly(OpenAmbulanceMenuKey.Value, "T")}");
                    w.WriteLine($"OpenAmbulanceMenuModifier={GetModifierOnly(OpenAmbulanceMenuKeyModifier.Value, "LMenu")}");
                    w.WriteLine("");

                    w.WriteLine("[Backup Menu]");
                    w.WriteLine("; Used to open the backup request menu.");
                    w.WriteLine($"OpenBackupMenu={GetPrimaryKeyOnly(OpenBackupMenuKey.Value, "B")}");
                    w.WriteLine($"OpenBackupMenuModifier={GetModifierOnly(OpenBackupMenuKeyModifier.Value, "None")}");
                    w.WriteLine("; Used to open the backup management menu for AI units.");
                    w.WriteLine($"OpenBackupManagerMenu={GetPrimaryKeyOnly(OpenBackupManagerMenuKey.Value, "B")}");
                    w.WriteLine($"OpenBackupManagerMenuModifier={GetModifierOnly(OpenBackupManagerMenuKeyModifier.Value, "LMenu")}");
                    w.WriteLine("");

                    w.WriteLine("[MDT]");
                    w.WriteLine("; Hold this key to open the MDT screen. Tap it while open to close.");
                    w.WriteLine($"OpenMdtKey={GetPrimaryKeyOnly(OpenMdtKey.Value, "F5")}");
                    w.WriteLine("");

                    w.WriteLine("; =========================================================");
                    w.WriteLine("; Stretcher Controls");
                    w.WriteLine("; =========================================================");
                    w.WriteLine("[Stretcher]");
                    w.WriteLine($"StretcherGrabKey={GetPrimaryKeyOnly(StretcherGrabKey.Value, "G")}");
                    w.WriteLine($"StretcherHeightKey={GetPrimaryKeyOnly(StretcherHeightKey.Value, "H")}");
                    w.WriteLine($"StretcherSitKey={GetPrimaryKeyOnly(StretcherSitKey.Value,  "J")}");
                    w.WriteLine("");

                    w.WriteLine("; =========================================================");
                    w.WriteLine("; EmsPlus Quick-Action Key Configuration");
                    w.WriteLine("; =========================================================");
                    w.WriteLine("[Cabin Toggle]");
                    w.WriteLine("; This key combo allows you to quickly enter or exit the patient cabin of your ambulance.");
                    w.WriteLine($"ToggleCabinKey={GetPrimaryKeyOnly(ToggleCabinKey.Value, "X")}");
                    w.WriteLine($"ToggleCabinKeyModifier={GetModifierOnly(ToggleCabinKeyModifier.Value, "LMenu")}");

                    w.WriteLine("[Stretcher Toggle]");
                    w.WriteLine("; This key combo allows you to quickly load or unload the stretcher. (Will open doors automatically)");
                    w.WriteLine($"ToggleStretcherKey={GetPrimaryKeyOnly(ToggleStretcherKey.Value, "C")}");
                    w.WriteLine($"ToggleStretcherKeyModifier={GetModifierOnly(ToggleStretcherKeyModifier.Value, "LMenu")}");
                }
            }
            catch { }
        }

        private void CreateDefaultFile()
        {
            Save();
        }

        /// <summary>
        /// Helper to parse out the primary key from a modifier+key combination string (e.g. extracts 'T' from 'LMenu+T').
        /// </summary>
        private string GetPrimaryKeyOnly(object keyCombo, string defaultKey)
        {
            if (keyCombo == null) return defaultKey;
            string str = keyCombo.ToString();
            if (string.IsNullOrEmpty(str)) return defaultKey;

            if (str.Contains("+"))
            {
                string[] parts = str.Split('+');
                return parts[parts.Length - 1].Trim();
            }
            return str;
        }

        /// <summary>
        /// Helper to get the modifier key representation safely.
        /// </summary>
        private string GetModifierOnly(object keyCombo, string defaultModifier)
        {
            if (keyCombo == null) return defaultModifier;
            string str = keyCombo.ToString();
            if (string.IsNullOrEmpty(str)) return defaultModifier;
            return str;
        }
    }
}