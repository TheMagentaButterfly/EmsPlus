using EmsPlus.Managers;
using Rage;
using Rage.Native;
using RAGENativeUI;
using RAGENativeUI.Elements;
using System;
using System.Collections.Generic;

namespace EmsPlus.UI.Native.ConfigMenu
{
    public static partial class ConfigMenuBuilder
    {
        #region Vehicle Configuration Menu

        private static void BuildVehicleMenu()
        {
            var btnReloadVeh = new UIMenuItem($"{C_HIGHLIGHT}{Localization.Get("ITEM_RELOAD_VEHICLE", "Reload Vehicle")}", Localization.Get("ITEM_RELOAD_VEHICLE_DESC", "Reload the current vehicle configuration"));
            var chkAllowed = new UIMenuCheckboxItem($"{BULLET} {Localization.Get("ITEM_ADD_TO_ALLOWED", "Add to Allowed Vehicles")}", false, Localization.Get("ITEM_ADD_TO_ALLOWED_DESC", "Enable interaction menu for this model."));
            var chkCanHaveStretcher = new UIMenuCheckboxItem($"{BULLET} {Localization.Get("ITEM_CAN_HAVE_STRETCHER", "Can Have Stretcher")}", true, Localization.Get("ITEM_CAN_HAVE_STRETCHER_DESC", "If disabled, acts as rapid response unit."));
            var chkHideStretcher = new UIMenuCheckboxItem($"{BULLET} {Localization.Get("ITEM_HIDE_STRETCHER", "Hide Stretcher in Vehicle")}", false, Localization.Get("ITEM_HIDE_STRETCHER_DESC", "Hides the stretcher when inside."));
            var chkCanEnterCabin = new UIMenuCheckboxItem($"{BULLET} {Localization.Get("ITEM_CAN_ENTER_CABIN", "Has Patient Cabin")}", true, Localization.Get("ITEM_CAN_ENTER_CABIN_DESC", "If disabled, you cannot enter the rear of the ambulance."));
            var interactionPointsMenu = new UIMenu($"{C_HEADER}{Localization.Get("MENU_INTERACTION_POINTS_TITLE", "Interaction Points")}", Localization.Get("MENU_INTERACTION_POINTS_SUBTITLE", "Configure interaction points"));
            MenuCore.AddMenu(interactionPointsMenu);
            var btnInteractionPoints = new UIMenuItem($"{BULLET} {Localization.Get("MENU_INTERACTION_POINTS_TITLE", "Interaction Points")}", Localization.Get("DESC_INTERACTION_POINTS", "Configure interaction points"));

            var listModes = new List<dynamic> {
                $"{C_HEADER}{Localization.Get("MODE_STOWED_POS", "Stowed Position")}",
                $"{C_HEADER}{Localization.Get("MODE_SLIDE_POS", "Slide Position")}",
                $"{C_HEADER}{Localization.Get("MODE_MEDIC_SEAT_POS", "Medic Seat Position")}"
            };
            var itemEditMode = new UIMenuListItem($"{C_HIGHLIGHT}{Localization.Get("ITEM_EDITING_MODE", "Editing Mode")}", listModes, 0, Localization.Get("ITEM_EDITING_MODE_DESC_VEHICLE", "Select the mode to edit"));

            VehiclePosMenu.AddItem(btnReloadVeh);
            VehiclePosMenu.AddItem(chkAllowed);
            VehiclePosMenu.AddItem(chkCanHaveStretcher);
            VehiclePosMenu.AddItem(chkHideStretcher);
            VehiclePosMenu.AddItem(chkCanEnterCabin);
            VehiclePosMenu.AddItem(btnInteractionPoints);
            VehiclePosMenu.BindMenuToItem(interactionPointsMenu, btnInteractionPoints);
            VehiclePosMenu.AddItem(itemEditMode);

            interactionPointsMenu.OnMenuOpen += (s) => RebuildInteractionPointMenu(interactionPointsMenu);

            var doorsMenu = new UIMenu($"{C_HEADER}{Localization.Get("MENU_DOOR_SETUP_TITLE", "Door Setup")}", Localization.Get("MENU_DOOR_SETUP_SUBTITLE", "Configure door settings"));
            MenuCore.AddMenu(doorsMenu);
            var btnDoors = new UIMenuItem($"{BULLET} {Localization.Get("ITEM_CONFIGURE_DOORS", "Configure Doors")}", Localization.Get("ITEM_CONFIGURE_DOORS_DESC", "Configure the doors of the vehicle"));
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
                chkHideStretcher.Checked = AmbulanceManager.CurrentConfig.HideStretcherInVehicle;
                chkCanEnterCabin.Checked = AmbulanceManager.CurrentConfig.CanEnterCabin;

                Vector3 p = getPos();
                Rotator r = getRot();
                MenuHelpers.SyncListItem(sX, p.X, MenuHelpers.FloatValues);
                MenuHelpers.SyncListItem(sY, p.Y, MenuHelpers.FloatValues);
                MenuHelpers.SyncListItem(sZ, p.Z, MenuHelpers.FloatValues);
                MenuHelpers.SyncListItem(sP, r.Pitch, MenuHelpers.DegreeValues);
                MenuHelpers.SyncListItem(sR, r.Roll, MenuHelpers.DegreeValues);
                MenuHelpers.SyncListItem(sYaw, r.Yaw, MenuHelpers.DegreeValues);
            };

