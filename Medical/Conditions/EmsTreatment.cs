namespace EmsPlus.Medical.Conditions
{
    public enum EmsTreatment
    {
        None = 0,
        // Basic Trauma
        Bandage,
        Suture,
        Tourniquet,
        DirectPressure,
        WoundPacking,
        JunctionalTourniquet,
        IcePack,
        StabiliseObject,

        // Advanced Trauma & Fluids
        IVAccess,
        IVFluids,
        Analgesia,
        Adrenaline,

        // Respiratory & Chest
        Oxygen,
        HighFlowOxygen,
        BagValveMask,
        ChestSeal,
        NeedleDecomp,
        AirwayManagement,

        // Immobilization
        Splint,
        TractionSplint,
        PelvicBinder,
        SpinalImmobilisation,
        CervicalCollar,

        // Specialized & Environmental
        WetDressing,
        BurnDressing,
        Irrigation,
        EyePatch,
        EyeShield,
        ActiveRewarming,
        ActiveCooling,
        RecoveryPosition,
        Monitoring,

        // Critical Care
        CPR,
        GiveNaloxone,
        GiveGlucose,

        // Medicine
        GiveEpinephrine
    }
}