using Rage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace EmsPlus.Configuration
{
    public class MedicationConfig
    {
        private const string FilePath = "Plugins/EmsPlus/Settings/Medications.xml";
        public List<MedicationDefinition> Definitions { get; private set; } = new List<MedicationDefinition>();

        public void Load()
        {
            Definitions.Clear();
            if (!File.Exists(FilePath)) CreateDefaultFile();

            try
            {
                XDocument doc = XDocument.Load(FilePath);
                foreach (XElement el in doc.Descendants("Medication"))
                {
                    string catAttr = el.Attribute("category")?.Value ?? "MEDS";
                    List<string> categories = catAttr.Split(',')
                                                     .Select(c => c.Trim().ToUpper())
                                                     .ToList();

                    string bonesAttr = el.Attribute("allowedBones")?.Value ?? "LeftForeArm,RightForearm";
                    List<PedBoneId> allowedBones = new List<PedBoneId>();
                    foreach (string boneName in bonesAttr.Split(','))
                    {
                        if (Enum.TryParse(boneName.Trim(), true, out PedBoneId boneId))
                        {
                            allowedBones.Add(boneId);
                        }
                    }

                    string tagAttr = el.Attribute("tags")?.Value ?? "";
                    List<string> tags = tagAttr.Split(',').Select(t => t.Trim().ToUpper()).ToList();

                    var def = new MedicationDefinition
                    {
                        Name = el.Attribute("name")?.Value ?? "Unknown",
                        RequiredKit = el.Attribute("requiredKit")?.Value?.ToUpper() ?? "NONE",
                        Categories = categories,
                        Tags = tags,
                        Description = el.Attribute("description")?.Value ?? "",
                        DurationSeconds = ParseInt(el.Attribute("duration")?.Value, 60),
                        AllowedBones = allowedBones,
                    };

                    Definitions.Add(def);
                }
                Game.Console.Print($"[EmsPlus] Loaded {Definitions.Count} medications from XML.");
            }
            catch (System.Exception ex)
            {
                Game.Console.Print($"[EmsPlus] Error loading Medications.xml: {ex.Message}");
            }
        }

        public MedicationDefinition GetByName(string name)
        {
            return Definitions.Find(d => d.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        private int ParseInt(string val, int defaultVal)
        {
            if (int.TryParse(val, out int result)) return result;
            return defaultVal;
        }

        private void CreateDefaultFile()
        {
            try
            {
                // Define the default XML structure
                var doc = new XDocument(new XElement("Medications",
                    new XComment(" CATEGORIES: AIRWAY, ORAL, IV, IM"),
                    new XComment(" BONES: Head, Neck, Chest, Stomach, LeftUpperArm, RightUpperArm, LeftForeArm, RightForearm, LeftHand, RightHand, LeftThigh, RightThigh, LeftCalf, RightCalf, LeftFoot, RightFoot "),

                    new XComment(" SHARED SECTION "),
                    // EPINEPHRINE
                    new XElement("Medication",
                        new XAttribute("name", "Epinephrine"),
                        new XAttribute("category", "IV, IM"),
                        new XAttribute("tags", "CURE_ALLERGY, ADRENALINE"),
                        new XAttribute("description", "For Anaphylaxis/Cardiac Arrest."),
                        new XAttribute("duration", "120"),
                        new XAttribute("allowedBones", "LeftUpperArm,RightUpperArm,LeftForeArm,RightForearm,LeftHand,RightHand,LeftThigh,RightThigh")
                    ),
                    // NALOXONE / NARCAN
                    new XElement("Medication",
                        new XAttribute("name", "Naloxone"),
                        new XAttribute("category", "ORAL, IV, IM"),
                        new XAttribute("tags", "CURE_OPIOID"),
                        new XAttribute("description", "Opioid Antagonist."),
                        new XAttribute("duration", "60"),
                        new XAttribute("allowedBones", "Head,LeftUpperArm,RightUpperArm,LeftForeArm,RightForearm,LeftHand,RightHand,LeftThigh,RightThigh")
                    ),
                    // MORPHINE
                    new XElement("Medication",
                        new XAttribute("name", "Morphine"),
                        new XAttribute("category", "IV, IM"),
                        new XAttribute("tags", "PAIN_HIGH"),
                        new XAttribute("description", "Strong analgesic."),
                        new XAttribute("duration", "300"),
                        new XAttribute("allowedBones", "LeftUpperArm,RightUpperArm,LeftForeArm,RightForearm,LeftHand,RightHand")
                    ),
                    // FENTAYL
                    new XElement("Medication",
                        new XAttribute("name", "Fentanyl"),
                        new XAttribute("category", "IV, IM"),
                        new XAttribute("tags", "PAIN_HIGH"),
                        new XAttribute("description", "High potency analgesic."),
                        new XAttribute("duration", "300"),
                        new XAttribute("allowedBones", "LeftUpperArm,RightUpperArm,LeftForeArm,RightForearm,LeftHand,RightHand")
                    ),
                    // KETOROLAC
                    new XElement("Medication",
                        new XAttribute("name", "Ketorolac"),
                        new XAttribute("category", "IV, IM"),
                        new XAttribute("tags", "PAIN_MID"),
                        new XAttribute("description", "Moderate pain relief (NSAID)."),
                        new XAttribute("duration", "300"),
                        new XAttribute("allowedBones", "LeftUpperArm,RightUpperArm,LeftForeArm,RightForearm,LeftHand,RightHand")
                    ),

                    new XComment(" AIRWAY SECTION "),
                    // OXYGEN
                    new XElement("Medication",
                        new XAttribute("name", "Oxygen"),
                        new XAttribute("tags", "OXYGEN"),
                        new XAttribute("category", "AIRWAY"),
                        new XAttribute("description", "Increases SpO2 levels."),
                        new XAttribute("duration", "60"),
                        new XAttribute("allowedBones", "Head,Neck")
                    ),

                    new XComment(" IV SECTION "),
                    // DEXTROSE / D50 (IV Only)
                    new XElement("Medication",
                        new XAttribute("name", "Dextrose"),
                        new XAttribute("category", "IV"),
                        new XAttribute("tags", "CURE_HYPO, SUGAR"),
                        new XAttribute("description", "Treats Hypoglycemia."),
                        new XAttribute("duration", "30"),
                        new XAttribute("allowedBones", "LeftForeArm,RightForearm,LeftHand,RightHand")
                    ),

                    new XComment(" INTRAMUSCULAR (IM) SECTION "),

                    new XComment(" ORAL SECTION "),
                    // NITROGLYCERIN
                    new XElement("Medication",
                        new XAttribute("name", "Nitroglycerin"),
                        new XAttribute("category", "ORAL"),
                        new XAttribute("tags", ""),
                        new XAttribute("description", "Vasodilator for Chest Pain."),
                        new XAttribute("duration", "120"),
                        new XAttribute("allowedBones", "Head")
                    ),
                    // ASPIRIN
                    new XElement("Medication",
                        new XAttribute("name", "Aspirin"),
                        new XAttribute("category", "ORAL"),
                        new XAttribute("tags", ""),
                        new XAttribute("description", "Antiplatelet for Cardiac events."),
                        new XAttribute("duration", "60"),
                        new XAttribute("allowedBones", "Head")
                    ),
                    // ALBUTEROL
                    new XElement("Medication",
                        new XAttribute("name", "Albuterol"),
                        new XAttribute("category", "ORAL"),
                        new XAttribute("tags", ""),
                        new XAttribute("description", "Bronchodilator for Asthma."),
                        new XAttribute("duration", "60"),
                        new XAttribute("allowedBones", "Head")
                    ),
                    // ACETAMINOPHEN
                    new XElement("Medication",
                        new XAttribute("name", "Acetaminophen"),
                        new XAttribute("category", "ORAL"),
                        new XAttribute("tags", "PAIN_LOW"),
                        new XAttribute("description", "Mild pain / Fever."),
                        new XAttribute("duration", "300"),
                        new XAttribute("allowedBones", "Head")
                    )
                ));
                doc.Save(FilePath);
                Game.Console.Print("[EmsPlus] Created default Medications.xml.");
            }
            catch { }
        }
    }

    public class MedicationDefinition
    {
        public string Name { get; set; }
        public string RequiredKit { get; set; }
        public List<string> Categories { get; set; } = new List<string>();
        public List<string> Tags { get; set; } = new List<string>();
        public string Description { get; set; }
        public int DurationSeconds { get; set; }
        public List<PedBoneId> AllowedBones { get; set; } = new List<PedBoneId>();
    }
}