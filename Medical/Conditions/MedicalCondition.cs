using EmsPlus.Medical.Conditions;
using System.Collections.Generic;

namespace EmsPlus.Medical.Frameworks
{
    public abstract class MedicalCondition
    {
        public string Name { get; protected set; }
        public bool IsTreated { get; set; } = false;

        public List<EmsTreatment> RequiredTreatments { get; protected set; } = new List<EmsTreatment>();

        public virtual void Update(Patient patient) { }

        public virtual void OnTreated(Patient patient, EmsTreatment treatment)
        {
            if (RequiredTreatments.Contains(treatment))
            {
                RequiredTreatments.Remove(treatment);
            }
            if (RequiredTreatments.Count == 0)
            {
                IsTreated = true;
            }
        }

        public virtual void ReactToTreatment(Patient patient, EmsTreatment treatment) { }
    }
}