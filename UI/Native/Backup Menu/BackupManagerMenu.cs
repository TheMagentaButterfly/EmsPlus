using EmsPlus.Core;
using EmsPlus.Managers;
using RAGENativeUI;
using RAGENativeUI.Elements;
using System.Collections.Generic;
using System.Linq;

namespace EmsPlus.UI.Native.BackupMenu
{
    public static class BackupManagerMenuBuilder
    {
        public static UIMenu BackupManagerMenu;
        private static UIMenuListItem _unitList;
        private static UIMenuListItem _patientList;

        public static void Build()
        {
            BackupManagerMenu = new UIMenu(Localization.Get("MENU_COMMAND_COLORED", "~b~Backup Manager"), Localization.Get("SUBTITLE_COMMAND", "~b~Manage AI Resources"));
            MenuCore.AddMenu(BackupManagerMenu);

            BackupManagerMenu.OnMenuOpen += (s) => RefreshMenu();
        }

        public static void RefreshMenu()
        {
            BackupManagerMenu.Clear();

            var idleUnits = BackupManager.ActiveUnits.Where(u => u.State != AIUnitState.Responding).ToList();
            var activePatients = GameState.ActivePatients.Where(p => p.Character != null && p.Character.Exists() && !p.IsDead).ToList();

            if (idleUnits.Count == 0)
            {
                var noUnits = new UIMenuItem("~c~No Units Available", "Request an EMS Backup unit from the Dispatch radio first.");
                noUnits.Enabled = false;
                BackupManagerMenu.AddItem(noUnits);
                BackupManagerMenu.RefreshIndex();
                return;
            }

            List<dynamic> unitNames = idleUnits.Select(u => (dynamic)$"Unit {u.UnitID} ({u.State})").ToList();
            _unitList = new UIMenuListItem(Localization.Get("ITEM_SELECT_UNIT", "Select Unit"), unitNames, 0, Localization.Get("DESC_SELECT_UNIT", "Choose which AI unit to give orders to."));
            BackupManagerMenu.AddItem(_unitList);

            if (activePatients.Count > 0)
            {
                List<dynamic> patientNames = activePatients.Select(p => (dynamic)$"{p.Details.FullName}").ToList();
                _patientList = new UIMenuListItem(Localization.Get("ITEM_SELECT_PATIENT", "Select Patient"), patientNames, 0, Localization.Get("DESC_SELECT_PATIENT", "Choose a patient on scene."));
                BackupManagerMenu.AddItem(_patientList);
            }
            else
            {
                var noPat = new UIMenuItem("~c~No Patients Found", "No valid patients are currently on scene.");
                noPat.Enabled = false;
                BackupManagerMenu.AddItem(noPat);
            }

            var btnTreat = new UIMenuItem(Localization.Get("ACT_DELEGATE_TREATMENT", "~y~Order: Delegate Treatment"), Localization.Get("DESC_DELEGATE_TREATMENT", "Unit will approach the patient and stabilize their vitals."));
            var selectedUnit = idleUnits[_unitList.Index];
            string transLabel = (selectedUnit?.State == AIUnitState.LoadedOnScene)
                ? Localization.Get("ACT_DRIVE_TO_HOSPITAL", "~o~Order: Drive to Hospital")
                : Localization.Get("ACT_ORDER_TRANSPORT", "~o~Order: Transport Patient");

            var btnTransport = new UIMenuItem(transLabel, Localization.Get("DESC_ORDER_TRANSPORT", "Unit will load the patient and transport them to the hospital."));

            var btnLoadOnly = new UIMenuItem(Localization.Get("ACT_ORDER_LOAD_ONLY", "~p~Order: Load Patient Only"), "Load patient into ambulance but stay on scene.");
            var btnDismiss = new UIMenuItem(Localization.Get("ACT_DISMISS_UNIT", "~r~Dismiss Unit"), Localization.Get("DESC_DISMISS_UNIT", "Unit will return to their ambulance and leave the scene."));

            if (activePatients.Count == 0 && selectedUnit?.State != AIUnitState.LoadedOnScene)
            {
                btnTreat.Enabled = false;
                btnTransport.Enabled = false;
                btnLoadOnly.Enabled = false;
            }


            BackupManagerMenu.AddItem(btnTreat);
            BackupManagerMenu.AddItem(btnLoadOnly);
            BackupManagerMenu.AddItem(btnTransport);
            BackupManagerMenu.AddItem(btnDismiss);

            BackupManagerMenu.OnItemSelect += (s, item, index) =>
            {
                if (idleUnits.Count == 0) return;

                var selectedPatient = activePatients.Count > 0 ? activePatients[_patientList.Index] : null;

                if (item == btnTreat && selectedPatient != null)
                {
                    MenuCore.CloseAll();
                    BackupManager.OrderTreatment(selectedUnit, selectedPatient);
                }
                else if (item == btnLoadOnly && selectedPatient != null)
                {
                    MenuCore.CloseAll();
                    BackupManager.OrderLoadOnly(selectedUnit, selectedPatient);
                }
                else if (item == btnTransport)
                {
                    MenuCore.CloseAll();
                    BackupManager.OrderTransport(selectedUnit, selectedPatient);
                }
                else if (item == btnDismiss)
                {
                    MenuCore.CloseAll();
                    BackupManager.DismissUnit(selectedUnit);
                }
            };

            BackupManagerMenu.RefreshIndex();
        }
    }
}