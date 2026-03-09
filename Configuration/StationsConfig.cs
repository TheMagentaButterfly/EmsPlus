using Rage;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace EmsPlus.Configuration
{
    public class StationLocation
    {
        public Vector3 Position { get; set; }
        public string ID { get; set; }
        public string Name { get; set; }
    }
    public class StationsConfig
    {
        private const string FilePath = "Plugins/EmsPlus/Settings/Data/Stations.xml";

        public List<StationLocation> Locations { get; private set; } = new List<StationLocation>();

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
                        float x = (float)el.Attribute("x");
                        float y = (float)el.Attribute("y");
                        float z = (float)el.Attribute("z");

                        string id = el.Attribute("id")?.Value.ToUpper() ?? "GLOBAL";
                        string name = el.Attribute("name")?.Value ?? "Fire Station";

                        Locations.Add(new StationLocation { Position = new Vector3(x, y, z), ID = id, Name = name });
                        count++;
                    }
                }

                Game.Console.Print($"[EmsPlus] Loaded {count} station locations from XML.");
            }
            catch (System.Exception ex)
            {
                Game.Console.Print($"[EmsPlus] Error loading Stationss.xml: {ex.Message}");
            }
        }

        private void CreateDefaultFile()
        {
            XDocument defaultDoc = new XDocument(
                new XElement("Stations",
                    new XComment(" Customize your Stations here. Delete lines to remove default Stations."),

                    // Default GTA V Stations
                    new XElement("Location", new XAttribute("x", "-633.115"), new XAttribute("y", "-121.875"), new XAttribute("z", "39.014"), new XAttribute("id", "ROCKFORD"), new XAttribute("name", "Rockford Hill Fire Station Entrance 1")),
                    new XElement("Location", new XAttribute("x", "1690.086"), new XAttribute("y", "3581.570"), new XAttribute("z", "35.621"), new XAttribute("id", "SANDY"), new XAttribute("name", "Sandy Shores Fire Station 2")),
                    new XElement("Location", new XAttribute("x", "-379.563"), new XAttribute("y", "6118.247"), new XAttribute("z", "31.848"), new XAttribute("id", "PALETO"), new XAttribute("name", "Paleto Bay Fire Station 3")),
                    new XElement("Location", new XAttribute("x", "-2095.190"), new XAttribute("y", "2829.382"), new XAttribute("z", "32.961"), new XAttribute("id", "FORTZANCUDO"), new XAttribute("name", "Fort Zancudo Fire Station 4")),
                    new XElement("Location", new XAttribute("x", "-1067.965"), new XAttribute("y", "-2378.975"), new XAttribute("z", "14.075"), new XAttribute("id", "LSIA"), new XAttribute("name", "Los Santos International Airport Fire Station 5")),
                    new XElement("Location", new XAttribute("x", "199.215"), new XAttribute("y", "-1634.704"), new XAttribute("z", "29.803"), new XAttribute("id", "DAVIS"), new XAttribute("name", "Davis Fire Station 6")),
                    new XElement("Location", new XAttribute("x", "1185.585"), new XAttribute("y", "-1464.056"), new XAttribute("z", "34.901"), new XAttribute("id", "LSCOUNTY"), new XAttribute("name", "Los Santos County Fire Station 7")),
                    new XElement("Location", new XAttribute("x", "-1183.651"), new XAttribute("y", "-1774.126"), new XAttribute("z", "4.190"), new XAttribute("id", "VESPUCCI"), new XAttribute("name", "Vespucci Beach Fire Station 8"))
                )
            );
            defaultDoc.Save(FilePath);
            Game.Console.Print("[EmsPlus] Created default Stations.xml.");
        }
    }
}