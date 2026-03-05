namespace EmsPlus.Framework
{
    public enum EmsStatus
    {
        OffDuty,
        Available,      // Can receive calls
        EnRoute,        // Driving to call
        OnScene,        // At the call
        Transporting,   // Driving to hospital
        Busy            // Generic busy state (paperwork, restocking)
    }
}