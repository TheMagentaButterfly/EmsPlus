using EmsPlus.Core;
using System.Collections.Generic;

namespace EmsPlus.UI.Custom.InspectMenu
{
    public class PatientDataManager
    {
        public List<DiagnosticItem> Items { get; private set; } = new List<DiagnosticItem>();

        public PatientDataManager() => Refresh();

        public void Refresh()
        {
            var newItems = new List<DiagnosticItem>();
            var p = GameState.CurrentPatient;
            if (p == null) return;

            newItems.Add(new DiagnosticItem(Localization.Get("DATA_NAME", "Name"), p.Details.FullName, true));
            newItems.Add(new DiagnosticItem(Localization.Get("DATA_DOB", "DOB & Age"), $"{p.Details.DateOfBirth} ({p.Details.Age}y)", true));
            newItems.Add(new DiagnosticItem(Localization.Get("DATA_GENDER", "Gender"), p.Details.Gender, true));
            newItems.Add(new DiagnosticItem(Localization.Get("DATA_HEIGHT", "Height"), $"{p.Details.Height} cm", true));
            newItems.Add(new DiagnosticItem(Localization.Get("DATA_WEIGHT", "Weight"), $"{p.Details.Weight} kg", true));
            newItems.Add(new DiagnosticItem(Localization.Get("DATA_BLOOD", "Blood Type"), p.Details.BloodType, true));
            newItems.Add(new DiagnosticItem(Localization.Get("DATA_ALLERGY", "Known Allergies"), p.Details.PrimaryAllergy, true));

            Items = newItems;
        }
    }
}