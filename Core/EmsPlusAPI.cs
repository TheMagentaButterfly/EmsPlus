using EmsPlus.Managers;
using System;

namespace EmsPlus.Core
{
    /// <summary>
    /// The public API for developers to create custom callouts and scenarios for EmsPlus.
    /// </summary>
    public static class Functions
    {
        /// <summary>
        /// Registers a custom callout with the EmsPlus Dispatcher.
        /// Ensure your class inherits from 'EmsCallout'.
        /// </summary>
        public static void RegisterCallout(Type calloutType)
        {
            CalloutManager.RegisterCallout(calloutType);
        }

        /// <summary>
        /// Registers a custom medical scenario that will appear in the random scenario generator.
        /// Ensure your class inherits from 'MedicalScenario'.
        /// </summary>
    }
}