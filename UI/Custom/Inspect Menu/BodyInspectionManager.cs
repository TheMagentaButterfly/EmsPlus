using EmsPlus.Core;
using EmsPlus.Managers;
using EmsPlus.Medical;
using EmsPlus.UI.Tasks;
using EmsPlus.UI.Helpers;
using EmsPlus.UI.Native;
using Rage;
using Rage.Native;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using MainMenu = EmsPlus.UI.Custom.InspectMenu.Menus.MainMenu;
using EmsPlus.UI.Custom.InspectMenu.Menus;

namespace EmsPlus.UI.Custom.InspectMenu
{
    public static class BodyInspectionManager
    {
        private enum CameraMode { TopDown, Standing }
        private static CameraMode _currentCamMode;

        private static float _orbitYaw = 0f;
        private static float _orbitPitch = 0f;
        private static float _orbitRadius = 1.8f;
        private const float MouseSensitivity = 1.5f;
        private static bool _isRotatingCamera = false;
        private static Point _preRotationCursorPos;
        private static bool _hasSavedState = false;
        private static float _savedRelativeYaw = 180f;
        private static float _savedPitch = 20f;
        private static float _savedRadius = 1.8f;

        private static Camera _cam;
        private static bool _active;
        public static bool IsActive => _active;
        private static Ped _patient;
        private static GameFiber _logicFiber;

        private static InspectionState _state;
        private static InspectionRenderer _renderer;
        private static InspectionInput _input;
        private static BodyPartManager _bodyParts;
        private static DiagnosticManager _diagnostics;

        public static string CurrentMenuCategory = "MAIN";

        public static List<InspectionAction> CurrentPanelActions { get; private set; } = new List<InspectionAction>();

        public static void StartInspection(Ped patient)
        {
            if (_active) return;

            try
            {
                var screenBounds = Screen.PrimaryScreen.Bounds;
                Cursor.Position = new Point(screenBounds.Width / 2, screenBounds.Height / 2);
            }
            catch { }

            InventoryManager.PlaceKitOnGround(patient);

            _patient = patient;
            _active = true;

            CurrentMenuCategory = "MAIN";

            _state = new InspectionState();
            _state.ToggleDiagnostics();

            if (!AmbulanceManager.IsPlayerInRearCabin)
            {
                NativeFunction.Natives.TASK_TURN_PED_TO_FACE_ENTITY(Game.LocalPlayer.Character, patient, 1000);
            }

            if (patient.IsOnFoot && (GameState.CurrentPatient.Consciousness == ConsciousnessLevel.Verbal || GameState.CurrentPatient.Consciousness == ConsciousnessLevel.Alert))
                _currentCamMode = CameraMode.Standing;
            else
                _currentCamMode = CameraMode.TopDown;

            if (_hasSavedState)
            {
                _orbitYaw = patient.Heading + _savedRelativeYaw;
                _orbitPitch = _savedPitch;
                _orbitRadius = _savedRadius;
            }
            else
            {
                if (_currentCamMode == CameraMode.Standing)
                {
                    _orbitPitch = 10f; _orbitRadius = 1.5f; _orbitYaw = patient.Heading + 180f;
                }
                else
                {
                    _orbitPitch = 60f;
                    _orbitRadius = 2.0f;
                }
                _orbitYaw = patient.Heading + 180f;
            }

            if (AmbulanceManager.IsPlayerInRearCabin)
            {
                _orbitRadius = 1.1f;
                if (_orbitPitch < 40f) _orbitPitch = 50f;
            }

            _input = new InspectionInput();
            _bodyParts = new BodyPartManager(patient);
            _diagnostics = new DiagnosticManager();
            _renderer = new InspectionRenderer();

            _cam = new Camera(false);
            UpdateCameraPosition();
            _cam.Active = true;
            NativeFunction.Natives.RENDER_SCRIPT_CAMS(true, false, 0, true, true, 0);
            MenuCore.CloseAll();
            GameState.SuppressPrompts = true;

            _logicFiber = GameFiber.StartNew(ProcessLogic);
            TutorialManager.TriggerInspectionTutorial();
            Game.FrameRender += OnRender;
        }

