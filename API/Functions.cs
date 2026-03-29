using System;
using EmsPlus.Core;
using EmsPlus.Managers;

namespace EmsPlus.API
{
    /// <summary>
    /// The official EmsPlus API for third-party developers.
    /// </summary>
    public static class Functions
    {
        public delegate void DutyStateChangedEventHandler(bool onDuty);
        public static event DutyStateChangedEventHandler OnDutyStateChanged;

        internal static void InvokeDutyStateChanged(bool onDuty)
        {
            OnDutyStateChanged?.Invoke(onDuty);
        }

        public static void RegisterCallout(Type calloutType)
        {
            CalloutManager.RegisterCallout(calloutType);
        }

        public static bool IsOnDuty() => EmsService.IsOnDuty;
    }
}