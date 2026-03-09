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
        public string TraumaBagName = "~r~Trauma Bag";
        public string TraumaBagDesc = "Contains Drugs, IVs, and advanced diagnostic tools.";

        public string OxygenBagModel = "xm_prop_x17_bag_med_01a";
        public string OxygenBagName = "~b~Oxygen Bag";
        public string OxygenBagDesc = "Contains Oxygen tank and masks.";

        public string DefibrillatorModel = "xm_prop_x17_bag_med_01a";
        public string DefibrillatorName = "~g~Defibrillator";
        public string DefibrillatorDesc = "ECG, DEFIB, SpO2, NIBP.";

        // Medical Props
        public string LucasPropModel = "xm_prop_x17_bag_med_01a";

        public override void Load()
        {
            if (!File.Exists(IniFilePath)) CreateDefaultFile();

            InitializationFile ini = new InitializationFile(IniFilePath);

            StretcherModel = ini.ReadString("Stretcher", "StretcherModelName", "m23_2_prop_m32_lgstretcher_01a");
            StretcherModelLowered = ini.ReadString("Stretcher", "LoweredStretcherModelName", "m23_2_prop_m32_lgstretcher_01a");
            StretcherModelSitting = ini.ReadString("Stretcher", "SittingStretcherModelName", "m23_2_prop_m32_lgstretcher_01a");


            TraumaBagModel = ini.ReadString("TraumaBag", "Model", "xm_prop_x17_bag_med_01a");
            TraumaBagName = ini.ReadString("TraumaBag", "Name", "~r~Trauma Bag");
            TraumaBagDesc = ini.ReadString("TraumaBag", "Description", "Contains Drugs, IVs, and advanced diagnostic tools.");

            OxygenBagModel = ini.ReadString("OxygenBag", "Model", "xm_prop_x17_bag_med_01a");
            OxygenBagName = ini.ReadString("OxygenBag", "Name", "~b~Oxygen Bag");
            OxygenBagDesc = ini.ReadString("OxygenBag", "Description", "Contains Oxygen tank and masks.");

            DefibrillatorModel = ini.ReadString("Defibrillator", "Model", "xm_prop_x17_bag_med_01a");
            DefibrillatorName = ini.ReadString("Defibrillator", "Name", "~g~Defibrillator");
            DefibrillatorDesc = ini.ReadString("Defibrillator", "Description", "ECG, DEFIB, SpO2, NIBP.");


            LucasPropModel = ini.ReadString("MedicalProps", "LucasPropModelName", "xm_prop_x17_bag_med_01a");

            Game.Console.Print($"[EmsPlus] Config Loaded. Stretcher Model: {StretcherModel}");
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

                    writer.WriteLine("[Kit Configuration]");
                    writer.WriteLine("[TraumaBag]");
                    writer.WriteLine($"Name={TraumaBagName}");
                    writer.WriteLine($"Description={TraumaBagDesc}");
                    writer.WriteLine($"Model={TraumaBagModel}");
                    writer.WriteLine("");

                    writer.WriteLine("[OxygenBag]");
                    writer.WriteLine($"Name={OxygenBagName}");
                    writer.WriteLine($"Description={OxygenBagDesc}");
                    writer.WriteLine($"Model={OxygenBagModel}");
                    writer.WriteLine("");

                    writer.WriteLine("[Defibrillator]");
                    writer.WriteLine($"Name={DefibrillatorName}");
                    writer.WriteLine($"Description={DefibrillatorDesc}");
                    writer.WriteLine($"Model={DefibrillatorModel}");
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