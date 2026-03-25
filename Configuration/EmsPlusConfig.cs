using IPT.Common.User.Settings;
using System.Collections.Generic;
using System.IO;

namespace EmsPlus.Configuration
{
    public class EmsPlusConfig : IPT.Common.User.Configuration
    {
        private const string IniFilePath = "Plugins/EmsPlus/EmsPlus.ini";
        
        // General Settings
        public SettingString Language = new SettingString("General", "Language", "The name of the file in the Localization folder (without .ini).", "English");
        public SettingString AllowedVehicles = new SettingString("General", "AllowedVehicles", "Comma separated list of model names allowed to be used as ambulances.", "ambulance");
        public SettingBool ShowAmbulancePrompts = new SettingBool("General", "ShowAmbulancePrompts", "If true, interaction circles will show around ambulances.", false);
        public SettingBool UseCustomInteractionPoints = new SettingBool("General", "UseCustomInteractionPoints", "If true, uses the configurable points. If false, uses default rear door logic.", false);
        public SettingBool UseNativeUIPatientMenu = new SettingBool("General", "UseNativeUIPatientMenu", "If true, uses standard NativeUI instead of the custom 3D inspection menu.", false);
        public List<string> ValidAmbulanceModels { get; private set; } = new List<string>();

        // Callout Settings
        public SettingInt CalloutMultiplier { get; set; } = new SettingInt("Callout Settings", "CalloutMultiplier", "Enables or disables callout difficulty multiplier based on player count.", 1, 0, 50, 1);

        // Difficulty Settings
        public SettingBool EnablePatientDeath = new SettingBool("Difficulty", "EnablePatientDeath", "If true, patients can die if vitals reach critical levels.", true);
        public SettingInt DegradationSpeed = new SettingInt("Difficulty", "DegradationSpeed", "How fast the patient worsens (1-5, 5 is fastest).", 2, 1, 5, 2);
        //public SettingBool ShowTreatmentHints = new SettingBool("Difficulty", "ShowTreatmentHints", "If true, the menu will suggest treatments.", true);

        public SettingBool AdvancedDebugging = new SettingBool("Debug", "AdvancedDebugging", "If true, enables advanced debugging features.", false);

        public override void Load()
        {
            if (!File.Exists(IniFilePath)) CreateDefaultFile();
            LoadINI(IniFilePath);
            ParseAllowedVehicles();
        }

        private void ParseAllowedVehicles()
        {
            ValidAmbulanceModels.Clear();
            if (!string.IsNullOrEmpty(AllowedVehicles.Value))
            {
                string[] parts = AllowedVehicles.Value.Split(',');
                foreach (string part in parts)
                {
                    string clean = part.Trim().ToLower();
                    if (!string.IsNullOrEmpty(clean))
                        ValidAmbulanceModels.Add(clean);
                }
            }
        }

        public void AddAllowedVehicle(string modelName)
        {
            string lower = modelName.ToLower();
            if (!ValidAmbulanceModels.Contains(lower))
            {
                ValidAmbulanceModels.Add(lower);
                UpdateAllowedString();
                Save();
            }
        }

        public void RemoveAllowedVehicle(string modelName)
        {
            string lower = modelName.ToLower();
            if (ValidAmbulanceModels.Contains(lower))
            {
                ValidAmbulanceModels.Remove(lower);
                UpdateAllowedString();
                Save();
            }
        }

        private void UpdateAllowedString()
        {
            string newList = string.Join(", ", ValidAmbulanceModels);

            AllowedVehicles = new SettingString("General", "AllowedVehicles", "Comma separated list of model names allowed to be used as ambulances.", newList);
        }

        public bool IsAllowed(string modelName)
        {
            return ValidAmbulanceModels.Contains(modelName.ToLower());
        }

        public void Save()
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(IniFilePath))
                {
                    writer.WriteLine("; =========================================================");
                    writer.WriteLine("; EmsPlus Main Configuration");
                    writer.WriteLine("; =========================================================");
                    writer.WriteLine("; Use this file to change global settings about EmsPlus.");
                    writer.WriteLine("");

                    writer.WriteLine("[General]");
                    writer.WriteLine("; The name of the file in the Localization folder (without .ini).");
                    writer.WriteLine($"Language={Language.Value}");
                    writer.WriteLine("");
                    writer.WriteLine("; Define which vehicles allow stretcher/gear interaction (Comma separated).");
                    writer.WriteLine("; You can add/remove vehicles here manually or via the in-game menu.");
                    writer.WriteLine($"AllowedVehicles={AllowedVehicles.Value}");
                    writer.WriteLine("");
                    writer.WriteLine("; If true, uses the custom interaction points defined in Vehicle configs.");
                    writer.WriteLine("; If false, uses the default 'Near Rear Doors' logic for all vehicles.");
                    writer.WriteLine($"UseCustomInteractionPoints={UseCustomInteractionPoints.Value}");
                    writer.WriteLine("; If true, interaction circles will show around ambulances. (true/false)");
                    writer.WriteLine($"ShowAmbulancePrompts={ShowAmbulancePrompts.Value}");
                    writer.WriteLine("");
                    //writer.WriteLine("; If true, the custom UI will be used for the petient interaction. (true/false)");
                    //writer.WriteLine($"UseNativeUIPatientMenu={UseNativeUIPatientMenu.Value}");
                    //writer.WriteLine("");

                    writer.WriteLine("[CalloutMultiplier]");
                    writer.WriteLine("; Adjusts frequency of callouts.");
                    writer.WriteLine("; 0 = Disable, Higher = Less Frequent, Lower = More Frequent.");
                    writer.WriteLine($"CalloutMultiplier={CalloutMultiplier.Value}");
                    writer.WriteLine("");

                    writer.WriteLine("[Difficulty]");
                    writer.WriteLine("; If true, patients can die if vitals reach critical levels.");
                    writer.WriteLine($"EnablePatientDeath={EnablePatientDeath.Value}");
                    writer.WriteLine("");
                    writer.WriteLine("; How fast the patient condition worsens (1 = Slow, 5 = Fast).");
                    writer.WriteLine($"DegradationSpeed={DegradationSpeed.Value}");
                    writer.WriteLine("");
                    //writer.WriteLine("; If true, the menu will suggest treatments based on diagnostics.");
                    //writer.WriteLine($"ShowTreatmentHints={ShowTreatmentHints.Value}");
                    //writer.WriteLine("");

                    //writer.WriteLine("[Debug]");
                    //writer.WriteLine("; If true, enables advanced debugging features.");
                    //writer.WriteLine($"AdvancedDebugging={AdvancedDebugging.Value}");
                }
            }
            catch (System.Exception ex)
            {
                Rage.Game.Console.Print($"[EmsPlus] Error saving EmsPlus.ini: {ex.Message}");
            }
        }

        private void CreateDefaultFile()
        {
            Save();
        }
    }
}