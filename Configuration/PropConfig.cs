using IPT.Common.User.Settings;
using Rage;
using System.Collections.Generic;
using System.IO;

namespace EmsPlus.Configuration
{
    public class PropConfig : IPT.Common.User.Configuration
    {
        private const string IniFilePath = "Plugins/EmsPlus/Settings/Props.ini";

        // Stretcher Models
        public string StretcherModel = "m23_2_prop_m32_lgstretcher_01a";
        public string StretcherModelLowered = "m23_2_prop_m32_lgstretcher_01a";
        public string StretcherModelSitting = "m23_2_prop_m32_lgstretcher_01a";

        // Kit Models
        public string TraumaBagModel = "xm_prop_x17_bag_med_01a";
        public string OxygenBagModel = "xm_prop_x17_bag_med_01a";
        public string DefibrillatorModel = "xm_prop_x17_bag_med_01a";

        // Medical Props
        public string LucasPropModel = "xm_prop_x17_bag_med_01a";

        public override void Load()
        {
            if (!File.Exists(IniFilePath)) CreateDefaultFile();

            InitializationFile ini = new InitializationFile(IniFilePath);

            StretcherModel = ini.ReadString("Stretcher", "StretcherModelName", "m23_2_prop_m32_lgstretcher_01a");
            StretcherModelLowered = ini.ReadString("Stretcher", "LoweredStretcherModelName", "m23_2_prop_m32_lgstretcher_01a");
            StretcherModelSitting = ini.ReadString("Stretcher", "SittingStretcherModelName", "m23_2_prop_m32_lgstretcher_01a");

            TraumaBagModel = ini.ReadString("Kits", "TraumaBagModel", "xm_prop_x17_bag_med_01a");
            OxygenBagModel = ini.ReadString("Kits", "OxygenBagModel", "xm_prop_x17_bag_med_01a");
            DefibrillatorModel = ini.ReadString("Kits", "DefibrillatorModel", "xm_prop_x17_bag_med_01a");

            LucasPropModel = ini.ReadString("MedicalProps", "LucasPropModelName", "xm_prop_x17_bag_med_01a");

            Save();

            Game.Console.Print($"[EmsPlus] Config Loaded. Stretcher Model: {StretcherModel}");
        }

        public void Save()
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

                    writer.WriteLine("[Kits]");
                    writer.WriteLine($"TraumaBagModel={TraumaBagModel}");
                    writer.WriteLine($"OxygenBagModel={OxygenBagModel}");
                    writer.WriteLine($"DefibrillatorModel={DefibrillatorModel}");
                    writer.WriteLine("");

                    writer.WriteLine("[MedicalProps]");
                    writer.WriteLine("; Your default models for the Medical Props like the Lucas");
                    writer.WriteLine($"LucasPropModelName={LucasPropModel}");
                }
            }
            catch (System.Exception ex)
            {
                Game.Console.Print($"[EmsPlus] Error: {ex.Message}");
            }
        }

        private void CreateDefaultFile()
        {
            Save();
        }
    }
}