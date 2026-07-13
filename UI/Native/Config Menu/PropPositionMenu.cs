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
                $"{C_HEADER}{Localization.Get("TRAUMABAG_NAME", "Trauma Bag")}",
                $"{C_HEADER}{Localization.Get("OXYGENBAG_NAME", "Oxygen Bag")}",
                $"{C_HEADER}{Localization.Get("DEFIBRILLATOR_NAME", "Defibrillator")}"
            };

            var itemKit = new UIMenuListItem($"{C_HIGHLIGHT}{Localization.Get("ITEM_SELECT_KIT", "Select Kit")}", listKits, 0, Localization.Get("ITEM_SELECT_KIT_DESC_PROP", "Select the kit to edit"));
            PropPosMenu.AddItem(itemKit);

            var listBones = new List<dynamic>
            {
                Localization.Get("BONE_RIGHT_HAND", "Right Hand"),
                Localization.Get("BONE_LEFT_HAND", "Left Hand"),
                Localization.Get("BONE_BACK", "Back")
            };
            var internalBones = new List<string> { "RightHand", "LeftHand", "Back" };

            var itemBone = new UIMenuListItem($"{C_HEADER}{Localization.Get("LABEL_ATTACH_BONE", "Attach Bone")}", listBones, 0, Localization.Get("DESC_ATTACH_BONE", "Select which body part to attach the bag to."));
            PropPosMenu.AddItem(itemBone);

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

            Func<string> getBone = () =>
            {
                var c = EntryPoint.OffsetConfig;
                if (_editingKitType == "TRAUMABAG") return c.TraumaAttachBone;
                if (_editingKitType == "OXYGENBAG") return c.OxygenAttachBone;
                return c.DefibAttachBone;
            };

            Action<string> setBone = (b) =>
            {
                var c = EntryPoint.OffsetConfig;
                if (_editingKitType == "TRAUMABAG") c.TraumaAttachBone = b;
                else if (_editingKitType == "OXYGENBAG") c.OxygenAttachBone = b;
                else if (_editingKitType == "DEFIBRILLATOR") c.DefibAttachBone = b;
            };

            UIMenuListItem sX = null, sY = null, sZ = null, sP = null, sR = null, sYaw = null;

            Action sync = () =>
            {
                Vector3 p = getPos();
                Rotator r = getRot();
                string currentBone = getBone();
                int boneIndex = internalBones.IndexOf(currentBone);
                itemBone.Index = boneIndex != -1 ? boneIndex : 0;
                MenuHelpers.SyncListItem(sX, p.X, MenuHelpers.FloatValues);
                MenuHelpers.SyncListItem(sY, p.Y, MenuHelpers.FloatValues);
                MenuHelpers.SyncListItem(sZ, p.Z, MenuHelpers.FloatValues);
                MenuHelpers.SyncListItem(sP, r.Pitch, MenuHelpers.DegreeValues);
                MenuHelpers.SyncListItem(sR, r.Roll, MenuHelpers.DegreeValues);
                MenuHelpers.SyncListItem(sYaw, r.Yaw, MenuHelpers.DegreeValues);
                InventoryManager.ReAttachProps();
            };

            sX = MenuHelpers.AddListControl(PropPosMenu, $"{C_HEADER}X {C_INFO}{Localization.Get("LABEL_LEFT_RIGHT", "Left/Right")}", 0f, v => { Vector3 p = getPos(); setPos(new Vector3(v, p.Y, p.Z)); InventoryManager.ReAttachProps(); }, null);
            sY = MenuHelpers.AddListControl(PropPosMenu, $"{C_HEADER}Y {C_INFO}{Localization.Get("LABEL_FORWARD_BACK", "Forward/Back")}", 0f, v => { Vector3 p = getPos(); setPos(new Vector3(p.X, v, p.Z)); InventoryManager.ReAttachProps(); }, null);
            sZ = MenuHelpers.AddListControl(PropPosMenu, $"{C_HEADER}Z {C_INFO}{Localization.Get("LABEL_UP_DOWN", "Up/Down")}", 0f, v => { Vector3 p = getPos(); setPos(new Vector3(p.X, p.Y, v)); InventoryManager.ReAttachProps(); }, null);
            sP = MenuHelpers.AddDegreeListControl(PropPosMenu, $"{C_HEADER}{Localization.Get("LABEL_PITCH", "Pitch")} {C_INFO}{Localization.Get("LABEL_TILT", "Tilt")}", 0f, v => { Rotator r = getRot(); setRot(new Rotator(v, r.Roll, r.Yaw)); InventoryManager.ReAttachProps(); }, null);
            sR = MenuHelpers.AddDegreeListControl(PropPosMenu, $"{C_HEADER}{Localization.Get("LABEL_ROLL", "Roll")} {C_INFO}{Localization.Get("LABEL_LEAN", "Lean")}", 0f, v => { Rotator r = getRot(); setRot(new Rotator(r.Pitch, v, r.Yaw)); InventoryManager.ReAttachProps(); }, null);
            sYaw = MenuHelpers.AddDegreeListControl(PropPosMenu, $"{C_HEADER}{Localization.Get("LABEL_YAW", "Yaw")} {C_INFO}{Localization.Get("LABEL_ROTATE", "Rotate")}", 0f, v => { Rotator r = getRot(); setRot(new Rotator(r.Pitch, r.Roll, v)); InventoryManager.ReAttachProps(); }, null);

            PropPosMenu.OnListChange += (s, item, index) =>
            {
                if (item == itemKit)
                {
                    InventoryManager.StowAllKits();

                    if (index == 0) _editingKitType = "TRAUMABAG";
                    else if (index == 1) _editingKitType = "OXYGENBAG";
                    else if (index == 2) _editingKitType = "DEFIBRILLATOR";

                    InventoryManager.EquipKit(_editingKitType);
                    sync();
                }
                else if (item == itemBone)
                {
                    setBone(internalBones[index]);
                    InventoryManager.ReAttachProps();
                }
            };

            PropPosMenu.OnListChange += (s, item, index) =>
            {
                if (item != itemKit) return;

                InventoryManager.StowAllKits();

                if (index == 0) _editingKitType = "TRAUMABAG";
                else if (index == 1) _editingKitType = "OXYGENBAG";
                else if (index == 2) _editingKitType = "DEFIBRILLATOR";

                InventoryManager.EquipKit(_editingKitType);
                sync();
            };

            PropPosMenu.OnMenuOpen += (s) =>
            {
                InventoryManager.StowAllKits();
                InventoryManager.EquipKit(_editingKitType);
                sync();
            };

            PropPosMenu.OnMenuClose += (s) => InventoryManager.StowAllKits();
        }

        #endregion
    }
}