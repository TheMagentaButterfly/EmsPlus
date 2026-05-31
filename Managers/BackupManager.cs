using EmsPlus.Core;
using EmsPlus.UI.Helpers;
using Rage;
using Rage.Native;
using RAGENativeUI.Elements;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace EmsPlus.Managers
{
    public enum AIUnitState
    {
        Responding,
        Idle,
        Treating,
        LoadedOnScene,
        Transporting
    }

    public class AIUnit
    {
        public int UnitID { get; set; }
        public AIUnitState State { get; set; } = AIUnitState.Responding;
        public Vehicle Ambulance { get; set; }
        public Ped Medic1 { get; set; }
        public Ped Medic2 { get; set; }
        public Patient AssignedPatient { get; set; }
        public Blip UnitBlip { get; set; }

        public Vector3 SceneParkingLocation { get; set; }
        public bool IsSlowingDown { get; set; } = false;
    }

    public static class BackupManager
    {
        public static List<AIUnit> ActiveUnits { get; private set; } = new List<AIUnit>();
        private static int _unitCounter = 1;

        private static bool _isInitialized = false;
        private static bool _isHoldingFastDispatch = false;
        private static uint _fastDispatchStartTime = 0;

        private static TimerBarPool _timerBarPool;
        private static BarTimerBar _dispatchTimerBar;

        public static void Initialize()
        {
            if (_isInitialized) return;

            _timerBarPool = new TimerBarPool();
            _dispatchTimerBar = new BarTimerBar(Localization.Get("LBL_FAST_DISPATCH", "FAST DISPATCH"));
            _dispatchTimerBar.BackgroundColor = Color.Cyan;
            _dispatchTimerBar.ForegroundColor = Color.Blue;

            Game.FrameRender += OnFrameRender;
            _isInitialized = true;
        }

        public static void Shutdown()
        {
            if (!_isInitialized) return;
            Game.FrameRender -= OnFrameRender;
            _isInitialized = false;
            Cleanup();
        }

        private static Vector3 GetFacilitySpawnLocation(Vector3 playerPos)
        {
            List<Vector3> facilities = new List<Vector3>();
            if (EntryPoint.StationsConfig?.Locations != null)
                facilities.AddRange(EntryPoint.StationsConfig.Locations.Select(l => l.Position));
            if (EntryPoint.HospitalsConfig?.Locations != null)
                facilities.AddRange(EntryPoint.HospitalsConfig.Locations.Select(l => l.Position));

            if (facilities.Count == 0) return World.GetNextPositionOnStreet(playerPos.Around(250f));

            Vector3 closest = facilities.OrderBy(f => f.DistanceTo(playerPos)).First();

            if (playerPos.DistanceTo(closest) > 1000f)
            {
                Vector3 dir = (closest - playerPos);
                dir.Normalize();
                Vector3 point800mAway = playerPos + (dir * 800f);
                return World.GetNextPositionOnStreet(point800mAway.Around(50f));
            }

            Vector3 spawnNode = World.GetNextPositionOnStreet(closest.Around(20f));
            return spawnNode != Vector3.Zero ? spawnNode : closest;
        }

        private static Vector3 GetNearestHospital(Vector3 position)
        {
            var locations = EntryPoint.HospitalsConfig.Locations;
            if (locations.Count == 0) return new Vector3(300f, -600f, 43f);

            return locations.OrderBy(l => position.DistanceTo(l.Position)).First().Position;
        }

        public static void RequestAmbulance(int responseCode = 3)
        {
            if (!EmsService.IsOnDuty) return;

            Game.DisplayNotification(Localization.Get("NOTIF_BACKUP_ENROUTE", "~b~Dispatch:~w~ Copy that, additional EMS unit is en route."));

            GameFiber.StartNew(delegate
            {
                Vector3 spawnPos = GetFacilitySpawnLocation(Game.LocalPlayer.Character.Position);

                string modelName = EntryPoint.EmsPlusConfig.ValidAmbulanceModels.FirstOrDefault() ?? "ambulance";

                Model vehModel = new Model(modelName);
                vehModel.LoadAndWait();
                Vehicle ambulance = new Vehicle(vehModel, spawnPos);
                vehModel.Dismiss();

                if (!ambulance.Exists()) return;

                ambulance.IsPersistent = true;

                Model pedModel = new Model("s_m_m_paramedic_01");
                pedModel.LoadAndWait();

                Ped driver = new Ped(pedModel, spawnPos, 0f);
                Ped passenger = new Ped(pedModel, spawnPos, 0f);
                pedModel.Dismiss();

                driver.IsPersistent = true; passenger.IsPersistent = true;
                driver.BlockPermanentEvents = true; passenger.BlockPermanentEvents = true;

                driver.WarpIntoVehicle(ambulance, -1);
                passenger.WarpIntoVehicle(ambulance, 0);

                Blip unitBlip = new Blip(ambulance);
                unitBlip.Color = Color.Orange;
                unitBlip.Name = $"EMS Backup {_unitCounter}";

                Vector3 parkingNode = World.GetNextPositionOnStreet(Game.LocalPlayer.Character.Position.Around(10f, 20f));

                if (parkingNode == Vector3.Zero || parkingNode.DistanceTo2D(Game.LocalPlayer.Character.Position) > 40f)
                {
                    parkingNode = Game.LocalPlayer.Character.Position;
                }

                var unit = new AIUnit
                {
                    UnitID = _unitCounter++,
                    Ambulance = ambulance,
                    Medic1 = driver,
                    Medic2 = passenger,
                    State = AIUnitState.Responding,
                    UnitBlip = unitBlip,
                    SceneParkingLocation = parkingNode
                };
                ActiveUnits.Add(unit);

                if (responseCode == 1)
                {
                    ambulance.IsSirenOn = false;
                    ambulance.IsSirenSilent = true;
                    driver.Tasks.DriveToPosition(ambulance, parkingNode, 15f, VehicleDrivingFlags.Normal, 10f);
                }
                else if (responseCode == 2)
                {
                    ambulance.IsSirenOn = true;
                    ambulance.IsSirenSilent = true;
                    driver.Tasks.DriveToPosition(ambulance, parkingNode, 20f, VehicleDrivingFlags.Emergency, 10f);
                }
                else
                {
                    ambulance.IsSirenOn = true;
                    ambulance.IsSirenSilent = false;
                    driver.Tasks.DriveToPosition(ambulance, parkingNode, 25f, VehicleDrivingFlags.Emergency, 10f);
                }
            });
        }

        public static void Process()
        {
            if (ActiveUnits.Count == 0) return;

            bool hasRespondingUnits = ActiveUnits.Any(u => u.State == AIUnitState.Responding);
            if (hasRespondingUnits && Game.IsKeyDownRightNow(System.Windows.Forms.Keys.Back))
            {
                if (!_isHoldingFastDispatch)
                {
                    _isHoldingFastDispatch = true;
                    _fastDispatchStartTime = Game.GameTime;
                    _dispatchTimerBar.Percentage = 0f;
                    _timerBarPool.Add(_dispatchTimerBar);
                }
                else
                {
                    float progress = (Game.GameTime - _fastDispatchStartTime) / 2000f;
                    _dispatchTimerBar.Percentage = UI.Helpers.MathHelper.ClampFloat(progress, 0f, 1f);

                    if (Game.GameTime > _fastDispatchStartTime + 2000)
                    {
                        TeleportRespondingUnits();
                        _isHoldingFastDispatch = false;
                        _timerBarPool.Remove(_dispatchTimerBar);
                    }
                }
            }
            else
            {
                if (_isHoldingFastDispatch)
                {
                    _isHoldingFastDispatch = false;
                    _timerBarPool.Remove(_dispatchTimerBar);
                }
            }

            foreach (var unit in ActiveUnits.ToList())
            {
                if (unit.State == AIUnitState.Responding && unit.Ambulance.Exists() && unit.Medic1.Exists())
                {
                    float distToPark = unit.Ambulance.DistanceTo(unit.SceneParkingLocation);

                    if (!unit.IsSlowingDown && distToPark < 100f)
                    {
                        unit.IsSlowingDown = true;
                        unit.Ambulance.IsSirenSilent = true;
                        unit.Medic1.Tasks.DriveToPosition(unit.Ambulance, unit.SceneParkingLocation, 8f, VehicleDrivingFlags.Normal, 5f);
                    }

                    if (distToPark < 12f || (unit.Ambulance.Speed < 0.5f && distToPark < 30f))
                    {
                        unit.State = AIUnitState.Idle;
                        unit.Ambulance.IsSirenSilent = true;

                        unit.Medic1.Tasks.Clear(); unit.Medic2.Tasks.Clear();
                        unit.Medic1.Tasks.LeaveVehicle(unit.Ambulance, LeaveVehicleFlags.None);
                        unit.Medic2.Tasks.LeaveVehicle(unit.Ambulance, LeaveVehicleFlags.None);

                        GameFiber.StartNew(delegate
                        {
                            GameFiber.Sleep(2500);
                            if (unit.Medic1.Exists()) unit.Medic1.Tasks.GoToOffsetFromEntity(Game.LocalPlayer.Character, 3f, 0f, 1.0f);
                            if (unit.Medic2.Exists()) unit.Medic2.Tasks.GoToOffsetFromEntity(Game.LocalPlayer.Character, -3f, 0f, 1.0f);
                        });
                    }
                }
            }
        }

        private static void TeleportRespondingUnits()
        {
            foreach (var unit in ActiveUnits.Where(u => u.State == AIUnitState.Responding))
            {
                if (unit.Ambulance.Exists())
                {
                    Vector3 safePos = unit.SceneParkingLocation;
                    unit.Ambulance.Position = safePos;
                }

                if (unit.Medic1.Exists()) unit.Medic1.WarpIntoVehicle(unit.Ambulance, -1);
                if (unit.Medic2.Exists()) unit.Medic2.WarpIntoVehicle(unit.Ambulance, 0);
            }
        }

        public static void OrderTreatment(AIUnit unit, Patient patient)
        {
            if (unit == null || patient == null || !patient.Character.Exists()) return;

            unit.State = AIUnitState.Treating;
            unit.AssignedPatient = patient;

            GameFiber.StartNew(delegate
            {
                if (unit.Medic1.Exists()) unit.Medic1.Tasks.GoToOffsetFromEntity(patient.Character, 1.5f, 0f, 2.0f);
                if (unit.Medic2.Exists()) unit.Medic2.Tasks.GoToOffsetFromEntity(patient.Character, 2.0f, 0f, 2.0f);

                WaitUntilClose(unit.Medic1, patient.Character.Position, 2.5f, 150);

                if (unit.Medic1.Exists()) unit.Medic1.Tasks.PlayAnimation("amb@medic@standing@tendtodead@base", "base", 8.0f, AnimationFlags.Loop);
                if (unit.Medic2.Exists()) unit.Medic2.Tasks.PlayAnimation("amb@medic@standing@kneel@idle_a", "idle_a", 8.0f, AnimationFlags.Loop);

                while (unit.State == AIUnitState.Treating && patient.Character.Exists())
                {
                    GameFiber.Sleep(5000);
                    if (patient.BloodVolume < 100f) patient.BloodVolume += 2f;
                    if (patient.BrainOxygen < 100f) patient.BrainOxygen += 5f;
                }
            });
        }

        public static void OrderLoadOnly(AIUnit unit, Patient patient)
        {
            StartLoadingSequence(unit, patient, false);
        }

        public static void OrderTransport(AIUnit unit, Patient patient)
        {
            StartLoadingSequence(unit, patient, true);
        }

        private static void StartLoadingSequence(AIUnit unit, Patient patient, bool driveToHospital)
        {
            if (unit == null || (patient == null && unit.AssignedPatient == null)) return;

            ActiveUnits.Remove(unit);
            bool isAlreadyLoaded = (unit.State == AIUnitState.LoadedOnScene);

            unit.State = AIUnitState.Transporting;
            if (patient != null) unit.AssignedPatient = patient;

            GameFiber.StartNew(delegate
            {
                var amb = unit.Ambulance;
                var m1 = unit.Medic1;
                var m2 = unit.Medic2;
                var pat = unit.AssignedPatient.Character;
                var offsets = EntryPoint.OffsetConfig;

                bool alreadyAttached = false;
                Rage.Object transportStretcher = null;

                if (pat.Exists())
                {
                    uint parentHandle = NativeFunction.Natives.GET_ENTITY_ATTACHED_TO<uint>(pat);
                    if (parentHandle != 0)
                    {
                        Entity parent = World.GetEntityByHandle<Entity>(parentHandle);
                        if (parent != null && parent.Exists())
                        {
                            uint grandParentHandle = NativeFunction.Natives.GET_ENTITY_ATTACHED_TO<uint>(parent);
                            if (grandParentHandle == amb.Handle.Value)
                            {
                                alreadyAttached = true;
                                transportStretcher = parent as Rage.Object;
                                if (StretcherManager.Prop != null && parentHandle == StretcherManager.Prop.Handle.Value)
                                {
                                    StretcherManager.ForgetProp();
                                }
                            }
                        }
                    }
                }

                if (!isAlreadyLoaded && !alreadyAttached)
                {
                    Vector3 rearPos = amb.GetOffsetPosition(new Vector3(0, -4.5f, 0));
                    if (m1.Exists()) m1.Tasks.GoStraightToPosition(rearPos, 1.5f, amb.Heading, 1.0f, 10000);
                    if (m2.Exists()) m2.Tasks.GoStraightToPosition(amb.GetOffsetPosition(new Vector3(2f, -4.5f, 0)), 1.5f, amb.Heading, 1.0f, 10000);

                    WaitUntilClose(m1, rearPos, 2.5f, 150);

                    if (amb.Exists()) { amb.Doors[2].Open(false); amb.Doors[3].Open(false); }
                    GameFiber.Sleep(1000);

                    Model stretcherModel = new Model(EntryPoint.PropConfig.StretcherModel);
                    stretcherModel.LoadAndWait();
                    Rage.Object aiStretcher = new Rage.Object(stretcherModel, m1.Position);
                    stretcherModel.Dismiss();
                    transportStretcher = aiStretcher;

                    aiStretcher.AttachTo(m1, 0, new Vector3(offsets.StretcherAttachOffsetX, offsets.StretcherAttachOffsetY, offsets.StretcherAttachOffsetZ), new Rotator(offsets.StretcherAttachPitch, offsets.StretcherAttachRoll, offsets.StretcherAttachYaw));
                    string carryDict = EntryPoint.AnimationConfig.MedicStretcherCarryDict.Value;
                    string carryName = EntryPoint.AnimationConfig.MedicStretcherCarryName.Value;
                    NativeFunction.Natives.REQUEST_ANIM_DICT(carryDict);
                    while (!NativeFunction.Natives.HAS_ANIM_DICT_LOADED<bool>(carryDict)) GameFiber.Yield();
                    m1.Tasks.PlayAnimation(carryDict, carryName, 8f, AnimationFlags.Loop | AnimationFlags.UpperBodyOnly | AnimationFlags.SecondaryTask);

                    if (m1.Exists()) m1.Tasks.GoToOffsetFromEntity(pat, 1.5f, 0f, 1.0f);
                    if (m2.Exists()) m2.Tasks.GoToOffsetFromEntity(pat, 2.0f, 0f, 1.0f);

                    WaitUntilClose(m1, pat.Position, 2.5f, 200);

                    GameFiber.Sleep(1500);

                    if (unit.AssignedPatient.IsOnStretcher && StretcherManager.Prop != null && StretcherManager.Prop.Exists() && StretcherManager.Prop.DistanceTo(pat) < 3f)
                    {
                        pat.Detach();
                        unit.AssignedPatient.IsOnStretcher = false;
                    }

                    pat.Tasks.ClearImmediately();
                    pat.AttachTo(aiStretcher, -1, new Vector3(offsets.PatientAttachOffsetX, offsets.PatientAttachOffsetY, offsets.PatientAttachOffsetZ), new Rotator(offsets.PatientAttachPitch, offsets.PatientAttachRoll, offsets.PatientAttachYaw));
                    string patDict = EntryPoint.AnimationConfig.PatientStretcherDict.Value;
                    string patName = EntryPoint.AnimationConfig.PatientStretcherName.Value;
                    NativeFunction.Natives.REQUEST_ANIM_DICT(patDict);
                    while (!NativeFunction.Natives.HAS_ANIM_DICT_LOADED<bool>(patDict)) GameFiber.Yield();
                    pat.Tasks.PlayAnimation(patDict, patName, 8f, AnimationFlags.Loop);
                    unit.AssignedPatient.IsOnStretcher = true;

                    rearPos = amb.GetOffsetPosition(new Vector3(0, -4.5f, 0));
                    if (m1.Exists()) m1.Tasks.GoStraightToPosition(rearPos, 1.5f, amb.Heading, 1.0f, 15000);
                    if (m2.Exists()) m2.Tasks.GoStraightToPosition(amb.GetOffsetPosition(new Vector3(2f, -4.5f, 0)), 1.5f, amb.Heading, 1.0f, 15000);

                    WaitUntilClose(m1, rearPos, 2.5f, 250);

                    GameFiber.Sleep(1500);
                    NativeFunction.Natives.CLEAR_PED_SECONDARY_TASK(m1);

                    var cfg = new Configuration.VehicleConfig(amb.Model.Name);
                    cfg.Load();

                    aiStretcher.AttachTo(amb, 0, cfg.StowPos, cfg.StowRot);
                    if (cfg.HideStretcherInVehicle) NativeFunction.Natives.SET_ENTITY_ALPHA(aiStretcher, 0, false);
                }

                if (amb.Exists()) { amb.Doors[2].Close(false); amb.Doors[3].Close(false); }
                GameState.ActivePatients.Remove(unit.AssignedPatient);

                if (!driveToHospital)
                {
                    unit.State = AIUnitState.LoadedOnScene;
                    ActiveUnits.Add(unit);
                    return;
                }

                if (m1.Exists()) m1.Tasks.EnterVehicle(amb, -1);
                if (m2.Exists()) m2.Tasks.EnterVehicle(amb, 0);

                int headcountTimeout = 0;
                while ((!m1.IsInVehicle(amb, false) || !m2.IsInVehicle(amb, false)) && headcountTimeout < 150)
                {
                    GameFiber.Sleep(200);
                    headcountTimeout++;
                }

                if (m1.Exists() && !m1.IsInVehicle(amb, false)) m1.WarpIntoVehicle(amb, -1);
                if (m2.Exists() && !m2.IsInVehicle(amb, false)) m2.WarpIntoVehicle(amb, 0);

                if (amb.Exists() && m1.Exists())
                {
                    Vector3 hospitalPos = GetNearestHospital(amb.Position);
                    amb.IsSirenOn = true;
                    amb.IsSirenSilent = false;

                    m1.Tasks.DriveToPosition(amb, hospitalPos, 25f, VehicleDrivingFlags.Emergency, 10f);
                }
                if (unit.UnitBlip != null && unit.UnitBlip.Exists()) unit.UnitBlip.Delete();
                GameFiber.Sleep(20000);
                if (amb.Exists()) amb.Dismiss();
                if (m1.Exists()) m1.Dismiss();
                if (m2.Exists()) m2.Dismiss();
                if (pat.Exists()) pat.Delete();
                if (transportStretcher != null && transportStretcher.Exists()) transportStretcher.Delete();
            });
        }

        public static void DismissUnit(AIUnit unit)
        {
            if (unit == null) return;
            ActiveUnits.Remove(unit);

            GameFiber.StartNew(delegate
            {
                var amb = unit.Ambulance;
                var m1 = unit.Medic1;
                var m2 = unit.Medic2;

                if (m1.Exists()) m1.Tasks.EnterVehicle(amb, -1);
                if (m2.Exists()) m2.Tasks.EnterVehicle(amb, 0);

                int headcountTimeout = 0;
                while (amb.Exists() && (!m1.IsInVehicle(amb, false) || !m2.IsInVehicle(amb, false)) && headcountTimeout < 150)
                {
                    GameFiber.Sleep(200);
                    headcountTimeout++;
                }

                if (m1.Exists() && !m1.IsInVehicle(amb, false)) m1.WarpIntoVehicle(amb, -1);
                if (m2.Exists() && !m2.IsInVehicle(amb, false)) m2.WarpIntoVehicle(amb, 0);

                if (amb.Exists() && m1.Exists())
                {
                    amb.IsSirenSilent = true;
                    amb.IsSirenOn = false;
                    m1.Tasks.CruiseWithVehicle(amb, 15f, VehicleDrivingFlags.Normal);
                }

                if (unit.UnitBlip != null && unit.UnitBlip.Exists()) unit.UnitBlip.Delete();
                GameFiber.Sleep(10000);
                if (amb.Exists()) amb.Dismiss();
                if (m1.Exists()) m1.Dismiss();
                if (m2.Exists()) m2.Dismiss();
            });
        }

        public static void Cleanup()
        {
            if (_timerBarPool != null && _dispatchTimerBar != null)
            {
                _timerBarPool.Remove(_dispatchTimerBar);
            }

            foreach (var unit in ActiveUnits)
            {
                if (unit.UnitBlip != null && unit.UnitBlip.Exists()) unit.UnitBlip.Delete();
                if (unit.Ambulance != null && unit.Ambulance.Exists()) unit.Ambulance.Delete();
                if (unit.Medic1 != null && unit.Medic1.Exists()) unit.Medic1.Delete();
                if (unit.Medic2 != null && unit.Medic2.Exists()) unit.Medic2.Delete();
            }
            ActiveUnits.Clear();
            _unitCounter = 1;
        }

        // ==========================================
        // HELPERS
        // ==========================================
        private static void WaitUntilClose(Ped ped, Vector3 target, float distance, int maxIterations)
        {
            int count = 0;
            while (ped.Exists() && ped.IsAlive && ped.DistanceTo(target) > distance && count < maxIterations)
            {
                GameFiber.Sleep(100);
                count++;
            }
            if (count >= maxIterations && ped.Exists())
            {
                ped.Position = target;
            }
        }

        private static void OnFrameRender(object sender, GraphicsEventArgs e)
        {
            if (_isHoldingFastDispatch && _timerBarPool != null)
            {
                _timerBarPool.Draw();
            }
        }
    }
}