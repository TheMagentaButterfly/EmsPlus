using IPT.Common.User.Settings;
using System.IO;

namespace EmsPlus.Configuration
{
    public class LoadoutConfig : IPT.Common.User.Configuration
    {
        private const string IniFilePath = "Plugins/EmsPlus/Settings/Loadout.ini";

        public SettingString DefaultLoadout = new SettingString("Loadout", "Weapons", "The default loadout you spawn with (Comma separated).", "WEAPON_FLASHLIGHT,WEAPON_FLARE,WEAPON_FIREEXTINGUISHER");

        public override void Load()
        {
            if (!File.Exists(IniFilePath))
            {
                CreateDefaultFile();
            }

            LoadINI(IniFilePath);
            Save();
        }

        public void Save()
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(IniFilePath))
                {
                    writer.WriteLine("; =========================================================");
                    writer.WriteLine("; EmsPlus Loadout Configuration");
                    writer.WriteLine("; =========================================================");
                    writer.WriteLine("; Add weapons here separated by commas.");
                    writer.WriteLine("; You can use simple names (Flashlight) or full names (WEAPON_FLASHLIGHT).");
                    writer.WriteLine("");
                    writer.WriteLine("[Loadout]");
                    writer.WriteLine($"Weapons={(DefaultLoadout.Value ?? "WEAPON_FLASHLIGHT,WEAPON_FLARE,WEAPON_FIREEXTINGUISHER")}");
                }
            }
            catch { }
        }

        private void CreateDefaultFile()
        {
            Save();
        }
    }
}