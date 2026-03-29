using Rage;

namespace EmsPlus.Medical
{
    public class PhysicalInjury : MedicalCondition
    {
        public PedBoneId Bone { get; }
        public float BleedSeverity { get; set; }

        public PhysicalInjury(string name, PedBoneId bone, float bleedSeverity, params EmsTreatment[] treatments)
        {
            Name = name;
            Bone = bone;
            BleedSeverity = bleedSeverity;
            RequiredTreatments.AddRange(treatments);
        }

        public override void OnTreated(Patient patient, EmsTreatment treatment)
        {
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