            sX = MenuHelpers.AddListControl(VehiclePosMenu, $"{C_HEADER}X {C_INFO}{Localization.Get("LABEL_LEFT_RIGHT", "Left/Right")}", 0f, v => { if (AmbulanceManager.CurrentConfig == null) return; Vector3 p = getPos(); setPos(new Vector3(v, p.Y, p.Z)); refreshGhost(); }, null);
            sY = MenuHelpers.AddListControl(VehiclePosMenu, $"{C_HEADER}Y {C_INFO}{Localization.Get("LABEL_FORWARD_BACK", "Forward/Back")}", 0f, v => { if (AmbulanceManager.CurrentConfig == null) return; Vector3 p = getPos(); setPos(new Vector3(p.X, v, p.Z)); refreshGhost(); }, null);
            sZ = MenuHelpers.AddListControl(VehiclePosMenu, $"{C_HEADER}Z {C_INFO}{Localization.Get("LABEL_UP_DOWN", "Up/Down")}", 0f, v => { if (AmbulanceManager.CurrentConfig == null) return; Vector3 p = getPos(); setPos(new Vector3(p.X, p.Y, v)); refreshGhost(); }, null);
            sP = MenuHelpers.AddDegreeListControl(VehiclePosMenu, $"{C_HEADER}{Localization.Get("LABEL_PITCH", "Pitch")} {C_INFO}{Localization.Get("LABEL_TILT", "Tilt")}", 0f, v => { if (AmbulanceManager.CurrentConfig == null) return; Rotator r = getRot(); setRot(new Rotator(v, r.Roll, r.Yaw)); refreshGhost(); }, null);
            sR = MenuHelpers.AddDegreeListControl(VehiclePosMenu, $"{C_HEADER}{Localization.Get("LABEL_ROLL", "Roll")} {C_INFO}{Localization.Get("LABEL_LEAN", "Lean")}", 0f, v => { if (AmbulanceManager.CurrentConfig == null) return; Rotator r = getRot(); setRot(new Rotator(r.Pitch, v, r.Yaw)); refreshGhost(); }, null);
            sYaw = MenuHelpers.AddDegreeListControl(VehiclePosMenu, $"{C_HEADER}{Localization.Get("LABEL_YAW", "Yaw")} {C_INFO}{Localization.Get("LABEL_ROTATE", "Rotate")}", 0f, v => { if (AmbulanceManager.CurrentConfig == null) return; Rotator r = getRot(); setRot(new Rotator(r.Pitch, r.Roll, v)); refreshGhost(); }, null);