        public static void StopInspection(bool restoreMenu = true)
        {
            if (!_active) return;
            _active = false;

            InventoryManager.ActiveTool = EmsTreatment.None;

            if (_patient != null && _patient.Exists())
            {
                _hasSavedState = true;
                _savedRelativeYaw = _orbitYaw - _patient.Heading;
                _savedPitch = _orbitPitch;
                _savedRadius = _orbitRadius;
            }

            Game.FrameRender -= OnRender;
            if (_cam?.IsValid() == true) { _cam.Active = false; _cam.Delete(); }
            GameState.SuppressPrompts = false;
            NativeFunction.Natives.RENDER_SCRIPT_CAMS(false, false, 0, true, true, 0);
            _patient = null;

            NativeFunction.Natives.SET_MOUSE_CURSOR_VISIBLE(false);

            GameFiber.StartNew(delegate {
                uint timeout = Game.GameTime + 250;
                while (Game.GameTime < timeout)
                {
                    GameFiber.Yield();
                    if (NativeFunction.Natives.IS_PAUSE_MENU_ACTIVE<bool>()) NativeFunction.Natives.SET_PAUSE_MENU_ACTIVE(false);
                    NativeFunction.Natives.DISABLE_CONTROL_ACTION(0, 199, true);
                    NativeFunction.Natives.DISABLE_CONTROL_ACTION(0, 200, true);
                }
                if (restoreMenu && MenuCore.MainMenu != null) MenuCore.MainMenu.Visible = true;
            });
        }

        public static void RefreshActions()
        {
            CurrentPanelActions.Clear();
            if (_state != null) _state.ActionScrollIndex = 0;

            var part = _bodyParts.SelectedPart;
            var p = GameState.CurrentPatient;
            if (part == null || p == null) return;

            if (part.LinkedEntity != null)
            {
                KitMenu.Build(part, p);
                return;
            }

            if (CurrentMenuCategory != "MAIN" && !CurrentMenuCategory.StartsWith("KIT_"))
            {
                CurrentPanelActions.Add(new InspectionAction(
                    Localization.Get("BTN_BACK"), Localization.Get("BTN_BACK_DESC"), Color.FromArgb(255, 80, 80, 80), true,
                    () => { CurrentMenuCategory = "MAIN"; RefreshActions(); }
                ));
            }

            switch (CurrentMenuCategory)
            {
                case "MAIN":
                    MainMenu.Build(part, p);
                    break;
                case "AIRWAY":
                    TreatmentMenu.BuildAirway(part, p);
                    break;
                case "ORAL":
                    TreatmentMenu.BuildOral(part, p);
                    break;
                case "IV":
                    TreatmentMenu.BuildIV(part, p);
                    break;
                case "IM":
                    TreatmentMenu.BuildIM(part, p);
                    break;
                case "TREATMENTS":
                    TreatmentMenu.BuildTreatments(part, p);
                    break;
                case "KIT_DIAGNOSTICS":
                    KitMenu.BuildDiagnostics(part, p);
                    break;
            }
        }

        private static void UpdateCameraPosition()
        {
            if (_cam == null || _patient == null || !_patient.Exists()) return;

            Vector3 targetPos;
            if (_currentCamMode == CameraMode.Standing)
            {
                targetPos = _patient.GetBonePosition(PedBoneId.Spine2);
                targetPos.Z += 0.1f;
            }
            else
            {
                targetPos = _patient.GetBonePosition(PedBoneId.SpineRoot);
            }

            float radYaw = _orbitYaw * (float)(System.Math.PI / 180.0);
            float radPitch = _orbitPitch * (float)(System.Math.PI / 180.0);

            float zOffset = _orbitRadius * (float)System.Math.Sin(radPitch);
            float horizontalRadius = _orbitRadius * (float)System.Math.Cos(radPitch);

            float xOffset = horizontalRadius * (float)System.Math.Sin(-radYaw);
            float yOffset = horizontalRadius * (float)System.Math.Cos(-radYaw);

            _cam.Position = new Vector3(targetPos.X + xOffset, targetPos.Y + yOffset, targetPos.Z + zOffset);
            NativeFunction.Natives.POINT_CAM_AT_COORD(_cam, targetPos.X, targetPos.Y, targetPos.Z);
        }

