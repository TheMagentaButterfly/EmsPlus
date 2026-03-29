namespace EmsPlus.Medical
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
        SalineBag,

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
        Defibrillation,

        // Medicine
        Adrenaline,
        Naloxone,
        Glucose,
        Analgesia
    }
}