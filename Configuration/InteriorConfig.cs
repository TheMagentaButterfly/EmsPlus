using Rage;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace EmsPlus.Configuration
{
    public class EntranceDefinition
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public Vector3 Coords { get; set; }
    }

    public class InteriorDefinition
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public Vector3 ExitCoords { get; set; }
        public List<Vector3> PatientSpawnPoints { get; set; } = new List<Vector3>();
        public List<string> RequiredIPLs { get; set; } = new List<string>();
        public List<EntranceDefinition> Entrances { get; set; } = new List<EntranceDefinition>();
    }

    public class InteriorConfig
    {
        private const string FilePath = "Plugins/EmsPlus/Settings/Data/Interiors.xml";
        public List<InteriorDefinition> Definitions { get; private set; } = new List<InteriorDefinition>();

        public void Load()
        {
            Definitions.Clear();
            if (!File.Exists(FilePath)) CreateDefaultFile();

            try
            {
                XDocument doc = XDocument.Load(FilePath);
                foreach (XElement el in doc.Descendants("Interior"))
                {
                    var def = new InteriorDefinition
                    {
                        ID = el.Attribute("id")?.Value ?? "UNKNOWN",
                        Name = el.Attribute("name")?.Value ?? "Apartment"
                    };

                    var ext = el.Element("Exit");
                    if (ext != null)
                    {
                        def.ExitCoords = new Vector3((float)ext.Attribute("x"), (float)ext.Attribute("y"), (float)ext.Attribute("z"));
                    }

                    var spawns = el.Element("SpawnPoints");
                    if (spawns != null)
                    {
                        foreach (var pt in spawns.Elements("Point"))
                        {
                            def.PatientSpawnPoints.Add(new Vector3((float)pt.Attribute("x"), (float)pt.Attribute("y"), (float)pt.Attribute("z")));
                        }
                    }

                    var ipls = el.Element("IPLs");
                    if (ipls != null)
                    {
                        foreach (var ipl in ipls.Elements("IPL"))
                        {
                            def.RequiredIPLs.Add(ipl.Attribute("name").Value);
                        }
                    }

                    var entrances = el.Element("Entrances");
                    if (entrances != null)
                    {
                        foreach (var ent in entrances.Elements("Entrance"))
                        {
                            def.Entrances.Add(new EntranceDefinition
                            {
                                ID = ent.Attribute("id")?.Value ?? "DOOR",
                                Name = ent.Attribute("name")?.Value ?? "Door",
                                Coords = new Vector3((float)ent.Attribute("x"), (float)ent.Attribute("y"), (float)ent.Attribute("z")),
                            });
                        }
                    }

                    Definitions.Add(def);
                }
                Game.Console.Print($"[EmsPlus] Loaded {Definitions.Count} interiors from XML.");
            }
            catch (System.Exception ex) { Game.Console.Print($"[EmsPlus] Error loading Interiors.xml: {ex.Message}"); }
        }

        private void CreateDefaultFile()
        {
            var doc = new XDocument(new XElement("Interiors",
                new XComment(" Low-End Motel Rooms "),
                new XElement("Interior", new XAttribute("id", "MOTEL_ROOM"), new XAttribute("name", "Pink Motel"),
                    new XElement("Exit", new XAttribute("x", "265.9"), new XAttribute("y", "-1002.8"), new XAttribute("z", "-99.0")),
                    new XElement("SpawnPoints",
                        new XElement("Point", new XAttribute("x", "262.2"), new XAttribute("y", "-1002.6"), new XAttribute("z", "-99.0"))
                    ),
                    new XElement("IPLs"),
                    new XElement("Entrances",
                        new XElement("Entrance", new XAttribute("id", "MOTEL_1"), new XAttribute("name", "Motel Room 1"), new XAttribute("x", "312.936"), new XAttribute("y", "-218.710"), new XAttribute("z", "54.222"))
                    )
                )
            ));
            doc.Save(FilePath);
        }
    }
}