        private static void ProcessLogic()
        {
            while (_active)
            {
                GameFiber.Yield();

                if (_patient == null || !_patient.Exists()) { StopInspection(); break; }
                if (NativeFunction.Natives.IS_PAUSE_MENU_ACTIVE<bool>()) NativeFunction.Natives.SET_PAUSE_MENU_ACTIVE(false);

                DisableControls();

                NativeFunction.Natives.DISABLE_CONTROL_ACTION(0, 24, true);
                NativeFunction.Natives.DISABLE_CONTROL_ACTION(0, 25, true);

                if (NativeFunction.Natives.IS_DISABLED_CONTROL_PRESSED<bool>(0, 25))
                {
                    if (!_isRotatingCamera)
                    {
                        _isRotatingCamera = true;
                        NativeFunction.Natives.SET_MOUSE_CURSOR_VISIBLE(false);
                        _preRotationCursorPos = Cursor.Position;
                    }

                    float mouseX = NativeFunction.Natives.GET_CONTROL_NORMAL<float>(0, (int)GameControl.LookLeftRight);
                    float mouseY = NativeFunction.Natives.GET_CONTROL_NORMAL<float>(0, (int)GameControl.LookUpDown);

                    _orbitYaw -= mouseX * MouseSensitivity * 3f;
                    _orbitPitch += mouseY * MouseSensitivity * 3f;
                    _orbitPitch = UI.Helpers.MathHelper.ClampFloat(_orbitPitch, -45f, 89f);
                }
                else
                {
                    if (_isRotatingCamera)
                    {
                        _isRotatingCamera = false;
                        NativeFunction.Natives.SET_MOUSE_CURSOR_VISIBLE(true);
                        Cursor.Position = _preRotationCursorPos;
                    }
                }

                if (_diagnostics != null) _diagnostics.Refresh();
                if (!_isRotatingCamera) _input.Update();

                _bodyParts.Update(_patient, _input.MousePosition);

                if (!_input.IsHoveringPanel)
                {
                    if (NativeFunction.Natives.IS_DISABLED_CONTROL_PRESSED<bool>(0, 15)) _orbitRadius -= 0.1f;
                    if (NativeFunction.Natives.IS_DISABLED_CONTROL_PRESSED<bool>(0, 14)) _orbitRadius += 0.1f;
                    float maxZoom = AmbulanceManager.IsPlayerInRearCabin ? 1.3f : 3.5f;
                    _orbitRadius = UI.Helpers.MathHelper.ClampFloat(_orbitRadius, 0.5f, maxZoom);
                }

                _state.Update();
                UpdateCameraPosition();

                if (NativeFunction.Natives.IS_DISABLED_CONTROL_JUST_PRESSED<bool>(0, 200)) { StopInspection(true); break; }

                if (!_isRotatingCamera)
                {
                    if (_input.ShouldToggleDiagnostics()) { _state.ToggleDiagnostics(); AudioHelper.PlaySelect(); }

                    if (_input.IsHoveringPanel)
                    {
                        if (NativeFunction.Natives.IS_DISABLED_CONTROL_JUST_PRESSED<bool>(0, 14)) if (_state.ActionScrollIndex < CurrentPanelActions.Count - 6) _state.ActionScrollIndex++;
                        if (NativeFunction.Natives.IS_DISABLED_CONTROL_JUST_PRESSED<bool>(0, 15)) if (_state.ActionScrollIndex > 0) _state.ActionScrollIndex--;
                    }

                    if (_input.IsClicking())
                    {
                        if (_input.ClickedExit()) { StopInspection(true); break; }
                        if (_input.ClickedDiagnostics()) { _state.ToggleDiagnostics(); AudioHelper.PlaySelect(); }

                        int visualIndex = _input.GetClickedPanelAction();
                        if (visualIndex != -1)
                        {
                            int realIndex = visualIndex + _state.ActionScrollIndex;
                            if (realIndex >= 0 && realIndex < CurrentPanelActions.Count)
                            {
                                var act = CurrentPanelActions[realIndex];
                                if (act.Enabled) { AudioHelper.PlaySelect(); act.OnExecute?.Invoke(); if (!_active) break; RefreshActions(); }
                                else AudioHelper.PlayError();
                            }
                        }
                        else
                        {
                            int diagIdx = _input.GetClickedDiagnosticAction();
                            if (diagIdx != -1 && _state.DiagnosticSlide > 0.1f)
                            {
                                var item = _diagnostics.Items[diagIdx];
                                if (item.HasAction) { AudioHelper.PlaySelect(); item.OnAction?.Invoke(); if (!_active) break; _diagnostics.Refresh(); }
                            }
                            if (_bodyParts.HoveredPart != null)
                            {
                                var activeTool = InventoryManager.ActiveTool;
                                if (_bodyParts.HoveredPart.LinkedEntity == null)
                                {
                                    GameState.CurrentPatient?.MarkBoneInspected(_bodyParts.HoveredPart.BoneId);
                                }

                                if (_bodyParts.HoveredPart.LinkedEntity != null)
                                {
                                    InventoryManager.ActiveTool = EmsTreatment.None;
                                    _bodyParts.SelectPart(_bodyParts.HoveredPart);

                                    CurrentMenuCategory = "KIT_HOME";

                                    RefreshActions();
                                    AudioHelper.PlaySelect();
                                }
                                else if (activeTool != EmsTreatment.None)
                                {
                                    var bone = _bodyParts.HoveredPart.BoneId;
                                    var p = GameState.CurrentPatient;

                                    bool isAnatomicallyValid = AnatomicalRegistry.IsToolValidForBone(activeTool, bone);
                                    bool isLocalized = AnatomicalRegistry.IsLocalizedTreatment(activeTool);

                                    bool needsThisTool = false;
                                    if (isLocalized)
                                    {
                                        needsThisTool = p.Conditions.OfType<PhysicalInjury>().Any(i => i.Bone == bone && !i.IsTreated && i.RequiredTreatments.Contains(activeTool));
                                    }
                                    else
                                    {
                                        needsThisTool = p.Conditions.Any(c => !c.IsTreated && c.RequiredTreatments.Contains(activeTool));
                                    }

                                    if (isAnatomicallyValid && needsThisTool)
                                    {
                                        HandleTreatmentLogic(activeTool, bone);
                                    }
                                    else
                                    {
                                        AudioHelper.PlayError();
                                    }
                                }
                                else
                                {
                                    _bodyParts.SelectPart(_bodyParts.HoveredPart);
                                    CurrentMenuCategory = "MAIN";
                                    RefreshActions();
                                    AudioHelper.PlaySelect();
                                }
                            }
                        }
                    }
                }
                _state.PanelSlide = _bodyParts.SelectedPart != null ? System.Math.Min(_state.PanelSlide + 0.12f, 1f) : System.Math.Max(_state.PanelSlide - 0.12f, 0f);
            }
        }

