using EmsPlus.Core;
using RAGENativeUI.Elements;

namespace EmsPlus.UI.Native.PatientMenu
{
    public static partial class PatientMenuBuilder
    {
        private static void BuildPatientDataMenu()
        {
            PatientDataMenu.Clear();
            var p = GameState.CurrentPatient;
            if (p == null) return;

            AddReadonlyItem(PatientDataMenu, $"~b~{Localization.Get("DATA_NAME", "Full Name")}", p.Details.FullName);
            AddReadonlyItem(PatientDataMenu, $"~b~{Localization.Get("DATA_DOB", "DOB & Age")}", $"{p.Details.DateOfBirth} ({p.Details.Age}y)");
            AddReadonlyItem(PatientDataMenu, $"~b~{Localization.Get("DATA_GENDER", "Gender")}", p.Details.Gender);
            AddReadonlyItem(PatientDataMenu, $"~b~{Localization.Get("DATA_HEIGHT", "Height")}", $"{p.Details.Height} cm");
            AddReadonlyItem(PatientDataMenu, $"~b~{Localization.Get("DATA_WEIGHT", "Weight")}", $"{p.Details.Weight} kg");
            AddMenuSeparator(PatientDataMenu, Localization.Get("CAT_SEP_MEDICAL_INFO", "~c~=== ~r~MEDICAL INFO ~c~==="));
            AddReadonlyItem(PatientDataMenu, $"~r~{Localization.Get("DATA_BLOOD", "Blood Type")}", p.Details.BloodType);
            AddReadonlyItem(PatientDataMenu, $"~y~{Localization.Get("DATA_ALLERGY", "Known Allergies")}", p.Details.PrimaryAllergy);

            PatientDataMenu.RefreshIndex();
        }
    }
}