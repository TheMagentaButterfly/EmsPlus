using EmsPlus.Managers;
using RAGENativeUI;
using RAGENativeUI.Elements;
using System.Collections.Generic;

namespace EmsPlus.UI.Native.BackupMenu
{
    public static class BackupMenuBuilder
    {
        public static UIMenu BackupMenu;
        private static UIMenuListItem _requestEmsItem;
        private static UIMenuListItem _requestFireItem;
        private static UIMenuListItem _requestPoliceItem;
        private static UIMenuItem _requestHeliItem;
        private static List<string> _codeDescs;

        public static void Build()
        {
            BackupMenu = new UIMenu(Localization.Get("MENU_BACKUP_COLORED", "~b~Backup"), Localization.Get("SUBTITLE_BACKUP", "~b~Request Resources"));
            MenuCore.AddMenu(BackupMenu);

            List<dynamic> codes = new List<dynamic>
            {
                Localization.Get("LBL_CODE_1", "Code 1"),
                Localization.Get("LBL_CODE_2", "Code 2"),
                Localization.Get("LBL_CODE_3", "Code 3")
            };

            _codeDescs = new List<string>
            {
                Localization.Get("DESC_CODE_1", "Routine response (obey traffic laws, no lights or sirens)."),
                Localization.Get("DESC_CODE_2", "Urgent response (lights only, no sirens)."),
                Localization.Get("DESC_CODE_3", "Emergency response (lights and sirens).")
            };

            _requestEmsItem = new UIMenuListItem(Localization.Get("ACT_REQ_AMBULANCE_COLORED", "~o~Request EMS Unit"), codes, 2, _codeDescs[2]);
            BackupMenu.AddItem(_requestEmsItem);

            _requestFireItem = new UIMenuListItem("~r~Request Fire Unit", codes, 2, _codeDescs[2]);
            BackupMenu.AddItem(_requestFireItem);

            _requestPoliceItem = new UIMenuListItem("~b~Request Police Unit", codes, 2, _codeDescs[2]);
            BackupMenu.AddItem(_requestPoliceItem);

            _requestHeliItem = new UIMenuItem("~y~Request Air Medevac", "Request a medical evacuation helicopter to land near your position.");
            BackupMenu.AddItem(_requestHeliItem);

            BackupMenu.OnListChange += (s, item, index) =>
            {
                if (item == _requestEmsItem) _requestEmsItem.Description = _codeDescs[index];
                else if (item == _requestFireItem) _requestFireItem.Description = _codeDescs[index];
                else if (item == _requestPoliceItem) _requestPoliceItem.Description = _codeDescs[index];
            };

            BackupMenu.OnItemSelect += (s, item, index) =>
            {
                MenuCore.CloseAll();

                if (item == _requestEmsItem)
                {
                    BackupManager.RequestBackup("Ambulance", _requestEmsItem.Index + 1);
                }
                else if (item == _requestFireItem)
                {
                    BackupManager.RequestBackup("Fire", _requestFireItem.Index + 1);
                }
                else if (item == _requestPoliceItem)
                {
                    BackupManager.RequestBackup("Police", _requestPoliceItem.Index + 1);
                }
                else if (item == _requestHeliItem)
                {
                    BackupManager.RequestBackup("Helicopter", 3);
                }
            };
        }
    }
}