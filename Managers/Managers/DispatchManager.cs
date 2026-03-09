using System.Collections.Generic;
using EmsPlus.Framework;
using Rage.Native;

namespace EmsPlus.Managers
{
    public static class DispatchManager
    {
        private static readonly Dictionary<string, string> ZoneAudioMap = new Dictionary<string, string>
        {
            {"AIRP", "LOS_SANTOS_INTERNATIONAL"},
            {"ALAMO", "THE_ALAMO_SEA"},
            {"ALTA", "ALTA"},
            {"ARMYB", "FORT_ZANCUDO"},
            {"BANHAMC", "BANHAM_CANYON"},
            {"BANNING", "BANNING"},
            {"BAYTRE", "BAYTREE_CANYON"},
            {"BEACH", "VESPUCCI_BEACH"},
            {"BHAMCA", "BANHAM_CANYON"},
            {"BRADP", "BRADDOCK_PASS"},
            {"BRADT", "THE_BRADDOCK_TUNNEL"},
            {"BURTON", "BURTON"},
            {"CALAFB", "THE_CALAFIA_BRIDGE"},
            {"CANNY", "RATON_CANYON"},
            {"CCREAK", "CASSIDY_CREEK"},
            {"CHAMH", "CHAMBERLAIN_HILLS"},
            {"CHIAS", "THE_CHILLIAD_MOUNTAIN_STATE_WILDERNESS"},
            {"CHU", "CHUMASH"},
            {"CMSW", "CHILLIAD_MOUNTAIN_STATE_WILDERNESS"},
            {"CYPRE", "CYPRESS_FLATS"},
            {"DAVIS", "DAVIS"},
            {"DELBE", "DEL_PERRO_BEACH"},
            {"DELPE", "DEL_PERRO"},
            {"DELSOL", "PUERTO_DEL_SOL"},
            {"DOWNT", "DOWNTOWN"},
            {"DTVINE", "DOWNTOWN_VINEWOOD"},
            {"EAST_V", "EAST_VINEWOOD"},
            {"EBURO", "EL_BURRO_HEIGHTS"},
            {"ELCHLO", "SAN_ANDREAS"},
            {"ELYSIAN", "ELYSIAN_ISLAND"},
            {"GALVICE", "GALILEO_OBSERVATORY"},
            {"GOLF", "GWC_GOLF_CLUB"},
            {"GRAPES", "GRAPESEED"},
            {"GREATW", "GREAT_CHAPARRAL"},
            {"HAWICK", "HAWICK"},
            {"HORS", "VINEWOOD_RACETRACK"},
            {"HUMLAB", "HUMANE_LABS"},
            {"JAIL", "BOLLINGBROKE_PENITENTIARY"},
            {"KOREAT", "LITTLE_SEOUL"},
            {"LAGO", "LAGO_ZANCUDO"},
            {"LDAM", "LAND_ACT_DAM"},
            {"LEGSQU", "STRAWBERRY"},
            {"LMESA", "LA_MESA"},
            {"LOSPUER", "LA_PUERTA"},
            {"MIRR", "MIRROR_PARK"},
            {"MOVIE", "BACKLOT_CITY"},
            {"MTCHIL", "MOUNT_CHILLIAD"},
            {"MTGORDO", "MOUNT_GORDO"},
            {"MTJOSE", "MOUNT_JOSIAH"},
            {"MURRI", "MURRIETA_HEIGHTS"},
            {"NCHU", "NORTH_CHUMASH"},
            {"NOOSE", "NOOSE_HQ"},
            {"OCEANA", "PACIFIC_OCEAN"},
            {"PALCOV", "PALETO_COVE"},
            {"PALETO", "PALETO_BAY"},
            {"PALFOR", "PALETO_FOREST"},
            {"PALHIGH", "PALOMINO_HIGHLANDS"},
            {"PALMPOW", "PALMER_TAYLOR_POWER_STATION"},
            {"PBLUFF", "PACIFIC_BLUFFS"},
            {"PBOX", "PILLBOX_HILL"},
            {"PROCOB", "PROCOPIO_BEACH"},
            {"RANCHO", "RANCHO"},
            {"RGLEN", "RICHMAN_GLEN"},
            {"RICHM", "RICHMAN"},
            {"ROCKF", "ROCKFORD_HILLS"},
            {"RTRAK", "THE_REDWOOD_LIGHTS_TRACK"},
            {"SANCHIA", "SAN_CHIANSKI_MOUNTAINS"},
            {"SANAND", "SAN_ANDREAS"},
            {"SANDY", "SANDY_SHORES"},
            {"SKID", "MISSION_ROW"},
            {"SLAB", "STAB_CITY"},
            {"STADIUM", "THE_STADIUM"},
            {"STRAW", "STRAWBERRY"},
            {"TATAMO", "TATAVIAM_MOUNTAINS"},
            {"TERMINA", "TERMINAL"},
            {"TEXTI", "TEXTILE_CITY"},
            {"TONGVAH", "TONGVA_HILLS"},
            {"TONGVAV", "TONGVA_VALLEY"},
            {"VCANA", "VESPUCCI_CANALS"},
            {"VESP", "VESPUCCI"},
            {"VINE", "VINEWOOD"},
            {"WINDF", "RON_ALTERNATES_WINDFARM"},
            {"WVINE", "WEST_VINEWOOD"},
            {"ZANCUDO", "ZANCUDO_RIVER"},
            {"ZP_ORT", "PORT_OF_SOUTH_LOS_SANTOS"},
            {"ZQ_UAR", "DAVIS_QUARTZ"}
        };

        public static void PlayCalloutAudio(EmsCallout callout)
        {
            NativeFunction.Natives.GET_STREET_NAME_AT_COORD(callout.CalloutPosition.X, callout.CalloutPosition.Y, callout.CalloutPosition.Z, out uint sHash, out uint cHash);
            string streetName = NativeFunction.Natives.GET_STREET_NAME_FROM_HASH_KEY<string>(sHash);

            string internalZone = NativeFunction.Natives.GET_NAME_OF_ZONE<string>(callout.CalloutPosition.X, callout.CalloutPosition.Y, callout.CalloutPosition.Z);

            string audioZoneName = internalZone;
            if (ZoneAudioMap.TryGetValue(internalZone, out string fullName))
            {
                audioZoneName = fullName;
            }

            List<DispatchAudioItem> sequence = new List<DispatchAudioItem>();

            sequence.Add(new DispatchAudioItem("GENERAL", "ATTENTION_ALL_UNITS_01"));
            sequence.Add(new DispatchAudioItem("GENERAL", "WE_HAVE_A"));
            sequence.Add(new DispatchAudioItem("CALLOUTS", callout.CalloutName));
            sequence.Add(new DispatchAudioItem("GENERAL", "IN"));

            sequence.Add(new DispatchAudioItem("ZONES", audioZoneName));

            sequence.Add(new DispatchAudioItem("GENERAL", "ON"));
            sequence.Add(new DispatchAudioItem("STREETS", streetName));
            sequence.Add(new DispatchAudioItem("GENERAL", "RESPOND_CODE_3"));

            AudioManager.PlayDispatchSequence(sequence);
        }
    }
}