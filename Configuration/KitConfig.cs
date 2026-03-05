using Rage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace EmsPlus.Configuration
{
    public class KitDefinition
    {
        public string ID { get; set; } // e.g. "TRAUMABAG"
        public string Name { get; set; } // e.g. "Trauma Bag"
        public string Description { get; set; } // e.g. "Contains Drugs, IVs, and advanced diagnostic tools."
        public string Model { get; set; } // e.g. "xm_prop_x17_bag_med_01a"
    }

    public class KitConfig
    {
        private const string FilePath = "Plugins/EmsPlus/Settings/Kits.xml";
        public List<KitDefinition> Definitions { get; private set; } = new List<KitDefinition>();

        public void Load()
        {
            Definitions.Clear();
            if (!File.Exists(FilePath)) CreateDefaultFile();

            try
            {
                XDocument doc = XDocument.Load(FilePath);
                foreach (XElement el in doc.Descendants("Kit"))
                {
                    Definitions.Add(new KitDefinition
                    {
                        ID = el.Attribute("id")?.Value.ToUpper() ?? "UNKNOWN",
                        Name = el.Attribute("name")?.Value ?? "Medical Kit",
                        Description = el.Attribute("description")?.Value ?? "No description available.",
                        Model = el.Attribute("model")?.Value ?? "xm_prop_x17_bag_med_01a"
                    });
                }
                Game.Console.Print($"[EmsPlus] Loaded {Definitions.Count} kits from XML.");
            }
            catch (Exception ex) { Game.Console.Print($"[EmsPlus] Error loading Kits.xml: {ex.Message}"); }
        }

        private void CreateDefaultFile()
        {
            var doc = new XDocument(new XElement("Kits",
                new XElement("Kit", new XAttribute("id", "TRAUMABAG"), new XAttribute("name", "~r~Trauma Bag"), new XAttribute("description", "Contains Drugs, IVs, and advanced diagnostic tools."), new XAttribute("model", "xm_prop_x17_bag_med_01a")),
                new XElement("Kit", new XAttribute("id", "OXYGENBAG"), new XAttribute("name", "~b~Oxygen Bag"), new XAttribute("description", "Contains Oxygen tank and masks."), new XAttribute("model", "xm_prop_x17_bag_med_01a")),
                new XElement("Kit", new XAttribute("id", "DEFIBRILLATOR"), new XAttribute("name", "~g~Defibrillator"), new XAttribute("description", "ECG, DEFIB, SpO2, NIBD."), new XAttribute("model", "xm_prop_x17_bag_med_01a"))
                //new XElement("Kit", new XAttribute("id", "LUCAS"), new XAttribute("name", "~y~Lucas Device"), new XAttribute("description", "Automated CPR device."), new XAttribute("model", "xm_prop_x17_bag_med_01a"))
            ));
            doc.Save(FilePath);
        }
    }
}