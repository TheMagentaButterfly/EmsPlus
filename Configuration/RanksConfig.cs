using Rage;
using Rage.Native;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace EmsPlus.Configuration
{
    public class RankPed
    {
        public string Model { get; set; }
        public Dictionary<int, PedVariation> Components { get; set; } = new Dictionary<int, PedVariation>();
        public Dictionary<int, PedVariation> Props { get; set; } = new Dictionary<int, PedVariation>();

        public void ApplyTo(Ped player)
        {
            if (player == null || !player.Exists()) return;

            bool modelChanged = false;
            if (!string.IsNullOrEmpty(Model) && !player.Model.Name.Equals(Model, StringComparison.OrdinalIgnoreCase))
            {
                Model newModel = new Model(Model);
                newModel.LoadAndWait();
                Game.LocalPlayer.Model = newModel;
                newModel.Dismiss();
                player = Game.LocalPlayer.Character;
                modelChanged = true;
            }

            if (modelChanged && player.Model.Name.ToLower().Contains("freemode"))
            {
                NativeFunction.Natives.SET_PED_DEFAULT_COMPONENT_VARIATION(player);
            }

            int currentHairDrawable = NativeFunction.Natives.GET_PED_DRAWABLE_VARIATION<int>(player, 2);
            int currentHairTexture = NativeFunction.Natives.GET_PED_TEXTURE_VARIATION<int>(player, 2);

            NativeFunction.CallByHash<int>(0xE861D0B05C7662B8, player, false, 0);
            NativeFunction.Natives.CLEAR_ALL_PED_PROPS(player);

            var sortedKeys = Components.Keys.OrderByDescending(k => k).ToList();
            foreach (int key in sortedKeys)
            {
                var val = Components[key];
                int drawable = val.Drawable > 0 ? val.Drawable - 1 : 0;
                int texture = val.Texture > 0 ? val.Texture - 1 : 0;
                NativeFunction.Natives.SET_PED_COMPONENT_VARIATION(player, key, drawable, texture, 0);
            }

            if (!Components.ContainsKey(2) && currentHairDrawable >= 0)
            {
                NativeFunction.Natives.SET_PED_COMPONENT_VARIATION(player, 2, currentHairDrawable, currentHairTexture, 0);
            }

            foreach (var kvp in Props)
            {
                int key = kvp.Key;
                var val = kvp.Value;

                if (val.Drawable <= 0)
                {
                    NativeFunction.Natives.CLEAR_PED_PROP(player, key);
                }
                else
                {
                    int drawable = val.Drawable - 1;
                    int texture = val.Texture > 0 ? val.Texture - 1 : 0;
                    NativeFunction.Natives.SET_PED_PROP_INDEX(player, key, drawable, texture, true);
                }
            }
        }
    }

    public class RankDefinition
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
        public List<RankPed> Peds { get; set; } = new List<RankPed>();

        public void ApplyTo(Ped player)
        {
            if (player == null || !player.Exists() || Peds.Count == 0) return;

            RankPed selectedPed = null;
            if (player.IsMale)
            {
                selectedPed = Peds.FirstOrDefault(p => p.Model.ToLower().Contains("mp_m") || p.Model.ToLower().Contains("_m_") || p.Model.ToLower().Contains("male")) ?? Peds[0];
            }
            else
            {
                selectedPed = Peds.FirstOrDefault(p => p.Model.ToLower().Contains("mp_f") || p.Model.ToLower().Contains("_f_") || p.Model.ToLower().Contains("female")) ?? Peds[0];
            }

            selectedPed.ApplyTo(player);
        }
    }

    public class RanksConfig
    {
        private const string FilePath = "Plugins/EmsPlus/Settings/Data/Ranks.xml";
        public List<RankDefinition> Ranks { get; private set; } = new List<RankDefinition>();

        private static readonly Dictionary<string, int> ComponentMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            { "face", 0 },
            { "mask", 1 }, { "beard", 1 },
            { "hair", 2 },
            { "shirt", 3 }, { "torso", 3 },
            { "pants", 4 }, { "legs", 4 },
            { "hands", 5 }, { "bags", 5 },
            { "shoes", 6 },
            { "eyes", 7 }, { "neck", 7 },
            { "accessories", 8 }, { "undershirt", 8 },
            { "tasks", 9 }, { "armor", 9 },
            { "decals", 10 },
            { "shirtoverlay", 11 }, { "tops", 11 }
        };

        private static readonly Dictionary<string, int> PropMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            { "hats", 0 }, { "glasses", 1 }, { "ears", 2 }, { "watches", 3 }
        };

        public void Load()
        {
            Ranks.Clear();
            if (!File.Exists(FilePath)) CreateDefaultFile();

            try
            {
                XDocument doc = XDocument.Load(FilePath);
                foreach (XElement rEl in doc.Descendants("Rank"))
                {
                    var rank = new RankDefinition
                    {
                        ID = rEl.Attribute("id")?.Value ?? "EMT",
                        Name = rEl.Attribute("name")?.Value ?? "Paramedic",
                        ShortName = rEl.Attribute("shortName")?.Value ?? "Medic"
                    };

                    var pedsEl = rEl.Element("Peds") ?? rEl;
                    foreach (XElement pEl in pedsEl.Elements("Ped"))
                    {
                        var ped = new RankPed
                        {
                            Model = pEl.Value.Trim()
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

                        rank.Peds.Add(ped);
                    }

                    if (rank.Peds.Count > 0)
                    {
                        Ranks.Add(rank);
                    }
                }
                Game.Console.Print($"[EmsPlus] Loaded {Ranks.Count} ranks from Ranks.xml.");
            }
            catch (Exception ex)
            {
                Game.Console.Print($"[EmsPlus] Error loading Ranks.xml: {ex.Message}");
            }
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
<EmsPlusRanks>
  <Ranks>
    <!-- EMT -->
    <Rank id=""EMT"" name=""EMT"" shortName=""EMT"">
      <Peds>
        <Ped prop_glasses=""0"" tex_glasses=""0"" prop_hats=""0"" tex_hats=""0"" prop_ears=""0"" tex_ears=""0"" comp_beard=""1"" tex_beard=""1"" comp_shirtoverlay=""34"" tex_shirtoverlay=""19"" comp_shirt=""89"" tex_shirt=""1"" comp_decals=""1"" tex_decals=""1"" comp_accessories=""35"" tex_accessories=""1"" comp_pants=""20"" tex_pants=""1"" comp_shoes=""22"" tex_shoes=""1"" comp_eyes=""49"" tex_eyes=""1"" comp_tasks=""1"" tex_tasks=""1"" comp_hands=""28"" tex_hands=""7"">MP_M_FREEMODE_01</Ped>
        <Ped prop_glasses=""0"" tex_glasses=""0"" prop_hats=""0"" tex_hats=""0"" prop_ears=""0"" tex_ears=""0"" comp_beard=""1"" tex_beard=""1"" comp_shirtoverlay=""34"" tex_shirtoverlay=""19"" comp_shirt=""89"" tex_shirt=""1"" comp_decals=""1"" tex_decals=""1"" comp_accessories=""35"" tex_accessories=""1"" comp_pants=""20"" tex_pants=""1"" comp_shoes=""22"" tex_shoes=""1"" comp_eyes=""49"" tex_eyes=""1"" comp_tasks=""1"" tex_tasks=""1"" comp_hands=""28"" tex_hands=""7"">MP_F_FREEMODE_01</Ped>
      </Peds>
    </Rank>

    <!-- Paramedic -->
    <Rank id=""PARAMEDIC"" name=""Paramedic"" shortName=""Medic"">
      <Peds>
        <Ped prop_glasses=""0"" tex_glasses=""0"" prop_hats=""0"" tex_hats=""0"" prop_ears=""0"" tex_ears=""0"" comp_beard=""1"" tex_beard=""1"" comp_shirtoverlay=""33"" tex_shirtoverlay=""19"" comp_shirt=""93"" tex_shirt=""1"" comp_decals=""1"" tex_decals=""1"" comp_accessories=""35"" tex_accessories=""1"" comp_pants=""20"" tex_pants=""1"" comp_shoes=""22"" tex_shoes=""1"" comp_eyes=""49"" tex_eyes=""1"" comp_tasks=""1"" tex_tasks=""1"" comp_hands=""28"" tex_hands=""7"">MP_M_FREEMODE_01</Ped>
        <Ped prop_glasses=""0"" tex_glasses=""0"" prop_hats=""0"" tex_hats=""0"" prop_ears=""0"" tex_ears=""0"" comp_beard=""1"" tex_beard=""1"" comp_shirtoverlay=""33"" tex_shirtoverlay=""19"" comp_shirt=""93"" tex_shirt=""1"" comp_decals=""1"" tex_decals=""1"" comp_accessories=""35"" tex_accessories=""1"" comp_pants=""20"" tex_pants=""1"" comp_shoes=""22"" tex_shoes=""1"" comp_eyes=""49"" tex_eyes=""1"" comp_tasks=""1"" tex_tasks=""1"" comp_hands=""28"" tex_hands=""7"">MP_F_FREEMODE_01</Ped>
      </Peds>
    </Rank>
  </Ranks>
</EmsPlusRanks>";

                File.WriteAllText(FilePath, xmlContent);
                Game.Console.Print("[EmsPlus] Created default Ranks.xml.");
            }
            catch { }
        }
    }
}