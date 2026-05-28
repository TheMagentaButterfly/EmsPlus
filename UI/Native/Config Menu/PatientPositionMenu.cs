using EmsPlus.Managers;
using Rage;
using RAGENativeUI.Elements;
using System;
using System.Collections.Generic;

namespace EmsPlus.UI.Native.ConfigMenu
{
    public static partial class ConfigMenuBuilder
    {
        #region Patient Position Menu

        private static void BuildPatientMenu()
        {
            var listModes = new List<dynamic>
            {
                $"{C_HEADER}{Localization.Get("MODE_NORMAL_STRETCHER", "Normal Stretcher")}",
                $"{C_HEADER}{Localization.Get("MODE_LOWERED_STRETCHER", "Lowered Stretcher")}",
                $"{C_HEADER}{Localization.Get("MODE_SITTING_STRETCHER", "Sitting Stretcher")}"
            };
            var itemMode = new UIMenuListItem($"{C_HIGHLIGHT}{Localization.Get("ITEM_EDITING_MODE", "Editing Mode")}", listModes, 0, Localization.Get("ITEM_EDITING_MODE_DESC_PATIENT", "Select the mode for editing patient positions"));
            PatientPosMenu.AddItem(itemMode);

            Func<Vector3> getPos = () =>
            {
                if (_editingPatientMode == 2) return new Vector3(EntryPoint.OffsetConfig.PatientAttachSittingOffsetX, EntryPoint.OffsetConfig.PatientAttachSittingOffsetY, EntryPoint.OffsetConfig.PatientAttachSittingOffsetZ);
                if (_editingPatientMode == 1) return new Vector3(EntryPoint.OffsetConfig.PatientAttachLoweredOffsetX, EntryPoint.OffsetConfig.PatientAttachLoweredOffsetY, EntryPoint.OffsetConfig.PatientAttachLoweredOffsetZ);
                return new Vector3(EntryPoint.OffsetConfig.PatientAttachOffsetX, EntryPoint.OffsetConfig.PatientAttachOffsetY, EntryPoint.OffsetConfig.PatientAttachOffsetZ);
            };

            Action<Vector3> setPos = (v) =>
            {
                if (_editingPatientMode == 2) { EntryPoint.OffsetConfig.PatientAttachSittingOffsetX = v.X; EntryPoint.OffsetConfig.PatientAttachSittingOffsetY = v.Y; EntryPoint.OffsetConfig.PatientAttachSittingOffsetZ = v.Z; }
                else if (_editingPatientMode == 1) { EntryPoint.OffsetConfig.PatientAttachLoweredOffsetX = v.X; EntryPoint.OffsetConfig.PatientAttachLoweredOffsetY = v.Y; EntryPoint.OffsetConfig.PatientAttachLoweredOffsetZ = v.Z; }
                else { EntryPoint.OffsetConfig.PatientAttachOffsetX = v.X; EntryPoint.OffsetConfig.PatientAttachOffsetY = v.Y; EntryPoint.OffsetConfig.PatientAttachOffsetZ = v.Z; }
            };

            Func<Rotator> getRot = () =>
            {
                if (_editingPatientMode == 2) return new Rotator(EntryPoint.OffsetConfig.PatientAttachSittingPitch, EntryPoint.OffsetConfig.PatientAttachSittingRoll, EntryPoint.OffsetConfig.PatientAttachSittingYaw);
                if (_editingPatientMode == 1) return new Rotator(EntryPoint.OffsetConfig.PatientAttachLoweredPitch, EntryPoint.OffsetConfig.PatientAttachLoweredRoll, EntryPoint.OffsetConfig.PatientAttachLoweredYaw);
                return new Rotator(EntryPoint.OffsetConfig.PatientAttachPitch, EntryPoint.OffsetConfig.PatientAttachRoll, EntryPoint.OffsetConfig.PatientAttachYaw);
            };

            Action<Rotator> setRot = (r) =>
            {
                if (_editingPatientMode == 2) { EntryPoint.OffsetConfig.PatientAttachSittingPitch = r.Pitch; EntryPoint.OffsetConfig.PatientAttachSittingRoll = r.Roll; EntryPoint.OffsetConfig.PatientAttachSittingYaw = r.Yaw; }
                else if (_editingPatientMode == 1) { EntryPoint.OffsetConfig.PatientAttachLoweredPitch = r.Pitch; EntryPoint.OffsetConfig.PatientAttachLoweredRoll = r.Roll; EntryPoint.OffsetConfig.PatientAttachLoweredYaw = r.Yaw; }
                else { EntryPoint.OffsetConfig.PatientAttachPitch = r.Pitch; EntryPoint.OffsetConfig.PatientAttachRoll = r.Roll; EntryPoint.OffsetConfig.PatientAttachYaw = r.Yaw; }
            };

            UIMenuListItem pX = null, pY = null, pZ = null, pP = null, pR = null, pYaw = null;

            Action syncMenu = () =>
            {
                Vector3 p = getPos();
                Rotator r = getRot();
                MenuHelpers.SyncListItem(pX, p.X, MenuHelpers.FloatValues);
                MenuHelpers.SyncListItem(pY, p.Y, MenuHelpers.FloatValues);
                MenuHelpers.SyncListItem(pZ, p.Z, MenuHelpers.FloatValues);
                MenuHelpers.SyncListItem(pP, r.Pitch, MenuHelpers.DegreeValues);
                MenuHelpers.SyncListItem(pR, r.Roll, MenuHelpers.DegreeValues);
                MenuHelpers.SyncListItem(pYaw, r.Yaw, MenuHelpers.DegreeValues);
                StretcherGhostManager.UpdatePatientGhost(_editingPatientMode);
            };

            pX = MenuHelpers.AddListControl(PatientPosMenu, $"{C_HEADER}X {C_INFO}{Localization.Get("LABEL_LEFT_RIGHT", "Left/Right")}", 0f, v => { Vector3 p = getPos(); setPos(new Vector3(v, p.Y, p.Z)); StretcherGhostManager.UpdatePatientGhost(_editingPatientMode); }, null);
            pY = MenuHelpers.AddListControl(PatientPosMenu, $"{C_HEADER}Y {C_INFO}{Localization.Get("LABEL_FORWARD_BACK", "Forward/Back")}", 0f, v => { Vector3 p = getPos(); setPos(new Vector3(p.X, v, p.Z)); StretcherGhostManager.UpdatePatientGhost(_editingPatientMode); }, null);
            pZ = MenuHelpers.AddListControl(PatientPosMenu, $"{C_HEADER}Z {C_INFO}{Localization.Get("LABEL_UP_DOWN", "Up/Down")}", 0f, v => { Vector3 p = getPos(); setPos(new Vector3(p.X, p.Y, v)); StretcherGhostManager.UpdatePatientGhost(_editingPatientMode); }, null);
            pP = MenuHelpers.AddDegreeListControl(PatientPosMenu, $"{C_HEADER}{Localization.Get("LABEL_PITCH", "Pitch")} {C_INFO}{Localization.Get("LABEL_TILT", "Tilt")}", 0f, v => { Rotator r = getRot(); setRot(new Rotator(v, r.Roll, r.Yaw)); StretcherGhostManager.UpdatePatientGhost(_editingPatientMode); }, null);
            pR = MenuHelpers.AddDegreeListControl(PatientPosMenu, $"{C_HEADER}{Localization.Get("LABEL_ROLL", "Roll")} {C_INFO}{Localization.Get("LABEL_LEAN", "Lean")}", 0f, v => { Rotator r = getRot(); setRot(new Rotator(r.Pitch, v, r.Yaw)); StretcherGhostManager.UpdatePatientGhost(_editingPatientMode); }, null);
            pYaw = MenuHelpers.AddDegreeListControl(PatientPosMenu, $"{C_HEADER}{Localization.Get("LABEL_YAW", "Yaw")} {C_INFO}{Localization.Get("LABEL_ROTATE", "Rotate")}", 0f, v => { Rotator r = getRot(); setRot(new Rotator(r.Pitch, r.Roll, v)); StretcherGhostManager.UpdatePatientGhost(_editingPatientMode); }, null);

            PatientPosMenu.OnListChange += (sender, item, index) =>
            {
                if (item != itemMode) return;
                _editingPatientMode = index;
                syncMenu();
            };

            PatientPosMenu.OnMenuOpen += (s) => syncMenu();
            PatientPosMenu.OnMenuClose += (s) => StretcherGhostManager.DeleteGhosts();
        }

        #endregion
    }
}