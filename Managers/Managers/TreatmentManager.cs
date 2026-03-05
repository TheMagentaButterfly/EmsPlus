using Rage;
using System;
using System.Collections.Generic;

namespace EmsPlus.Managers
{
    public class CustomAction
    {
        public string ID { get; set; }           // e.g., "APPLY_TOURNIQUET"
        public string Label { get; set; }        // e.g., "Apply Tourniquet"
        public string SubLabel { get; set; }     // e.g., "Stop arterial bleeding"
        public string RequiredKitID { get; set; } // e.g., "TRAUMABAG"
        public Action<Patient, PedBoneId> OnExecute { get; set; }
    }

    public static class TreatmentRegistry
    {
        private static Dictionary<string, CustomAction> _registry = new Dictionary<string, CustomAction>();

        public static void Register(CustomAction action)
        {
            if (!_registry.ContainsKey(action.ID))
                _registry.Add(action.ID, action);
        }

        public static CustomAction Get(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            return _registry.TryGetValue(id, out var action) ? action : null;
        }
    }
}