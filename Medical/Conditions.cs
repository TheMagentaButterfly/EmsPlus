using Rage;
using System.Collections.Generic;

namespace EmsPlus.Medical
{
    // --- BASE COMPONENT ---
    public abstract class MedicalCondition
    {
        public string Name { get; protected set; }
        public bool IsTreated { get; set; } = false;
        public List<EmsTreatment> RequiredTreatments { get; protected set; } = new List<EmsTreatment>();

        public virtual void Update(Patient patient) { }

        public virtual void OnTreated(Patient patient, EmsTreatment treatment)
        {
            if (RequiredTreatments.Contains(treatment)) RequiredTreatments.Remove(treatment);
            if (RequiredTreatments.Count == 0) IsTreated = true;
        }

        public virtual void ReactToTreatment(Patient patient, EmsTreatment treatment) { }
    }

    // --- STANDARD INJURIES / SYSTEMIC ISSUES ---
    public class PhysicalInjury : MedicalCondition
    {
        public PedBoneId Bone { get; }
        public float BleedSeverity { get; set; }

        public PhysicalInjury(string name, PedBoneId bone, float bleedSeverity, params EmsTreatment[] treatments)
        {
            Name = name; Bone = bone; BleedSeverity = bleedSeverity; RequiredTreatments.AddRange(treatments);
        }

        public override void Update(Patient patient)
        {
            if (IsTreated) return;
            if (BleedSeverity > 1.5f && patient.BloodPressure > VitalState.CriticalLow)
                patient.BloodPressure = VitalState.Low;

            if (Name == "Tension Pneumothorax") { patient.SpO2 = VitalState.CriticalLow; patient.BloodPressure = VitalState.Low; }
        }

        public override void OnTreated(Patient patient, EmsTreatment treatment)
        {
            if (treatment == EmsTreatment.Tourniquet) BleedSeverity = 0.2f;
            base.OnTreated(patient, treatment);
            if (IsTreated) BleedSeverity = 0f;
        }
    }

    public class SystemicCondition : MedicalCondition
    {
        public SystemicCondition(string name, params EmsTreatment[] treatments)
        {
            Name = name;
            RequiredTreatments.AddRange(treatments);
        }
    }
}