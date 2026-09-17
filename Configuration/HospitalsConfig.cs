using Rage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace EmsPlus.Configuration
{
    public class HospitalsConfig
    {
        public class HospitalLocation
        {
            public Vector3 Position { get; set; }
            public string Name { get; set; }
            public bool IsHelipad { get; set; }
        }

        private const string FilePath = "Plugins/EmsPlus/Settings/Data/Hospitals.xml";

        public List<HospitalLocation> Locations { get; private set; } = new List<HospitalLocation>();

        public void Load()
        {
            Locations.Clear();

            if (!File.Exists(FilePath))
            {
                CreateDefaultFile();
            }

            try
            {
                XDocument doc = XDocument.Load(FilePath);
                int count = 0;

                foreach (XElement el in doc.Descendants("Location"))
                {
                    if (el.Attribute("x") != null && el.Attribute("y") != null && el.Attribute("z") != null)
                    {
                        float x = float.Parse(el.Attribute("x").Value);
                        float y = float.Parse(el.Attribute("y").Value);
                        float z = float.Parse(el.Attribute("z").Value);
                        string name = el.Attribute("name")?.Value ?? "Hospital";

                        bool isHelipad = el.Attribute("isHelipad") != null && el.Attribute("isHelipad").Value.ToLower() == "true";

                        Locations.Add(new HospitalLocation { Position = new Vector3(x, y, z), Name = name, IsHelipad = isHelipad });
                        count++;
                    }
                }

                Game.Console.Print($"[EmsPlus] Loaded {count} hospital locations from XML.");
            }
            catch (System.Exception ex)
            {
                Game.Console.Print($"[EmsPlus] Error loading Hospitals.xml: {ex.Message}");
            }
        }

        private void CreateDefaultFile()
        {
            XDocument defaultDoc = new XDocument(
                new XElement("Hospitals",
                    new XComment(" Customize your drop-off points here. Delete lines to remove default hospitals."),
                    new XComment(" Use isHelipad=\"true\" to change the map icon from a Hospital to a Helipad."),

                    // Default GTA V Hospitals (Updated with isHelipad attribute)
                    new XElement("Location", new XAttribute("x", "351.949"), new XAttribute("y", "-588.247"), new XAttribute("z", "74.166"), new XAttribute("name", "Pillbox Hospital Helipad"), new XAttribute("isHelipad", "true")),
                    new XElement("Location", new XAttribute("x", "291.018"), new XAttribute("y", "-587.7867"), new XAttribute("z", "43.1925"), new XAttribute("name", "Pillbox Hospital Upper")),
                    new XElement("Location", new XAttribute("x", "364.45"), new XAttribute("y", "-591.39"), new XAttribute("z", "28.69"), new XAttribute("name", "Pillbox Hospital Lower")),
                    new XElement("Location", new XAttribute("x", "-454.7999"), new XAttribute("y", "-341.0583"), new XAttribute("z", "34.36345"), new XAttribute("name", "Mount Zonah Bay 1")),
                    new XElement("Location", new XAttribute("x", "-491.831"), new XAttribute("y", "-336.4246"), new XAttribute("z", "34.37281"), new XAttribute("name", "Mount Zonah Bay 2")),
                    new XElement("Location", new XAttribute("x", "293.641"), new XAttribute("y", "-1438.732"), new XAttribute("z", "29.804"), new XAttribute("name", "Center LS Mecial Center Bay")),
                    new XElement("Location", new XAttribute("x", "299.316"), new XAttribute("y", "-1453.558"), new XAttribute("z", "46.509"), new XAttribute("name", "Center LS Mecial Center Helipad 1"), new XAttribute("isHelipad", "true")),
                    new XElement("Location", new XAttribute("x", "313.383"), new XAttribute("y", "-1465.261"), new XAttribute("z", "46.509"), new XAttribute("name", "Center LS Mecial Center Helipad 2"), new XAttribute("isHelipad", "true")),
                    new XElement("Location", new XAttribute("x", "1827.337"), new XAttribute("y", "3693.38"), new XAttribute("z", "34.22425"), new XAttribute("name", "Sandy Shores Hospital")),
                    new XElement("Location", new XAttribute("x", "-240.504"), new XAttribute("y", "6334.762"), new XAttribute("z", "32.426"), new XAttribute("name", "Paleto Bay Medical Center"))
                )
            );
            defaultDoc.Save(FilePath);
            Game.Console.Print("[EmsPlus] Created default Hospitals.xml.");
        }
    }
}