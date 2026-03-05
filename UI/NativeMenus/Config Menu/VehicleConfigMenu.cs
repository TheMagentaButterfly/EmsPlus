using Rage;
using EmsPlus.Managers;
using RAGENativeUI;
using RAGENativeUI.Elements;
using System;
using System.Collections.Generic;

namespace EmsPlus.UI.NativeMenus.ConfigMenu
{
    public static partial class ConfigMenuBuilder
    {
        #region Vehicle Configuration Menu

        private static void BuildVehicleMenu()
        {
            var btnReloadVeh = new UIMenuItem($"{C_HIGHLIGHT}{Localization.Get("ITEM_RELOAD_VEHICLE")}", Localization.Get("ITEM_RELOAD_VEHICLE_DESC"));
            var chkAllowed = new UIMenuCheckboxItem($"{BULLET} {Localization.Get("ITEM_ADD_TO_ALLOWED") ?? "Add to Allowed Vehicles"}", false, Localization.Get("ITEM_ADD_TO_ALLOWED_DESC") ?? "Enable interaction menu for this model.");
            var chkCanHaveStretcher = new UIMenuCheckboxItem($"{BULLET} {Localization.Get("ITEM_CAN_HAVE_STRETCHER") ?? "Can Have Stretcher"}", true, Localization.Get("ITEM_CAN_HAVE_STRETCHER_DESC") ?? "If disabled, acts as rapid response unit.");

            var interactionPointsMenu = new UIMenu($"{C_HEADER}{Localization.Get("MENU_INTERACTION_POINTS_TITLE")}", Localization.Get("MENU_INTERACTION_POINTS_SUBTITLE"));
            MenuCore.AddMenu(interactionPointsMenu);
            var btnInteractionPoints = new UIMenuItem($"{BULLET} {Localization.Get("MENU_INTERACTION_POINTS_TITLE")}", Localization.Get("DESC_INTERACTION_POINTS"));

            var listModes = new List<dynamic> {
                $"{C_HEADER}{Localization.Get("MODE_STOWED_POS")}",
                $"{C_HEADER}{Localization.Get("MODE_SLIDE_POS")}",
                $"{C_HEADER}{Localization.Get("MODE_MEDIC_SEAT_POS")}"
            };
            var itemEditMode = new UIMenuListItem($"{C_HIGHLIGHT}{Localization.Get("ITEM_EDITING_MODE")}", listModes, 0, Localization.Get("ITEM_EDITING_MODE_DESC_VEHICLE"));

            VehiclePosMenu.AddItem(btnReloadVeh);
            VehiclePosMenu.AddItem(chkAllowed);
            VehiclePosMenu.AddItem(chkCanHaveStretcher);
            VehiclePosMenu.AddItem(btnInteractionPoints);
            VehiclePosMenu.BindMenuToItem(interactionPointsMenu, btnInteractionPoints);
            VehiclePosMenu.AddItem(itemEditMode);

            interactionPointsMenu.OnMenuOpen += (s) => RebuildInteractionPointMenu(interactionPointsMenu);

            var doorsMenu = new UIMenu($"{C_HEADER}{Localization.Get("MENU_DOOR_SETUP_TITLE")}", Localization.Get("MENU_DOOR_SETUP_SUBTITLE"));
            MenuCore.AddMenu(doorsMenu);
            var btnDoors = new UIMenuItem($"{BULLET} {Localization.Get("ITEM_CONFIGURE_DOORS")}", Localization.Get("ITEM_CONFIGURE_DOORS_DESC"));
            VehiclePosMenu.AddItem(btnDoors);
            VehiclePosMenu.BindMenuToItem(doorsMenu, btnDoors);

            Func<Vector3> getPos = () => {
                if (_editingVehicleMode == 2) return AmbulanceManager.CurrentConfig.MedicPos;
                return _editingVehicleMode == 1 ? AmbulanceManager.CurrentConfig.SlidePos : AmbulanceManager.CurrentConfig.StowPos;
            };
            Action<Vector3> setPos = (v) => {
                if (_editingVehicleMode == 2) AmbulanceManager.CurrentConfig.MedicPos = v;
                else if (_editingVehicleMode == 1) AmbulanceManager.CurrentConfig.SlidePos = v;
                else AmbulanceManager.CurrentConfig.StowPos = v;
            };
            Func<Rotator> getRot = () => {
                if (_editingVehicleMode == 2) return AmbulanceManager.CurrentConfig.MedicRot;
                return _editingVehicleMode == 1 ? AmbulanceManager.CurrentConfig.SlideRot : AmbulanceManager.CurrentConfig.StowRot;
            };
            Action<Rotator> setRot = (r) => {
                if (_editingVehicleMode == 2) AmbulanceManager.CurrentConfig.MedicRot = r;
                else if (_editingVehicleMode == 1) AmbulanceManager.CurrentConfig.SlideRot = r;
                else AmbulanceManager.CurrentConfig.StowRot = r;
            };

            UIMenuListItem sX = null, sY = null, sZ = null, sP = null, sR = null, sYaw = null;

            Action refreshGhost = () =>
            {
                if (AmbulanceManager.CurrentVehicle != null && AmbulanceManager.CurrentVehicle.Exists())
                    StretcherGhostManager.UpdateVehicleGhost(AmbulanceManager.CurrentVehicle, AmbulanceManager.CurrentConfig, _editingVehicleMode);
            };

            Action syncVehicleMenu = () =>
            {
                if (AmbulanceManager.CurrentConfig == null) return;

                chkAllowed.Checked = EntryPoint.EmsPlusConfig.IsAllowed(AmbulanceManager.CurrentConfig.ModelName);
                chkCanHaveStretcher.Checked = AmbulanceManager.CurrentConfig.CanHaveStretcher;

                Vector3 p = getPos();
                Rotator r = getRot();
                MenuHelpers.SyncListItem(sX, p.X, MenuHelpers.FloatValues);
                MenuHelpers.SyncListItem(sY, p.Y, MenuHelpers.FloatValues);
                MenuHelpers.SyncListItem(sZ, p.Z, MenuHelpers.FloatValues);
                MenuHelpers.SyncListItem(sP, r.Pitch, MenuHelpers.DegreeValues);
                MenuHelpers.SyncListItem(sR, r.Roll, MenuHelpers.DegreeValues);
                MenuHelpers.SyncListItem(sYaw, r.Yaw, MenuHelpers.DegreeValues);
            };

            sX = MenuHelpers.AddListControl(VehiclePosMenu, $"{C_HEADER}X {C_INFO}{Localization.Get("LABEL_LEFT_RIGHT")}", 0f, v => { if (AmbulanceManager.CurrentConfig == null) return; Vector3 p = getPos(); setPos(new Vector3(v, p.Y, p.Z)); refreshGhost(); }, null);
            sY = MenuHelpers.AddListControl(VehiclePosMenu, $"{C_HEADER}Y {C_INFO}{Localization.Get("LABEL_FORWARD_BACK")}", 0f, v => { if (AmbulanceManager.CurrentConfig == null) return; Vector3 p = getPos(); setPos(new Vector3(p.X, v, p.Z)); refreshGhost(); }, null);
            sZ = MenuHelpers.AddListControl(VehiclePosMenu, $"{C_HEADER}Z {C_INFO}{Localization.Get("LABEL_UP_DOWN")}", 0f, v => { if (AmbulanceManager.CurrentConfig == null) return; Vector3 p = getPos(); setPos(new Vector3(p.X, p.Y, v)); refreshGhost(); }, null);
            sP = MenuHelpers.AddDegreeListControl(VehiclePosMenu, $"{C_HEADER}{Localization.Get("LABEL_PITCH")} {C_INFO}{Localization.Get("LABEL_TILT")}", 0f, v => { if (AmbulanceManager.CurrentConfig == null) return; Rotator r = getRot(); setRot(new Rotator(v, r.Roll, r.Yaw)); refreshGhost(); }, null);
            sR = MenuHelpers.AddDegreeListControl(VehiclePosMenu, $"{C_HEADER}{Localization.Get("LABEL_ROLL")} {C_INFO}{Localization.Get("LABEL_LEAN")}", 0f, v => { if (AmbulanceManager.CurrentConfig == null) return; Rotator r = getRot(); setRot(new Rotator(r.Pitch, v, r.Yaw)); refreshGhost(); }, null);
            sYaw = MenuHelpers.AddDegreeListControl(VehiclePosMenu, $"{C_HEADER}{Localization.Get("LABEL_YAW")} {C_INFO}{Localization.Get("LABEL_ROTATE")}", 0f, v => { if (AmbulanceManager.CurrentConfig == null) return; Rotator r = getRot(); setRot(new Rotator(r.Pitch, r.Roll, v)); refreshGhost(); }, null);

            VehiclePosMenu.OnItemSelect += (s, item, i) =>
            {
                if (item != btnReloadVeh) return;

                if (AmbulanceManager.GetVehicleForConfig(out Vehicle v))
                {
                    Game.DisplayNotification($"{C_SUCCESS}{Localization.Get("NOTIF_VEHICLE_DETECTED", v.Model.Name)}");
                    GameFiber.StartNew(delegate
                    {
                        GameFiber.Yield();
                        syncVehicleMenu();
                        StretcherGhostManager.UpdateVehicleGhost(v, AmbulanceManager.CurrentConfig, _editingVehicleMode);
                    });
                }
                else
                {
                    Game.DisplayNotification($"{C_WARNING}{Localization.Get("NOTIF_NO_AMBULANCE_NEARBY")}");
                }
            };

            VehiclePosMenu.OnListChange += (s, item, index) =>
            {
                if (item != itemEditMode) return;
                _editingVehicleMode = index;
                syncVehicleMenu();
                refreshGhost();
            };

            VehiclePosMenu.OnMenuOpen += (s) => syncVehicleMenu();
            VehiclePosMenu.OnMenuClose += (s) => StretcherGhostManager.DeleteGhosts();

            chkAllowed.CheckboxEvent += (s, c) =>
            {
                if (AmbulanceManager.CurrentConfig == null) return;

                string model = AmbulanceManager.CurrentConfig.ModelName;
                if (c)
                {
                    EntryPoint.EmsPlusConfig.AddAllowedVehicle(model);
                    Game.DisplayNotification($"{C_SUCCESS}{Localization.Get("NOTIF_VEHICLE_ADDED", model)}");
                    AmbulanceManager.CurrentConfig.Save();
                }
                else
                {
                    EntryPoint.EmsPlusConfig.RemoveAllowedVehicle(model);
                    Game.DisplayNotification($"{C_WARNING}{Localization.Get("NOTIF_VEHICLE_REMOVED", model)}");
                }
            };

            chkCanHaveStretcher.CheckboxEvent += (s, c) =>
            {
                if (AmbulanceManager.CurrentConfig == null) return;
                AmbulanceManager.CurrentConfig.CanHaveStretcher = c;

                if (AmbulanceManager.CurrentVehicle != null && AmbulanceManager.CurrentVehicle.Exists())
                    StretcherGhostManager.UpdateVehicleGhost(AmbulanceManager.CurrentVehicle, AmbulanceManager.CurrentConfig, _editingVehicleMode);
            };

            BuildDoorSubMenu(doorsMenu);
        }

        #endregion
    }
}