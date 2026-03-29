namespace EmsPlus.Medical
{
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
}