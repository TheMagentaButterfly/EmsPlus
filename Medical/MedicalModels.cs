using EmsPlus.Managers;
using Rage;
using System.Collections.Generic;

namespace EmsPlus.Medical
{
    // --- ENUMS ---
    public enum ConsciousnessLevel { Alert, Verbal, Pain, Unresponsive }
    public enum VitalState { None, Normal, Low, CriticalLow, Elevated, CriticalHigh, AirwaySwelling }
    public enum BloodLevel { Normal, Low, Critical }
    public enum PatientSpawnState { Unconscious, PainedHunched, PainedStanding }
    public enum BloodGlucoseLevel { Normal, Low, CriticalLow, High, CriticalHigh }
    public enum OxygenSaturationLevel { Normal, Low, CriticalLow }

    // --- DATA MODELS ---

    public class PatientHistory
    {
        public string Symptoms { get; set; }
        public string Allergies { get; set; }
        public string Medications { get; set; }
        public string PastHistory { get; set; }
        public string LastIntake { get; set; }
        public string Events { get; set; }

        public string GetLimitedHistoryString()
        {
            return $"{Localization.Get("SAMPLERS_SS")}{Symptoms}\n" +
                   $"{Localization.Get("SAMPLERS_ALLERGIES")}{Allergies}\n" +
                   $"{Localization.Get("SAMPLERS_MEDS")}{Medications}\n" +
                   $"{Localization.Get("SAMPLERS_PAST")}{PastHistory}\n" +
                   $"{Localization.Get("SAMPLERS_INTAKE")}{LastIntake}\n" +
                   $"{Localization.Get("SAMPLERS_EVENTS")}{Events}";
        }
    }

    public class PatientDetails
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Age { get; set; }
        public string DateOfBirth { get; set; }
        public int Height { get; set; } // cm
        public int Weight { get; set; } // kg
        public string Gender { get; set; }
        public string BloodType { get; set; }
        public string PrimaryAllergy { get; set; }

        public string FullName => $"{FirstName} {LastName}";

        public void GenerateRandom(bool isMale)
        {
            var maleNames = new[] { "James", "John", "Robert", "Michael", "William", "David", "Richard", "Joseph", "Thomas", "Charles" };
            var femaleNames = new[] { "Mary", "Patricia", "Jennifer", "Linda", "Elizabeth", "Barbara", "Susan", "Jessica", "Sarah", "Karen" };
            var lastNames = new[] { "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis", "Rodriguez", "Martinez" };
            var bloodTypes = new[] { "O+", "O-", "A+", "A-", "B+", "B-", "AB+", "AB-" };
            var allergies = new[] { "Penicillin", "Latex", "Peanuts", "Aspirin", "Shellfish", "Sulfa drugs", "Ibuprofen" };

            var rnd = new System.Random();
            FirstName = isMale ? maleNames[rnd.Next(maleNames.Length)] : femaleNames[rnd.Next(femaleNames.Length)];
            LastName = lastNames[rnd.Next(lastNames.Length)];
            Gender = isMale ? "Male" : "Female";

            Age = rnd.Next(18, 85);
            int birthYear = 2026 - Age;
            int birthMonth = rnd.Next(1, 13);
            int birthDay = rnd.Next(1, 29);
            DateOfBirth = $"{birthMonth:D2}/{birthDay:D2}/{birthYear}";

            Height = rnd.Next(155, 195);
            Weight = rnd.Next(55, 110);
            BloodType = bloodTypes[rnd.Next(bloodTypes.Length)];
            PrimaryAllergy = allergies[rnd.Next(allergies.Length)];
        }
    }

    public class Bystander
    {
        public Ped Character { get; }
        public Bystander(Ped character) { Character = character; }
        public bool HasBeenSpokenTo { get; set; } = false;
        public List<DialogueLine> Dialogue { get; set; } = new List<DialogueLine>();
    }

    public class BodyPart
    {
        public string Name { get; set; }
        public PedBoneId BoneId { get; set; }
        public Entity LinkedEntity { get; set; }

        // UI State
        public Vector2 ScreenPosition { get; set; }
        public bool IsHovered { get; set; }

        public BodyPart(string name, PedBoneId bone)
        {
            Name = name; BoneId = bone; LinkedEntity = null;
        }

        public BodyPart(string name, Entity entity)
        {
            Name = name; LinkedEntity = entity; BoneId = PedBoneId.Pelvis;
        }
    }
}