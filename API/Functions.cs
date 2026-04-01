using System;
using EmsPlus.Core;
using EmsPlus.Managers;

namespace EmsPlus.API
{
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