using IPT.Common.User.Settings;
using System.IO;

namespace EmsPlus.Configuration
{
    public class AnimationConfig : IPT.Common.User.Configuration
    {
        private const string IniFilePath = "Plugins/EmsPlus/Settings/Animations.ini";

        public SettingString MedicAssessDict = new SettingString("Medic", "AssessmentAnimationDictionary", "", "amb@medic@standing@kneel@idle_a");
        public SettingString MedicAssessName = new SettingString("Medic", "AssessmentAnimationName", "", "idle_a");
        public SettingString MedicTreatDict = new SettingString("Medic", "TreatmentAnimationDictionary", "", "amb@medic@standing@tendtodead@idle_a");
        public SettingString MedicTreatName = new SettingString("Medic", "TreatmentAnimationName", "", "idle_a");
        public SettingString MedicNoteDict = new SettingString("Medic", "NoteAnimationDictionary", "", "amb@medic@standing@timeofdeath@base");
        public SettingString MedicNoteName = new SettingString("Medic", "NoteAnimationName", "", "base");
        public SettingString MedicSitDict = new SettingString("Medic", "SittingAnimationDictionary", "", "anim@heists@fleeca_bank@hostages@intro");
        public SettingString MedicSitName = new SettingString("Medic", "SittingAnimationName", "", "intro_loop_ped_a");
        public SettingString MedicStretcherCarryDict = new SettingString("Medic", "StretcherCarryingAnimationDictionary", "", "anim@heists@box_carry@");
        public SettingString MedicStretcherCarryName = new SettingString("Medic", "StretcherCarryingAnimationName", "", "idle");
        public SettingString InteractDict = new SettingString("Medic", "InteractionAnimationDictionary", "", "anim@narcotics@trash");
        public SettingString InteractName = new SettingString("Medic", "InteractionAnimationName", "", "drop_front");

        public SettingString PatientUnconDict = new SettingString("Patient", "UnconsciousAnimationDictionary", "", "misslamar1dead_body");
        public SettingString PatientUnconName = new SettingString("Patient", "UnconsciousAnimationName", "", "dead_idle");
        public SettingString PatientHunchedDict = new SettingString("Patient", "HunchedAnimationDictionary", "", "misschinese2_crystalmaze");
        public SettingString PatientHunchedName = new SettingString("Patient", "HunchedAnimationName", "", "2int_loop_a_taocheng");
        public SettingString PatientStandingDict = new SettingString("Patient", "StandingPainAnimationDictionary", "", "rcmfanatic1out_of_breath");
        public SettingString PatientStandingName = new SettingString("Patient", "StandingPainAnimationName", "", "p_zero_tired_01e");
        public SettingString PatientStretcherDict = new SettingString("Patient", "OnStretcherAnimationDictionary", "", "amb@world_human_sunbathe@female@back@base");
        public SettingString PatientStretcherName = new SettingString("Patient", "OnStretcherAnimationName", "", "base");
        public SettingString PatientSittingStretcherDict = new SettingString("Patient", "SittingOnStretcherAnimationDictionary", "", "anim@amb@business@bgen@bgen_no_work@");
        public SettingString PatientSittingStretcherName = new SettingString("Patient", "SittingOnStretcherAnimationName", "", "sit_phone_phoneputdown_idle_nowork");
        public SettingString PatientReviveDict = new SettingString("Patient", "ReviveAnimationDictionary", "", "amb@world_human_sunbathe@female@back@base");
        public SettingString PatientReviveName = new SettingString("Patient", "ReviveAnimationName", "", "base");

        public SettingString BystanderWaveDict = new SettingString("Bystander", "BystanderWaveAnimationDictionary", "", "anim@amb@waving@male");
        public SettingString BystanderWaveName = new SettingString("Bystander", "BystanderWaveAnimationName", "", "ground_wave");

        public override void Load()
        {
            if (!File.Exists(IniFilePath)) Save();
            LoadINI(IniFilePath);
            Save();
        }

        public void Save()
        {
            try
            {
                using (var w = new StreamWriter(IniFilePath))
                {
                    w.WriteLine("[Medic]");
                    w.WriteLine($"AssessmentAnimationDictionary={MedicAssessDict.Value}");
                    w.WriteLine($"AssessmentAnimationName={MedicAssessName.Value}");
                    // BUG FIX: Corrected keys from Assessment* to Treatment*
                    w.WriteLine($"TreatmentAnimationDictionary={MedicTreatDict.Value}");
                    w.WriteLine($"TreatmentAnimationName={MedicTreatName.Value}");
                    w.WriteLine($"NoteAnimationDictionary={MedicNoteDict.Value}");
                    w.WriteLine($"NoteAnimationName={MedicNoteName.Value}");
                    w.WriteLine($"SittingAnimationDictionary={MedicSitDict.Value}");
                    w.WriteLine($"SittingAnimationName={MedicSitName.Value}");
                    w.WriteLine($"StretcherCarryingAnimationDictionary={MedicStretcherCarryDict.Value}");
                    w.WriteLine($"StretcherCarryingAnimationName={MedicStretcherCarryName.Value}");
                    w.WriteLine($"InteractionAnimationDictionary={InteractDict.Value}");
                    w.WriteLine($"InteractionAnimationName={InteractName.Value}\n");

                    w.WriteLine("[Patient]");
                    w.WriteLine($"UnconsciousAnimationDictionary={PatientUnconDict.Value}");
                    w.WriteLine($"UnconsciousAnimationName={PatientUnconName.Value}");
                    w.WriteLine($"HunchedAnimationDictionary={PatientHunchedDict.Value}");
                    w.WriteLine($"HunchedAnimationName={PatientHunchedName.Value}");
                    w.WriteLine($"StandingPainAnimationDictionary={PatientStandingDict.Value}");
                    w.WriteLine($"StandingPainAnimationName={PatientStandingName.Value}");
                    w.WriteLine($"OnStretcherAnimationDictionary={PatientStretcherDict.Value}");
                    w.WriteLine($"OnStretcherAnimationName={PatientStretcherName.Value}");
                    w.WriteLine($"SittingOnStretcherAnimationDictionary={PatientSittingStretcherDict.Value}");
                    w.WriteLine($"SittingOnStretcherAnimationName={PatientSittingStretcherName.Value}");
                    w.WriteLine($"ReviveAnimationDictionary={PatientReviveDict.Value}");
                    w.WriteLine($"ReviveAnimationName={PatientReviveName.Value}\n");

                    w.WriteLine("[Bystander]");
                    w.WriteLine($"BystanderWaveAnimationDictionary={BystanderWaveDict.Value}");
                    w.WriteLine($"BystanderWaveAnimationName={BystanderWaveName.Value}");
                }
            }
            catch { }
        }
    }
}