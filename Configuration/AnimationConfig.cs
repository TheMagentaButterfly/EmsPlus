using IPT.Common.User.Settings;
using System.IO;

namespace EmsPlus.Configuration
{
    public class AnimationConfig : IPT.Common.User.Configuration
    {
        private const string IniFilePath = "Plugins/EmsPlus/Settings/Animations.ini";

        // --- MEDIC ACTIONS ---
        public SettingString MedicAssessDict = new SettingString("Medic", "AssessmentAnimationDictionary", "CPR/Vitals animation dictionary", "amb@medic@standing@kneel@idle_a");
        public SettingString MedicAssessName = new SettingString("Medic", "AssessmentAnimationName", "CPR/Vitals animation name", "idle_a");

        public SettingString MedicTreatDict = new SettingString("Medic", "TreatmentAnimationDictionary", "Treatments animation dictionary", "amb@medic@standing@tendtodead@idle_a");
        public SettingString MedicTreatName = new SettingString("Medic", "TreatmentAnimationName", "Treatments animation name", "idle_a");

        public SettingString MedicNoteDict = new SettingString("Medic", "NoteAnimationDictionary", "Writing notes/checking history dictionary", "amb@medic@standing@timeofdeath@base");
        public SettingString MedicNoteName = new SettingString("Medic", "NoteAnimationName", "Writing notes/checking history name", "base");

        public SettingString InteractDict = new SettingString("Medic", "InteractionAnimationDictionary", "Interaction animation dictionary", "anim@narcotics@trash");
        public SettingString InteractName = new SettingString("Medic", "InteractionAnimationName", "Interaction animation name", "drop_front");

        // --- PATIENT STATES ---
        public SettingString PatientUnconDict = new SettingString("Patient", "UnconsciousAnimationDictionary", "Patient unconscious/dead on ground", "misslamar1dead_body");
        public SettingString PatientUnconName = new SettingString("Patient", "UnconsciousAnimationName", "Patient unconscious/dead on ground", "dead_idle");
        public SettingString PatientHunchedDict = new SettingString("Patient", "HunchedAnimationDictionary", "Patient in severe pain on the ground", "misschinese2_crystalmaze");
        public SettingString PatientHunchedName = new SettingString("Patient", "HunchedAnimationName", "Patient in severe pain on the ground", "2int_loop_a_taocheng");
        public SettingString PatientStandingDict = new SettingString("Patient", "StandingPainAnimationDictionary", "Patient standing in moderate pain", "rcmfanatic1out_of_breath");
        public SettingString PatientStandingName = new SettingString("Patient", "StandingPainAnimationName", "Patient standing in moderate pain", "p_zero_tired_01e");

        public SettingString PatientStretcherDict = new SettingString("Patient", "OnStretcherAnimationDictionary", "Patient laying on stretcher", "amb@world_human_sunbathe@female@back@base");
        public SettingString PatientStretcherName = new SettingString("Patient", "OnStretcherAnimationName", "Patient laying on stretcher", "base");
        public SettingString PatientSittingStretcherDict = new SettingString("Patient", "SittingOnStretcherAnimationDictionary", "Patient sitting on stretcher", "anim@amb@business@bgen@bgen_no_work@");
        public SettingString PatientSittingStretcherName = new SettingString("Patient", "SittingOnStretcherAnimationName", "Patient sitting on stretcher", "sit_phone_phoneputdown_idle_nowork");

        public SettingString PatientReviveDict = new SettingString("Patient", "ReviveAnimationDictionary", "Patient waking up/getting up", "amb@world_human_sunbathe@female@back@base");
        public SettingString PatientReviveName = new SettingString("Patient", "ReviveAnimationName", "Patient waking up/getting up", "base");

        // --- BYSTANDER SATES ---
        public SettingString BystanderWaveDict = new SettingString("Bystander", "BystanderWaveAnimationDictionary", "Bystander waving you down", "anim@amb@waving@male");
        public SettingString BystanderWaveName = new SettingString("Bystander", "BystanderWaveAnimationName", "Bystander waving you down", "ground_wave");

