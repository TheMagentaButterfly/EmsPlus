using EmsPlus.Managers;
using Rage;
using System.Collections.Generic;

namespace EmsPlus.Medical
{
    // --- ENUMS ---
    public enum ConsciousnessLevel { Alert, Verbal, Pain, Unresponsive }
    public enum VitalState { None, Normal, Low, CriticalLow, Elevated, CriticalHigh, AirwaySwelling, }
    public enum BloodLevel { Normal, Low, Critical }
    public enum PatientSpawnState { Unconscious, PainedHunched, PainedStanding }
    public enum BloodGlucoseLevel { Normal, Low, CriticalLow, High, CriticalHigh }
    public enum OxygenSaturationLevel { Normal, Low, CriticalLow }

    // --- DATA MODELS ---
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