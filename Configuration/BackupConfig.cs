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

            foreach (var kvp in Components)
            {
                NativeFunction.Natives.SET_PED_COMPONENT_VARIATION(ped, kvp.Key, kvp.Value.Drawable, kvp.Value.Texture, 0);
            }

            foreach (var kvp in Props)
            {
                // If drawable is -1, it clears the prop
                if (kvp.Value.Drawable == -1)
                    NativeFunction.Natives.CLEAR_PED_PROP(ped, kvp.Key);
                else
                    NativeFunction.Natives.SET_PED_PROP_INDEX(ped, kvp.Key, kvp.Value.Drawable, kvp.Value.Texture, true);
            }
        }
    }

    public class BackupDepartment : IWeightedItem
    {
        public string Name { get; set; }
        public int Chance { get; set; }
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
            { "torso", 3 }, { "hands", 3 }, { "legs", 4 }, { "pants", 4 },
            { "bags", 5 }, { "tasks", 5 }, { "shoes", 6 },
            { "accessories", 7 }, { "undershirt", 8 }, { "shirt", 8 },
            { "armor", 9 }, { "decals", 10 }, { "tops", 11 }, { "shirtoverlay", 11 }
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
                        Chance = ParseInt(deptEl.Attribute("chance")?.Value, 100)
                    };

                    // Parse Vehicles
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

                    // Parse Peds
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
                                        int texture = ParseInt(pEl.Attribute("tex_" + type)?.Value, 0);
                                        ped.Components[id] = new PedVariation { Drawable = drawable, Texture = texture };
                                    }
                                }
                                else if (attrName.StartsWith("prop_"))
                                {
                                    string type = attrName.Substring(5);
                                    if (PropMap.TryGetValue(type, out int id))
                                    {
                                        int drawable = ParseInt(attr.Value, -1);
                                        int texture = ParseInt(pEl.Attribute("tex_" + type)?.Value, 0);
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

        public BackupDepartment GetRandomDepartment() => GetRandomItem(Departments);

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
    <!-- Vanilla Los Santos Fire Department -->
    <Department name=""Los Santos Fire Department"" chance=""50"">
      <Vehicles>
        <Vehicle chance=""100"">ambulance</Vehicle>
      </Vehicles>
      <Peds>
        <Ped chance=""100"">s_m_m_paramedic_01</Ped>
      </Peds>
    </Department>

    <!-- Example EUP / Custom Ped Department -->
    <Department name=""EUP Medics"" chance=""50"">
      <Vehicles>
        <Vehicle chance=""100"">ambulance</Vehicle>
      </Vehicles>
      <Peds>
        <!-- MP Male EUP Paramedic Example -->
        <Ped comp_beard=""1"" tex_beard=""1"" comp_shirtoverlay=""32"" tex_shirtoverlay=""1"" comp_shirt=""5"" tex_shirt=""1"" comp_decals=""9"" tex_decals=""2"" comp_accessories=""58"" tex_accessories=""1"" comp_pants=""21"" tex_pants=""1"" comp_shoes=""22"" tex_shoes=""1"" comp_hands=""10"" tex_hands=""1"" prop_glasses=""0"" tex_glasses=""0"" prop_hats=""12"" tex_hats=""2"" chance=""50"">MP_M_FREEMODE_01</Ped>
        <!-- MP Female EUP Paramedic Example -->
        <Ped comp_beard=""1"" tex_beard=""1"" comp_shirtoverlay=""32"" tex_shirtoverlay=""3"" comp_shirt=""5"" tex_shirt=""1"" comp_decals=""9"" tex_decals=""9"" comp_accessories=""66"" tex_accessories=""2"" comp_pants=""21"" tex_pants=""13"" comp_shoes=""22"" tex_shoes=""1"" comp_hands=""11"" tex_hands=""1"" prop_hats=""22"" tex_hats=""1"" chance=""50"">MP_F_FREEMODE_01</Ped>
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