        public override void Load()
        {
            if (!File.Exists(IniFilePath)) CreateDefault();
            LoadINI(IniFilePath);
        }

        public void Save()
        {
            SaveINI(IniFilePath);
        }

        private void CreateDefault()
        {
            try
            {
                using (StreamWriter w = new StreamWriter(IniFilePath))
                {
                    w.WriteLine("; ==================================================");
                    w.WriteLine("; EmsPlus Animation Configuration");
                    w.WriteLine("; Use this file to customize animations.");
                    w.WriteLine("; ==================================================");
                    w.WriteLine("; I suggest this website to find the animation you want: https://forge.plebmasters.de/animations");
                    w.WriteLine("");
                    w.WriteLine("");


                    w.WriteLine("[Medic]");
                    w.WriteLine("; CPR/Vitals animation");
                    w.WriteLine($"AssessmentAnimationDictionary={MedicAssessDict.Value}");
                    w.WriteLine($"AssessmentAnimationName={MedicAssessName.Value}");
                    w.WriteLine("");

                    w.WriteLine("; Treatments animation");
                    w.WriteLine($"AssessmentAnimationDictionary={MedicTreatDict.Value}");
                    w.WriteLine($"AssessmentAnimationName={MedicTreatName.Value}");
                    w.WriteLine("");

                    w.WriteLine("; Writing notes animation");
                    w.WriteLine($"NoteAnimationDictionary={MedicNoteDict.Value}");
                    w.WriteLine($"NoteAnimationName={MedicNoteName.Value}");
                    w.WriteLine("");

                    w.WriteLine("; General Interaction animation");
                    w.WriteLine($"InteractionAnimationDictionary={InteractDict.Value}");
                    w.WriteLine($"InteractionAnimationName={InteractName.Value}");
                    w.WriteLine("");
                    w.WriteLine("");


                    w.WriteLine("[Patient]");
                    w.WriteLine("; Unconscious on ground");
                    w.WriteLine($"UnconsciousAnimationDictionary={PatientUnconDict.Value}");
                    w.WriteLine($"UnconsciousAnimationName={PatientUnconName.Value}");
                    w.WriteLine("");

                    w.WriteLine("; Hunched over or writhing on ground in pain");
                    w.WriteLine($"HunchedAnimationDictionary={PatientHunchedDict.Value}");
                    w.WriteLine($"HunchedAnimationName={PatientHunchedName.Value}");
                    w.WriteLine("");

                    w.WriteLine("; Standing but in pain (e.g. holding stomach)");
                    w.WriteLine($"StandingPainAnimationDictionary={PatientStandingDict.Value}");
                    w.WriteLine($"StandingPainAnimationName={PatientStandingName.Value}");
                    w.WriteLine("");

                    w.WriteLine("; Laying on Stretcher");
                    w.WriteLine($"OnStretcherAnimationDictionary={PatientStretcherDict.Value}");
                    w.WriteLine($"OnStretcherAnimationName={PatientStretcherName.Value}");
                    w.WriteLine("");

                    w.WriteLine("; Sitting on Stretcher");
                    w.WriteLine($"SittingOnStretcherAnimationDictionary={PatientSittingStretcherDict.Value}");
                    w.WriteLine($"SittingOnStretcherAnimationName={PatientSittingStretcherName.Value}");
                    w.WriteLine("");

                    w.WriteLine("; Revive / Get Up");
                    w.WriteLine($"ReviveAnimationDictionary={PatientReviveDict.Value}");
                    w.WriteLine($"ReviveAnimationName={PatientReviveName.Value}");
                    w.WriteLine("");
                    w.WriteLine("");

                    w.WriteLine("[Bystander]");
                    w.WriteLine($"BystanderWaveAnimationDictionary={BystanderWaveDict.Value}");
                    w.WriteLine($"BystanderWaveAnimationName={BystanderWaveName.Value}");
                }
            }
            catch { }
        }
    }
}