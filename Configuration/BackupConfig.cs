using Rage;
using Rage.Native;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace EmsPlus.Configuration
{
    public interface IWeightedItem
    {
        int Chance { get; }
    }

    public class BackupVehicle : IWeightedItem
    {
        public string Model { get; set; }
        public int Chance { get; set; }
    }

    public class PedVariation
    {
        public int Drawable { get; set; }
        public int Texture { get; set; }
    }

    public class BackupPed : IWeightedItem
    {
        public string Model { get; set; }
        public int Chance { get; set; }
        public Dictionary<int, PedVariation> Components { get; set; } = new Dictionary<int, PedVariation>();
        public Dictionary<int, PedVariation> Props { get; set; } = new Dictionary<int, PedVariation>();

        public void ApplyTo(Ped ped)
        {
            if (ped == null || !ped.Exists()) return;

            string modelName = ped.Model.Name.ToLower();
            if (modelName.Contains("freemode"))
            {
                NativeFunction.Natives.SET_PED_DEFAULT_COMPONENT_VARIATION(ped);

                System.Random rnd = new System.Random();
                int shapeFirst = rnd.Next(0, 46);
                int shapeSecond = rnd.Next(0, 46);
                int skinFirst = rnd.Next(0, 46);
                int skinSecond = rnd.Next(0, 46);
                float shapeMix = (float)rnd.NextDouble();
                float skinMix = (float)rnd.NextDouble();

                NativeFunction.Natives.SET_PED_HEAD_BLEND_DATA(ped, shapeFirst, shapeSecond, 0, skinFirst, skinSecond, 0, shapeMix, skinMix, 0f, false);
                NativeFunction.CallByHash<int>(0x50B56988B170AFDF, ped, rnd.Next(0, 10));
            }

            NativeFunction.CallByHash<int>(0xE861D0B05C7662B8, ped, false, 0);
            NativeFunction.Natives.CLEAR_ALL_PED_PROPS(ped);

            var sortedKeys = Components.Keys.OrderByDescending(k => k).ToList();
            foreach (int key in sortedKeys)
            {
                var val = Components[key];
                int drawable = val.Drawable > 0 ? val.Drawable - 1 : 0;
                int texture = val.Texture > 0 ? val.Texture - 1 : 0;
                NativeFunction.Natives.SET_PED_COMPONENT_VARIATION(ped, key, drawable, texture, 0);
            }

            foreach (var kvp in Props)
            {
                int key = kvp.Key;
                var val = kvp.Value;

                if (val.Drawable <= 0)
                {
                    NativeFunction.Natives.CLEAR_PED_PROP(ped, key);
                }
                else
                {
                    int drawable = val.Drawable - 1;
                    int texture = val.Texture > 0 ? val.Texture - 1 : 0;
                    NativeFunction.Natives.SET_PED_PROP_INDEX(ped, key, drawable, texture, true);
                }
            }
        }
    }

    public class BackupDepartment : IWeightedItem
    {
        public string Name { get; set; }
        public int Chance { get; set; }
        public string ServiceType { get; set; } = "Ambulance"; // Default service category
        public List<BackupVehicle> Vehicles { get; set; } = new List<BackupVehicle>();
        public List<BackupPed> Peds { get; set; } = new List<BackupPed>();

        public BackupVehicle GetRandomVehicle() => BackupConfig.GetRandomItem(Vehicles);
        public BackupPed GetRandomPed() => BackupConfig.GetRandomItem(Peds);
    }

    public class BackupConfig
    {
        private const string FilePath = "Plugins/EmsPlus/Settings/Data/Backup.xml";
        public List<BackupDepartment> Departments { get; private set; } = new List<BackupDepartment>();

        private static readonly Dictionary<string, int> ComponentMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            { "face", 0 }, { "mask", 1 }, { "beard", 1 }, { "hair", 2 },
            { "shirt", 3 }, { "torso", 3 }, { "pants", 4 }, { "legs", 4 },
            { "hands", 5 }, { "bags", 5 }, { "shoes", 6 }, { "eyes", 7 },
            { "neck", 7 }, { "accessories", 8 }, { "undershirt", 8 },
            { "tasks", 9 }, { "armor", 9 }, { "decals", 10 }, { "shirtoverlay", 11 }, { "tops", 11 }
        };

        private static readonly Dictionary<string, int> PropMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            { "hats", 0 }, { "glasses", 1 }, { "ears", 2 }, { "watches", 3 }
        };

        public void Load()
        {
            Departments.Clear();
            if (!File.Exists(FilePath)) CreateDefaultFile();

            try
            {
                XDocument doc = XDocument.Load(FilePath);
                foreach (XElement deptEl in doc.Descendants("Department"))
                {
                    var dept = new BackupDepartment
                    {
                        Name = deptEl.Attribute("name")?.Value ?? "Unknown Department",
                        Chance = ParseInt(deptEl.Attribute("chance")?.Value, 100),
                        ServiceType = deptEl.Attribute("service")?.Value ?? "Ambulance" // Load custom service type
                    };

                    var vehsEl = deptEl.Element("Vehicles");
                    if (vehsEl != null)
                    {
                        foreach (XElement vEl in vehsEl.Elements("Vehicle"))
                        {
                            dept.Vehicles.Add(new BackupVehicle
                            {
                                Model = vEl.Value.Trim(),
                                Chance = ParseInt(vEl.Attribute("chance")?.Value, 10)
                            });
                        }
                    }

                    var pedsEl = deptEl.Element("Peds");
                    if (pedsEl != null)
                    {
                        foreach (XElement pEl in pedsEl.Elements("Ped"))
                        {
                            var ped = new BackupPed
                            {
                                Model = pEl.Value.Trim(),
                                Chance = ParseInt(pEl.Attribute("chance")?.Value, 10)
                            };

                            foreach (var attr in pEl.Attributes())
                            {
                                string attrName = attr.Name.LocalName.ToLower();

                                if (attrName.StartsWith("comp_"))
                                {
                                    string type = attrName.Substring(5);
                                    if (ComponentMap.TryGetValue(type, out int id))
                                    {
                                        int drawable = ParseInt(attr.Value, 0);
                                        string texName = "tex_" + type;
                                        XAttribute texAttr = pEl.Attributes().FirstOrDefault(a => a.Name.LocalName.Equals(texName, StringComparison.OrdinalIgnoreCase));
                                        int texture = texAttr != null ? ParseInt(texAttr.Value, 0) : 0;

                                        ped.Components[id] = new PedVariation { Drawable = drawable, Texture = texture };
                                    }
                                }
                                else if (attrName.StartsWith("prop_"))
                                {
                                    string type = attrName.Substring(5);
                                    if (PropMap.TryGetValue(type, out int id))
                                    {
                                        int drawable = ParseInt(attr.Value, -1);
                                        string texName = "tex_" + type;
                                        XAttribute texAttr = pEl.Attributes().FirstOrDefault(a => a.Name.LocalName.Equals(texName, StringComparison.OrdinalIgnoreCase));
                                        int texture = texAttr != null ? ParseInt(texAttr.Value, 0) : 0;

                                        ped.Props[id] = new PedVariation { Drawable = drawable, Texture = texture };
                                    }
                                }
                            }
                            dept.Peds.Add(ped);
                        }
                    }

                    if (dept.Vehicles.Count > 0 && dept.Peds.Count > 0)
                    {
                        Departments.Add(dept);
                    }
                }
                Game.Console.Print($"[EmsPlus] Loaded {Departments.Count} Backup Departments from Backup.xml.");
            }
            catch (Exception ex)
            {
                Game.Console.Print($"[EmsPlus] Error loading Backup.xml: {ex.Message}");
            }
        }

        public BackupDepartment GetRandomDepartmentForService(string serviceType)
        {
            var depts = Departments.Where(d => d.ServiceType.Equals(serviceType, StringComparison.OrdinalIgnoreCase)).ToList();
            return depts.Count > 0 ? GetRandomItem(depts) : null;
        }

        public static T GetRandomItem<T>(IEnumerable<T> items) where T : IWeightedItem
        {
            if (items == null || !items.Any()) return default(T);

            int totalWeight = items.Sum(i => i.Chance);
            if (totalWeight <= 0) return items.FirstOrDefault();

            int random = new Random().Next(0, totalWeight);
            foreach (var item in items)
            {
                if (random < item.Chance) return item;
                random -= item.Chance;
            }
            return items.FirstOrDefault();
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
                string xmlContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<EmsPlusBackup>
  <Departments>
    <!-- Land Ambulance Service -->
    <Department name=""Los Santos Fire Department"" chance=""50"" service=""Ambulance"">
      <Vehicles>
        <Vehicle chance=""100"">ambulance</Vehicle>
      </Vehicles>
      <Peds>
        <Ped chance=""100"">s_m_m_paramedic_01</Ped>
      </Peds>
    </Department>

    <!-- Police Security Service -->
    <Department name=""Los Santos City Police"" chance=""100"" service=""Police"">
      <Vehicles>
        <Vehicle chance=""50"">police</Vehicle>
        <Vehicle chance=""50"">police2</Vehicle>
      </Vehicles>
      <Peds>
        <Ped chance=""100"">s_m_y_cop_01</Ped>
      </Peds>
    </Department>

    <!-- Fire Response Service -->
    <Department name=""Los Santos Fire Rescue"" chance=""100"" service=""Fire"">
      <Vehicles>
        <Vehicle chance=""100"">firetruk</Vehicle>
      </Vehicles>
      <Peds>
        <Ped chance=""100"">s_m_y_fireman_01</Ped>
      </Peds>
    </Department>

    <!-- Air Medevac Service -->
    <Department name=""Medevac Air Unit"" chance=""100"" service=""Helicopter"">
      <Vehicles>
        <Vehicle chance=""100"">polmav</Vehicle>
      </Vehicles>
      <Peds>
        <Ped chance=""100"">s_m_m_paramedic_01</Ped>
      </Peds>
    </Department>
  </Departments>
</EmsPlusBackup>";

                File.WriteAllText(FilePath, xmlContent);
                Game.Console.Print("[EmsPlus] Created default Backup.xml.");
            }
            catch { }
        }
    }
}