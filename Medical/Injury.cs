using Rage;

namespace EmsPlus.Medical
{
    public class Injury
    {
        public string Type { get; set; }
        public PedBoneId Location { get; set; }
        public float Severity { get; set; }
        public float BleedSeverity { get; set; }
        public int PainAmount { get; set; }

        public bool IsTreated { get; set; } = false;
        public string RequiredActionID { get; set; }

        public Injury(string type, PedBoneId location, float severity, float bleedSeverity = 0f, int painAmount = 0, string requiredAction = null)
        {
            Type = type;
            Location = location;
            Severity = severity;
            BleedSeverity = bleedSeverity;
            PainAmount = painAmount;
            RequiredActionID = requiredAction;
        }

        public string GetName()
        {
            string key = $"INJURY_{Type.ToUpper()}";
            string localized = Localization.Get(key);
            if (localized == key) return Type;
            return localized;
        }
    }
}