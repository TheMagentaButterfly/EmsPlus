using EmsPlus.Managers;
using RAGENativeUI;
using RAGENativeUI.Elements;
using System.Collections.Generic;

namespace EmsPlus.UI.Native.BackupMenu
{
    public static class BackupMenuBuilder
    {
        public static UIMenu BackupMenu;

        private static UIMenuListItem _responseTypeItem;
        private static UIMenuItem _btnAmbulance;
        private static UIMenuItem _btnLocalPatrol;
        private static UIMenuItem _btnStatePatrol;
        private static UIMenuItem _btnFire;
        private static UIMenuItem _btnHeli;

        private static List<string> _codeDescs;

        public static void Build()
        {
            BackupMenu = new UIMenu(Localization.Get("MENU_BACKUP_COLORED", "Backup"), Localization.Get("SUBTITLE_BACKUP", "Request Resources"));
            MenuCore.AddMenu(BackupMenu);

            List<dynamic> responseCodes = new List<dynamic>
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

            _responseTypeItem = new UIMenuListItem(Localization.Get("LBL_RESPONSE_TYPE", "Response Type"), responseCodes, 2, _codeDescs[2]);
            BackupMenu.AddItem(_responseTypeItem);

            _btnAmbulance = new UIMenuItem(Localization.Get("LBL_AMBULANCE", "Ambulance"), Localization.Get("DESC_AMBULANCE", "Request a ground ambulance unit."));
            _btnLocalPatrol = new UIMenuItem(Localization.Get("LBL_LOCAL_PATROL", "Local Patrol"), Localization.Get("DESC_LOCAL_PATROL", "Request a local police patrol unit."));
            _btnFire = new UIMenuItem(Localization.Get("LBL_FIRE_DEPARTMENT", "Fire Department"), Localization.Get("DESC_FIRE_DEPARTMENT", "Request a fire engine unit."));

            BackupMenu.AddItem(_btnAmbulance);
            BackupMenu.AddItem(_btnLocalPatrol);
            BackupMenu.AddItem(_btnFire);

            BackupMenu.OnListChange += (s, item, index) =>
            {
                if (item == _responseTypeItem && index < _codeDescs.Count)
                {
                    _responseTypeItem.Description = _codeDescs[index];
                }
            };

            BackupMenu.OnItemSelect += (s, item, index) =>
            {
                if (item == _responseTypeItem) return;

                int responseCode = _responseTypeItem.Index + 1; // 1 = Code 1, 2 = Code 2, 3 = Code 3

                MenuCore.CloseAll();

                if (item == _btnAmbulance)
                {
                    BackupManager.RequestBackup("Ambulance", responseCode);
                }
                else if (item == _btnLocalPatrol)
                {
                    BackupManager.RequestBackup("Police", responseCode);
                }
                else if (item == _btnFire)
                {
                    BackupManager.RequestBackup("Fire", responseCode);
                }
            };
        }
    }
}