        private static void OnRender(object sender, GraphicsEventArgs e)
        {
            if (!_active) return;
            _renderer.Render(e.Graphics, _state, _bodyParts, _diagnostics, _input, CurrentPanelActions);
        }

        private static void DisableControls()
        {
            for (int i = 0; i <= 2; i++)
            {
                foreach (GameControl c in (GameControl[])Enum.GetValues(typeof(GameControl)))
                {
                    if (c != GameControl.LookLeftRight && c != GameControl.LookUpDown)
                        Game.DisableControlAction(i, c, true);
                }
            }
        }

        public static void HandleTreatmentLogic(EmsTreatment tool, PedBoneId bone)
        {
            StopInspection(false);

            var skillTask = new SkillCheckTask();
            GameFiber.StartNew(delegate {
                bool? success = null;
                while (success == null)
                {
                    GameFiber.Yield();
                    success = skillTask.Process();
                }

                if (success == true)
                {
                    GameState.CurrentPatient.ApplyTreatment(tool, bone);
                    InventoryManager.ActiveTool = EmsTreatment.None;
                }

                if (GameState.CurrentPatient != null && GameState.CurrentPatient.Character.Exists())
                {
                    StartInspection(GameState.CurrentPatient.Character);
                }
            });
        }

        public static void Cleanup()
        {
            if (_active)
            {
                _active = false;

                if (_cam != null && _cam.IsValid())
                {
                    _cam.Active = false;
                    _cam.Delete();
                }
                _cam = null;

                NativeFunction.Natives.RENDER_SCRIPT_CAMS(false, false, 0, true, true, 0);

                NativeFunction.Natives.SET_MOUSE_CURSOR_VISIBLE(false);

                if (Game.LocalPlayer.Character.Exists())
                {
                    Game.LocalPlayer.Character.IsPositionFrozen = false;
                }
            }
        }
    }
}