            VehiclePosMenu.OnItemSelect += (s, item, i) =>
            {
                if (item != btnReloadVeh) return;

                if (AmbulanceManager.GetVehicleForConfig(out Vehicle v))
                {
                    Game.DisplayNotification($"{C_SUCCESS}{string.Format(Localization.GetFormat("NOTIF_VEHICLE_DETECTED", "Vehicle detected: {0}"), v.Model.Name)}");
                    GameFiber.StartNew(delegate
                    {
                        GameFiber.Yield();
                        syncVehicleMenu();
                        StretcherGhostManager.UpdateVehicleGhost(v, AmbulanceManager.CurrentConfig, _editingVehicleMode);
                    });
                }
                else
                {
                    Game.DisplayNotification($"{C_WARNING}{string.Format(Localization.GetFormat("NOTIF_NO_VEHICLE_NEARBY", "No vehicle nearby."))}");
                }
            };

            VehiclePosMenu.OnListChange += (s, item, index) =>
            {
                if (item != itemEditMode) return;
                _editingVehicleMode = index;
                syncVehicleMenu();
                refreshGhost();
            };

            VehiclePosMenu.OnMenuOpen += (s) =>
            {
                if (AmbulanceManager.TryGetClosestAmbulance(out Vehicle v))
                {
                    AmbulanceManager.UpdateCurrentConfig(v);
                }
                syncVehicleMenu();
            };
            VehiclePosMenu.OnMenuClose += (s) =>
            {
                StretcherGhostManager.DeleteGhosts();
            };

            chkAllowed.CheckboxEvent += (s, c) =>
            {
                if (AmbulanceManager.CurrentVehicle == null || !AmbulanceManager.CurrentVehicle.Exists()) return;

                string model = AmbulanceManager.CurrentVehicle.Model.Name.ToLower();

                if (c)
                {
                    EntryPoint.EmsPlusConfig.AddAllowedVehicle(model);
                    Game.DisplayNotification($"~g~{string.Format(Localization.GetFormat("NOTIF_VEHICLE_ADDED", "Added {0} to allowed vehicles."), model)}");
                }
                else
                {
                    EntryPoint.EmsPlusConfig.RemoveAllowedVehicle(model);
                    Game.DisplayNotification($"~r~{string.Format(Localization.GetFormat("NOTIF_VEHICLE_REMOVED", "Removed {0} from allowed vehicles."), model)}");
                }
            };

            chkCanHaveStretcher.CheckboxEvent += (s, c) =>
            {
                if (AmbulanceManager.CurrentConfig == null) return;
                AmbulanceManager.CurrentConfig.CanHaveStretcher = c;

                if (AmbulanceManager.CurrentVehicle != null && AmbulanceManager.CurrentVehicle.Exists())
                    StretcherGhostManager.UpdateVehicleGhost(AmbulanceManager.CurrentVehicle, AmbulanceManager.CurrentConfig, _editingVehicleMode);
            };

            chkHideStretcher.CheckboxEvent += (s, c) =>
            {
                if (AmbulanceManager.CurrentConfig == null) return;
                AmbulanceManager.CurrentConfig.HideStretcherInVehicle = c;

                if (AmbulanceManager.IsStretcherLoaded && StretcherManager.Prop != null && StretcherManager.Prop.Exists())
                {
                    if (c)
                    {
                        NativeFunction.Natives.SET_ENTITY_ALPHA(StretcherManager.Prop, 255, false);
                    }
                    else
                    {
                        NativeFunction.Natives.RESET_ENTITY_ALPHA(StretcherManager.Prop);
                    }
                }
            };

            chkCanEnterCabin.CheckboxEvent += (s, c) =>
            {
                if (AmbulanceManager.CurrentConfig == null) return;
                AmbulanceManager.CurrentConfig.CanEnterCabin = c;
            };

            BuildDoorSubMenu(doorsMenu);
        }

        #endregion
    }
}