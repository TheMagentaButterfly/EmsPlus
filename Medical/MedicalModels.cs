using EmsPlus.Managers;
using Rage;
using System.Collections.Generic;

namespace EmsPlus.Medical
{
    // --- ENUMS ---
    public enum ConsciousnessLevel { Alert, Verbal, Pain, Unresponsive }
    public enum VitalState { None, Normal, Low, CriticalLow, Elevated, CriticalHigh, AirwaySwelling }
    public enum BloodLevel { Normal, Low, Critical }
    public enum PatientSpawnState { Unconscious, PainedHunched, PainedStanding }
    public enum BloodGlucoseLevel { Normal, Low, CriticalLow, High, CriticalHigh }
    public enum OxygenSaturationLevel { Normal, Low, CriticalLow }

    // --- DATA MODELS ---

    public class PatientHistory
    {
        public string Symptoms { get; set; }
        public string Allergies { get; set; }
        public string Medications { get; set; }
        public string PastHistory { get; set; }
        public string LastIntake { get; set; }
        public string Events { get; set; }

        public string GetLimitedHistoryString()
        {
            return $"{Localization.Get("SAMPLERS_SS")}{Symptoms}\n" +
                   $"{Localization.Get("SAMPLERS_ALLERGIES")}{Allergies}\n" +
                   $"{Localization.Get("SAMPLERS_MEDS")}{Medications}\n" +
                   $"{Localization.Get("SAMPLERS_PAST")}{PastHistory}\n" +
                   $"{Localization.Get("SAMPLERS_INTAKE")}{LastIntake}\n" +
                   $"{Localization.Get("SAMPLERS_EVENTS")}{Events}";
        }
    }

    public class Bystander
    {
        public Ped Character { get; }
        public Bystander(Ped character) { Character = character; }
        public bool HasBeenSpokenTo { get; set; } = false;
        public List<DialogueLine> Dialogue { get; set; } = new List<DialogueLine>();
    }

    public class BodyPart
    {
        public string Name { get; set; }
        public PedBoneId BoneId { get; set; }
        public Entity LinkedEntity { get; set; }

        // UI State
        public Vector2 ScreenPosition { get; set; }
        public bool IsHovered { get; set; }

        public BodyPart(string name, PedBoneId bone)
        {
            Name = name; BoneId = bone; LinkedEntity = null;
        }

        public BodyPart(string name, Entity entity)
        {
            Name = name; LinkedEntity = entity; BoneId = PedBoneId.Pelvis;
        }
    }
}