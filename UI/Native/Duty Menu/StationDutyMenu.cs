using EmsPlus.Configuration;
using EmsPlus.Core;
using EmsPlus.Managers;
using Rage;
using RAGENativeUI;
using RAGENativeUI.Elements;
using System.Collections.Generic;
using System.Linq;

namespace EmsPlus.UI.Native.DutyMenu
{
    public static class StationDutyMenu
    {
        public static UIMenu DutyMenu;
        private static UIMenuListItem _stationList;
        private static UIMenuListItem _rankList;
        private static UIMenuItem _toggleDutyBtn;

        private static List<StationLocation> _availableStations = new List<StationLocation>();
        private static List<RankDefinition> _availableRanks = new List<RankDefinition>();

        public static void Build()
        {
            if (DutyMenu != null) return;

            DutyMenu = new UIMenu(Localization.Get("MENU_STATION_TITLE", "~b~Station Services"), Localization.Get("MENU_STATION_SUBTITLE", "~b~Select Station & Rank"));
            MenuCore.AddMenu(DutyMenu);

            DutyMenu.OnItemSelect += OnMenuItemSelected;
        }

        public static void Open(StationLocation currentStation)
        {
            if (DutyMenu == null)
            {
                Build();
            }

            RefreshMenu(currentStation);
            DutyMenu.Visible = true;
        }

        private static void RefreshMenu(StationLocation currentStation)
        {
            DutyMenu.Clear();

            if (EntryPoint.StationsConfig?.Locations == null || EntryPoint.RanksConfig?.Ranks == null)
            {
                Game.DisplayNotification("~r~Error:~w~ Configurations not loaded.");
                return;
            }

            _availableStations = EntryPoint.StationsConfig.Locations.ToList();
            _availableRanks = EntryPoint.RanksConfig.Ranks.ToList();

            if (_availableStations.Count == 0 || _availableRanks.Count == 0)
            {
                Game.DisplayNotification("~r~Error:~w~ No stations or ranks loaded in XML.");
                return;
            }

            List<dynamic> stationNames = _availableStations.Select(s => (dynamic)s.Name).ToList();
            int defaultStationIdx = currentStation != null ? _availableStations.IndexOf(currentStation) : 0;
            if (defaultStationIdx == -1) defaultStationIdx = 0;

            _stationList = new UIMenuListItem(Localization.Get("MENU_STATION_LABEL", "~b~Station"), stationNames, defaultStationIdx, Localization.Get("MENU_STATION_DESC", "Select the station you are operating out of."));
            DutyMenu.AddItem(_stationList);

            List<dynamic> rankNames = _availableRanks.Select(r => (dynamic)$"{r.Name} ({r.ShortName})").ToList();
            int currentRankIdx = EmsService.CurrentRank != null ? _availableRanks.IndexOf(EmsService.CurrentRank) : 0;
            if (currentRankIdx == -1) currentRankIdx = 0;

            _rankList = new UIMenuListItem(Localization.Get("MENU_RANK_LABEL", "~y~Title / Rank"), rankNames, currentRankIdx, Localization.Get("MENU_RANK_DESC", "Select your active title/rank and uniform."));
            DutyMenu.AddItem(_rankList);

            string dutyLabel = EmsService.IsOnDuty ? Localization.Get("MENU_DUTY_OFF_LABEL", "~r~Go Off Duty") : Localization.Get("MENU_DUTY_ON_LABEL", "~g~Go On Duty");
            string dutyDesc = EmsService.IsOnDuty ? Localization.Get("MENU_DUTY_OFF_DESC", "Clock out and return to civilian duties.") : Localization.Get("MENU_DUTY_ON_DESC", "Clock in with the selected station and uniform.");
            _toggleDutyBtn = new UIMenuItem(dutyLabel, dutyDesc);
            DutyMenu.AddItem(_toggleDutyBtn);

            DutyMenu.RefreshIndex();
        }

        private static void OnMenuItemSelected(UIMenu sender, UIMenuItem selectedItem, int index)
        {
            if (selectedItem == _toggleDutyBtn)
            {
                MenuCore.CloseAll();

                if (_availableStations.Count == 0 || _availableRanks.Count == 0) return;

                var selectedStation = _availableStations[_stationList.Index];
                var selectedRank = _availableRanks[_rankList.Index];

                if (!EmsService.IsOnDuty)
                {
                    StationManager.ActiveStation = selectedStation;
                    EmsService.CurrentRank = selectedRank;
                    EmsService.ToggleDuty();
                    EntryPoint.StartPluginLogic();
                }
                else
                {
                    EmsService.ToggleDuty();
                    EntryPoint.StopPluginLogic();
                    StationManager.ActiveStation = null;
                }

                StationManager.UpdateBlipVisibility();
            }
        }
    }
}