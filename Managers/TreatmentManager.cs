using EmsPlus.Medical;
using Rage;
using System;
using System.Collections.Generic;

namespace EmsPlus.Managers
{
    public class CustomAction
    {
        public string ID { get; set; }
        public string Label { get; set; }
        public string SubLabel { get; set; }
        public string RequiredKitID { get; set; }
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