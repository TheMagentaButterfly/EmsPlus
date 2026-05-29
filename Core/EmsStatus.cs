namespace EmsPlus.Core
{
    public enum EmsStatus
    {
        UrgentRequestToSpeak,   // Urgent request for information
        Available,              // Can receive calls (away from station)
        AvailableAtStation,     // Available but at the station
        EnRoute,                // Driving to call
        OnScene,                // On Scene of the call
        RequestToSpeak,         // Request to speak (to dispatch)
        OffDuty,                // Not available for calls
        Transporting,           // Driving to hospital
        AtDestination,          // At the hospital
        Busy,                   // Generic busy state (paperwork, restocking)
        Emergency,              // In an emergency situation (e.g., vehicle breakdown)
    }
}