using IPT.Common.User.Settings;
using Rage;
using System.Collections.Generic;
using System.IO;

namespace EmsPlus.Configuration
{
    public class PropConfig : IPT.Common.User.Configuration
    {
        private const string IniFilePath = "Plugins/EmsPlus/Settings/Props.ini";

        // Stretcher Models - Using raw strings instead of SettingString wrappers for manual control
        public string StretcherModel = "m23_2_prop_m32_lgstretcher_01a";
        public string StretcherModelLowered = "m23_2_prop_m32_lgstretcher_01a";
        public string StretcherModelSitting = "m23_2_prop_m32_lgstretcher_01a";

        // Medical Props
        public string LucasPropModel = "xm_prop_x17_bag_med_01a";

        public override void Load()
        {
            if (!File.Exists(IniFilePath)) CreateDefaultFile();

            InitializationFile ini = new InitializationFile(IniFilePath);

            // Manual Read - This guarantees we get the value or the default
            StretcherModel = ini.ReadString("Stretcher", "StretcherModelName", "m23_2_prop_m32_lgstretcher_01a");
            StretcherModelLowered = ini.ReadString("Stretcher", "LoweredStretcherModelName", "m23_2_prop_m32_lgstretcher_01a");
            StretcherModelSitting = ini.ReadString("Stretcher", "SittingStretcherModelName", "m23_2_prop_m32_lgstretcher_01a");

            LucasPropModel = ini.ReadString("MedicalProps", "LucasPropModelName", "xm_prop_x17_bag_med_01a");

            // Debug print to confirm what we loaded
            Game.Console.Print($"[EmsPlus] Config Loaded. Stretcher Model: {StretcherModel}");
        }

        public void Save()
        {
            InitializationFile ini = new InitializationFile(IniFilePath);
            ini.Write("Stretcher", "StretcherModelName", StretcherModel);
            ini.Write("Stretcher", "LoweredStretcherModelName", StretcherModelLowered);
            ini.Write("Stretcher", "StretcherModelSitting", StretcherModelSitting);
            ini.Write("MedicalProps", "LucasPropModelName", LucasPropModel);
        }

        private void CreateDefaultFile()
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(IniFilePath))
                {
                    writer.WriteLine("; =========================================================");
                    writer.WriteLine("; EmsPlus Prop Configuration");
                    writer.WriteLine("; =========================================================");
                    writer.WriteLine("; Use this file to change the props used by EmsPlus!");
                    writer.WriteLine("; To find valid vanilla models i suggest using this website: forge.plebmasters.de/objects");
                    writer.WriteLine("");

                    writer.WriteLine("[Stretcher]");
                    writer.WriteLine("; Your default stretcher model");
                    writer.WriteLine($"StretcherModelName={StretcherModel}");
                    writer.WriteLine("; The stretcher model when it has been lowered");
                    writer.WriteLine($"LoweredStretcherModelName={StretcherModelLowered}");
                    writer.WriteLine("; The stretcher model when you want the patient to sit");
                    writer.WriteLine($"SittingStretcherModelName={StretcherModelSitting}");
                    writer.WriteLine("");

                    writer.WriteLine("[MedicalProps]");
                    writer.WriteLine("; Your default models for the Medical Props like the Lucas");
                    writer.WriteLine($"LucasPropModelName={LucasPropModel}");
                }
            }
            catch { }
        }
    }
}