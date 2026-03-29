using EmsPlus.Managers;
using Rage;
using RAGENativeUI.Elements;
using System;
using System.Collections.Generic;

namespace EmsPlus.UI.Native.ConfigMenu
{
    public static partial class ConfigMenuBuilder
    {
        #region Prop Position Menu

        private static void BuildPropPosMenu()
        {
            var listKits = new List<dynamic>
            {
                $"{C_HEADER}{Localization.Get("TRAUMABAG_NAME")}",
                $"{C_HEADER}{Localization.Get("OXYGENBAG_NAME")}",
                $"{C_HEADER}{Localization.Get("DEFIBRILLATOR_NAME")}"
            };

            var itemKit = new UIMenuListItem($"{C_HIGHLIGHT}{Localization.Get("ITEM_SELECT_KIT")}", listKits, 0, Localization.Get("ITEM_SELECT_KIT_DESC_PROP"));
            PropPosMenu.AddItem(itemKit);

            Func<Vector3> getPos = () =>
            {
                var c = EntryPoint.OffsetConfig;
                if (_editingKitType == "TRAUMABAG") return new Vector3(c.TraumaAttachX, c.TraumaAttachY, c.TraumaAttachZ);
                if (_editingKitType == "OXYGENBAG") return new Vector3(c.OxygenAttachX, c.OxygenAttachY, c.OxygenAttachZ);
                return new Vector3(c.DefibAttachX, c.DefibAttachY, c.DefibAttachZ);
            };

            Action<Vector3> setPos = (v) =>
            {
                var c = EntryPoint.OffsetConfig;
                if (_editingKitType == "TRAUMABAG") { c.TraumaAttachX = v.X; c.TraumaAttachY = v.Y; c.TraumaAttachZ = v.Z; }
                else if (_editingKitType == "OXYGENBAG") { c.OxygenAttachX = v.X; c.OxygenAttachY = v.Y; c.OxygenAttachZ = v.Z; }
                else if (_editingKitType == "DEFIBRILLATOR") { c.DefibAttachX = v.X; c.DefibAttachY = v.Y; c.DefibAttachZ = v.Z; }
            };

            Func<Rotator> getRot = () =>
            {
                var c = EntryPoint.OffsetConfig;
                if (_editingKitType == "TRAUMABAG") return new Rotator(c.TraumaAttachPitch, c.TraumaAttachRoll, c.TraumaAttachYaw);
                if (_editingKitType == "OXYGENBAG") return new Rotator(c.OxygenAttachPitch, c.OxygenAttachRoll, c.OxygenAttachYaw);
                return new Rotator(c.DefibAttachPitch, c.DefibAttachRoll, c.DefibAttachYaw);
            };

            Action<Rotator> setRot = (r) =>
            {
                var c = EntryPoint.OffsetConfig;
                if (_editingKitType == "TRAUMABAG") { c.TraumaAttachPitch = r.Pitch; c.TraumaAttachRoll = r.Roll; c.TraumaAttachYaw = r.Yaw; }
                else if (_editingKitType == "OXYGENBAG") { c.OxygenAttachPitch = r.Pitch; c.OxygenAttachRoll = r.Roll; c.OxygenAttachYaw = r.Yaw; }
                else if (_editingKitType == "DEFIBRILLATOR") { c.DefibAttachPitch = r.Pitch; c.DefibAttachRoll = r.Roll; c.DefibAttachYaw = r.Yaw; }
            };

            UIMenuListItem sX = null, sY = null, sZ = null, sP = null, sR = null, sYaw = null;

            Action sync = () =>
            {
                Vector3 p = getPos();
                Rotator r = getRot();
                MenuHelpers.SyncListItem(sX, p.X, MenuHelpers.FloatValues);
                MenuHelpers.SyncListItem(sY, p.Y, MenuHelpers.FloatValues);
                MenuHelpers.SyncListItem(sZ, p.Z, MenuHelpers.FloatValues);
                MenuHelpers.SyncListItem(sP, r.Pitch, MenuHelpers.DegreeValues);
                MenuHelpers.SyncListItem(sR, r.Roll, MenuHelpers.DegreeValues);
                MenuHelpers.SyncListItem(sYaw, r.Yaw, MenuHelpers.DegreeValues);
                InventoryManager.ReAttachProp();
            };

            sX = MenuHelpers.AddListControl(PropPosMenu, $"{C_HEADER}X {C_INFO}{Localization.Get("LABEL_LEFT_RIGHT")}", 0f, v => { Vector3 p = getPos(); setPos(new Vector3(v, p.Y, p.Z)); InventoryManager.ReAttachProp(); }, null);
            sY = MenuHelpers.AddListControl(PropPosMenu, $"{C_HEADER}Y {C_INFO}{Localization.Get("LABEL_FORWARD_BACK")}", 0f, v => { Vector3 p = getPos(); setPos(new Vector3(p.X, v, p.Z)); InventoryManager.ReAttachProp(); }, null);
            sZ = MenuHelpers.AddListControl(PropPosMenu, $"{C_HEADER}Z {C_INFO}{Localization.Get("LABEL_UP_DOWN")}", 0f, v => { Vector3 p = getPos(); setPos(new Vector3(p.X, p.Y, v)); InventoryManager.ReAttachProp(); }, null);
            sP = MenuHelpers.AddDegreeListControl(PropPosMenu, $"{C_HEADER}{Localization.Get("LABEL_PITCH")} {C_INFO}{Localization.Get("LABEL_TILT")}", 0f, v => { Rotator r = getRot(); setRot(new Rotator(v, r.Roll, r.Yaw)); InventoryManager.ReAttachProp(); }, null);
            sR = MenuHelpers.AddDegreeListControl(PropPosMenu, $"{C_HEADER}{Localization.Get("LABEL_ROLL")} {C_INFO}{Localization.Get("LABEL_LEAN")}", 0f, v => { Rotator r = getRot(); setRot(new Rotator(r.Pitch, v, r.Yaw)); InventoryManager.ReAttachProp(); }, null);
            sYaw = MenuHelpers.AddDegreeListControl(PropPosMenu, $"{C_HEADER}{Localization.Get("LABEL_YAW")} {C_INFO}{Localization.Get("LABEL_ROTATE")}", 0f, v => { Rotator r = getRot(); setRot(new Rotator(r.Pitch, r.Roll, v)); InventoryManager.ReAttachProp(); }, null);

            PropPosMenu.OnListChange += (s, item, index) =>
            {
                if (item != itemKit) return;

                if (index == 0) _editingKitType = "TRAUMABAG";
                else if (index == 1) _editingKitType = "OXYGENBAG";
                else if (index == 2) _editingKitType = "DEFIBRILLATOR";

                InventoryManager.EquipKit(_editingKitType);
                sync();
            };

            PropPosMenu.OnMenuOpen += (s) =>
            {
                if (!InventoryManager.CurrentKitID.Equals(_editingKitType))
                    InventoryManager.EquipKit(_editingKitType);
                sync();
            };

            PropPosMenu.OnMenuClose += (s) => InventoryManager.StowKit();
        }

        #endregion
    }
}