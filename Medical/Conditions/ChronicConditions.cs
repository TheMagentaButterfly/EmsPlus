using EmsPlus.Medical.Conditions;

namespace EmsPlus.Medical.Frameworks
{
    public class COPD : MedicalCondition
    {
        public override void Update(Patient patient)
        {
            if (!IsTreated && patient.SpO2 > VitalState.Low)
            {
                patient.SpO2 = VitalState.Low;
                patient.Respiration = VitalState.Elevated;
            }
        }

        public override void ReactToTreatment(Patient patient, EmsTreatment treatment)
        {
        }
    }
}