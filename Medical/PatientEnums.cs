namespace EmsPlus.Medical
{
    public enum PatientSpawnState
    {
        Unconscious,
        PainedHunched,
        PainedStanding
    }

    public enum ConsciousnessLevel
    {
        Alert,
        Verbal,
        Pain,
        Unresponsive
    }

    public enum VitalState
    {
        None,
        Normal,
        Low,
        CriticalLow,
        Elevated,
        CriticalHigh,
        AirwaySwelling,
    }

    public enum BloodLevel
    {
        Normal,
        Low,
        Critical
    }

    public enum BloodGlucoseLevel
    {
        Normal,
        Low,
        CriticalLow,
        High,
        CriticalHigh
    }

    public enum OxygenSaturationLevel
    {
        Normal,
        Low,
        CriticalLow
    }
}