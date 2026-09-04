using EmsPlus.Core;
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
        public string UnitDisplayName { get; set; }
        public AIUnitState State { get; set; } = AIUnitState.Responding;
        public Vehicle Ambulance { get; set; }
        public Ped Medic1 { get; set; }
        public Ped Medic2 { get; set; }
        public Patient AssignedPatient { get; set; }
        public Blip UnitBlip { get; set; }
        public Rage.Object TransportStretcher { get; set; }
        public Vector3 SceneParkingLocation { get; set; }
        public bool IsSlowingDown { get; set; } = false;
        public string ServiceType { get; set; } = "Ambulance";
    }

    public static class BackupManager
    {
        public static List<AIUnit> ActiveUnits { get; private set; } = new List<AIUnit>();
        private static int _unitCounter = 1;

        private static bool _isInitialized = false;
        private static bool _isHoldingFastDispatch = false;
        private static uint _fastDispatchStartTime = 0;

        private static bool _isHoldingFastDismiss = false;
        private static uint _fastDismissStartTime = 0;

        private static TimerBarPool _timerBarPool;
        private static BarTimerBar _dispatchTimerBar;
        private static BarTimerBar _dismissTimerBar;

        public static void Initialize()
        {
            if (_isInitialized) return;

            _timerBarPool = new TimerBarPool();

            _dispatchTimerBar = new BarTimerBar(Localization.Get("LBL_FAST_DISPATCH", "FAST DISPATCH"));
            _dispatchTimerBar.BackgroundColor = Color.DarkBlue;
            _dispatchTimerBar.ForegroundColor = Color.Blue;

            _dismissTimerBar = new BarTimerBar(Localization.Get("LBL_FAST_DISMISS", "FAST DISMISS"));
            _dismissTimerBar.BackgroundColor = Color.DarkRed;
            _dismissTimerBar.ForegroundColor = Color.Red;

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

        private static Vector3 GetSpawnLocationInRadius(Vector3 playerPos, float minDistance = 200f, float maxDistance = 400f)
        {
            for (int i = 0; i < 15; i++)
            {
                Vector3 candidate = World.GetNextPositionOnStreet(playerPos.Around(minDistance, maxDistance));
                if (candidate != Vector3.Zero && candidate.DistanceTo(playerPos) >= minDistance)
                {
                    return candidate;
                }
            }

            Vector3 fallback = World.GetNextPositionOnStreet(playerPos.Around(250f));
            return fallback != Vector3.Zero ? fallback : playerPos.Around(250f);
        }

        private static Vector3 FindClearLandingZone(Vector3 playerPos)
        {
            for (int i = 0; i < 15; i++)
            {
                Vector3 candidate = World.GetNextPositionOnStreet(playerPos.Around(35f, 75f));
                if (candidate == Vector3.Zero) candidate = playerPos.Around(45f);

                float? groundZ = World.GetGroundZ(candidate, true, true);
                if (groundZ.HasValue)
                {
                    Vector3 groundPos = new Vector3(candidate.X, candidate.Y, groundZ.Value);

                    // Vertical raycast ensuring 35m of unobstructed sky above the landing spot
                    var hit = World.TraceLine(groundPos + new Vector3(0, 0, 1.5f), groundPos + new Vector3(0, 0, 35f), TraceFlags.IntersectWorld | TraceFlags.IntersectVehicles);
                    if (!hit.Hit)
                    {
                        return groundPos;
                    }
                }
            }

            float? fallbackZ = World.GetGroundZ(playerPos.Around(45f), true, true);
            return new Vector3(playerPos.X + 40f, playerPos.Y + 20f, fallbackZ ?? playerPos.Z);
        }

        private static string GetUnitDisplayName(string serviceType, Configuration.BackupDepartment dept, int unitId)
        {
            if (serviceType.Equals("Helicopter", StringComparison.OrdinalIgnoreCase))
            {
                return $"Medevac-{unitId}";
            }
            if (serviceType.Equals("Fire", StringComparison.OrdinalIgnoreCase))
            {
                return $"FireDepartment-{unitId}";
            }
            if (serviceType.Equals("Police", StringComparison.OrdinalIgnoreCase))
            {
                string deptName = dept?.Name?.ToLower() ?? "";
                if (deptName.Contains("sheriff") || deptName.Contains("state") || deptName.Contains("highway") || deptName.Contains("county"))
                {
                    return $"State Patrol-{unitId}";
                }
                return $"Local Patrol-{unitId}";
            }

            return $"Ambulance-{unitId}";
        }

        private static Vector3 GetNearestHospital(Vector3 position)
        {
            var locations = EntryPoint.HospitalsConfig.Locations;
            if (locations.Count == 0) return new Vector3(300f, -600f, 43f);

            return locations.OrderBy(l => position.DistanceTo(l.Position)).First().Position;
        }

        public static void RequestBackup(string serviceType, int responseCode = 3)
        {
            if (!EmsService.IsOnDuty) return;

            GameFiber.StartNew(delegate
            {
                var dept = EntryPoint.BackupConfig.GetRandomDepartmentForService(serviceType)
                           ?? EntryPoint.BackupConfig.Departments.FirstOrDefault();

                if (dept == null) return;

                int currentUnitNumber = _unitCounter++;
                string unitName = GetUnitDisplayName(serviceType, dept, currentUnitNumber);

                Game.DisplayNotification($"~b~Dispatch:~w~ Copy that, ~y~{unitName}~w~ is en route.");

                var vehDef = dept.GetRandomVehicle();
                var pedDef1 = dept.GetRandomPed();
                var pedDef2 = dept.GetRandomPed();

                if (vehDef == null || pedDef1 == null || pedDef2 == null) return;

                Model vehModel = new Model(vehDef.Model);
                vehModel.LoadAndWait();

                Model pedModel1 = new Model(pedDef1.Model);
                pedModel1.LoadAndWait();

                Model pedModel2 = new Model(pedDef2.Model);
                pedModel2.LoadAndWait();

                if (serviceType.Equals("Helicopter", StringComparison.OrdinalIgnoreCase))
                {
                    // Spawn helicopter in a radius high in the sky around the player
                    Vector3 spawnPos = Game.LocalPlayer.Character.Position.Around(350f, 500f);
                    spawnPos.Z += 120f;

                    Vehicle heli = new Vehicle(vehModel, spawnPos);
                    heli.IsPersistent = true;

                    Ped pilot = new Ped(pedModel1, spawnPos, 0f);
                    Ped flightMedic = new Ped(pedModel2, spawnPos, 0f);

                    pilot.IsPersistent = true; flightMedic.IsPersistent = true;
                    pilot.BlockPermanentEvents = true; flightMedic.BlockPermanentEvents = true;

                    pilot.WarpIntoVehicle(heli, -1);
                    flightMedic.WarpIntoVehicle(heli, 0);

                    NativeFunction.Natives.SET_PED_CONFIG_FLAG(pilot, 34, true);
                    NativeFunction.Natives.SET_PED_CONFIG_FLAG(flightMedic, 34, true);

                    pedDef1.ApplyTo(pilot);
                    pedDef2.ApplyTo(flightMedic);

                    Blip unitBlip = new Blip(heli);
                    unitBlip.Color = Color.Red;
                    unitBlip.Name = unitName;

                    Vector3 landingPos = FindClearLandingZone(Game.LocalPlayer.Character.Position);

                    var unit = new AIUnit
                    {
                        UnitID = currentUnitNumber,
                        UnitDisplayName = unitName,
                        Ambulance = heli,
                        Medic1 = pilot,
                        Medic2 = flightMedic,
                        State = AIUnitState.Responding,
                        UnitBlip = unitBlip,
                        ServiceType = "Helicopter",
                        SceneParkingLocation = landingPos
                    };
                    ActiveUnits.Add(unit);

                    NativeFunction.Natives.SET_DRIVER_ABILITY(pilot, 1.0f);
                    NativeFunction.Natives.SET_DRIVER_AGGRESSIVENESS(pilot, 0.0f);
                    NativeFunction.Natives.SET_HELI_BLADES_FULL_SPEED(heli);

                    // Initial approach task
                    NativeFunction.CallByHash<int>(0xDAD029E187A2BEB4, pilot, heli, 0, 0, landingPos.X, landingPos.Y, landingPos.Z, 20, 30.0f, 8.0f, 0.0f, -1, -1, -1f, 32);

                    GameFiber.StartNew(delegate
                    {
                        uint approachStartTime = Game.GameTime;

                        // 1. Wait until helicopter reaches the landing zone vicinity (2D check)
                        while (heli.Exists() && heli.Position.DistanceTo2D(landingPos) > 40f && (Game.GameTime - approachStartTime) < 45000)
                        {
                            GameFiber.Sleep(500);
                        }

                        // 2. Issue precision land command
                        if (heli.Exists() && pilot.Exists())
                        {
                            NativeFunction.CallByHash<int>(0xDAD029E187A2BEB4, pilot, heli, 0, 0, landingPos.X, landingPos.Y, landingPos.Z, 4, 15.0f, 2.0f, 0.0f, -1, -1, -1f, 32);
                        }

                        // 3. Monitor descent & assist touchdown if AI stalls in a hover
                        uint descentStartTime = Game.GameTime;
                        while (heli.Exists() && heli.HeightAboveGround > 1.2f && (Game.GameTime - descentStartTime) < 25000)
                        {
                            if (heli.Position.DistanceTo2D(landingPos) < 25f && (Game.GameTime - descentStartTime) > 3500)
                            {
                                if (heli.Speed < 4.0f && heli.HeightAboveGround > 1.2f)
                                {
                                    heli.Velocity = new Vector3(heli.Velocity.X * 0.7f, heli.Velocity.Y * 0.7f, -1.8f);
                                }
                            }
                            GameFiber.Sleep(100);
                        }

                        // 4. Touchdown stabilization & engine idle
                        if (heli.Exists())
                        {
                            heli.Velocity = Vector3.Zero;
                            float? finalGz = World.GetGroundZ(heli.Position, true, true);
                            if (finalGz.HasValue && heli.HeightAboveGround > 0.6f && heli.HeightAboveGround < 4.0f)
                            {
                                heli.Position = new Vector3(heli.Position.X, heli.Position.Y, finalGz.Value + 0.3f);
                            }

                            if (pilot.Exists())
                            {
                                pilot.Tasks.Clear();
                                NativeFunction.Natives.SET_VEHICLE_ENGINE_ON(heli, true, true, false);
                                NativeFunction.Natives.SET_HELI_BLADES_FULL_SPEED(heli);
                            }
                        }

                        unit.State = AIUnitState.Idle;

                        // 5. Flight medic disembarks to assist
                        if (flightMedic.Exists())
                        {
                            flightMedic.Tasks.Clear();
                            flightMedic.Tasks.LeaveVehicle(heli, LeaveVehicleFlags.None);
                            GameFiber.Sleep(2000);
                            if (flightMedic.Exists()) flightMedic.Tasks.GoToOffsetFromEntity(Game.LocalPlayer.Character, 3f, 0f, 1.0f);
                        }
                    });
                }
                else
                {
                    // Spawn land vehicles within a 200m–400m radius
                    Vector3 spawnPos = GetSpawnLocationInRadius(Game.LocalPlayer.Character.Position, 200f, 400f);
                    Vehicle vehicle = new Vehicle(vehModel, spawnPos);
                    vehicle.IsPersistent = true;

                    Ped driver = new Ped(pedModel1, spawnPos, 0f);
                    Ped passenger = new Ped(pedModel2, spawnPos, 0f);

                    driver.IsPersistent = true; passenger.IsPersistent = true;
                    driver.BlockPermanentEvents = true; passenger.BlockPermanentEvents = true;

                    driver.WarpIntoVehicle(vehicle, -1);
                    passenger.WarpIntoVehicle(vehicle, 0);

                    NativeFunction.Natives.SET_PED_CONFIG_FLAG(driver, 34, true);
                    NativeFunction.Natives.SET_PED_CONFIG_FLAG(passenger, 34, true);

                    pedDef1.ApplyTo(driver);
                    pedDef2.ApplyTo(passenger);

                    if (serviceType.Equals("Police", StringComparison.OrdinalIgnoreCase))
                    {
                        driver.RelationshipGroup = "COP"; passenger.RelationshipGroup = "COP";
                    }
                    else if (serviceType.Equals("Fire", StringComparison.OrdinalIgnoreCase))
                    {
                        driver.RelationshipGroup = "FIREMAN"; passenger.RelationshipGroup = "FIREMAN";
                    }
                    else
                    {
                        driver.RelationshipGroup = "MEDIC"; passenger.RelationshipGroup = "MEDIC";
                    }

                    Blip unitBlip = new Blip(vehicle);
                    unitBlip.Color = serviceType.Equals("Police", StringComparison.OrdinalIgnoreCase) ? Color.Blue : Color.Orange;
                    unitBlip.Name = unitName;

                    Vector3 parkingNode = World.GetNextPositionOnStreet(Game.LocalPlayer.Character.Position.Around(12f, 25f));
                    if (parkingNode == Vector3.Zero || parkingNode.DistanceTo2D(Game.LocalPlayer.Character.Position) > 45f)
                    {
                        parkingNode = Game.LocalPlayer.Character.Position;
                    }

                    var unit = new AIUnit
                    {
                        UnitID = currentUnitNumber,
                        UnitDisplayName = unitName,
                        Ambulance = vehicle,
                        Medic1 = driver,
                        Medic2 = passenger,
                        State = AIUnitState.Responding,
                        UnitBlip = unitBlip,
                        SceneParkingLocation = parkingNode,
                        ServiceType = serviceType
                    };
                    ActiveUnits.Add(unit);

                    if (responseCode == 1)
                    {
                        vehicle.IsSirenOn = false; vehicle.IsSirenSilent = true;
                        driver.Tasks.DriveToPosition(vehicle, parkingNode, 15f, VehicleDrivingFlags.Normal, 10f);
                    }
                    else if (responseCode == 2)
                    {
                        vehicle.IsSirenOn = true; vehicle.IsSirenSilent = true;
                        driver.Tasks.DriveToPosition(vehicle, parkingNode, 22f, VehicleDrivingFlags.Emergency, 10f);
                    }
                    else
                    {
                        vehicle.IsSirenOn = true; vehicle.IsSirenSilent = false;
                        driver.Tasks.DriveToPosition(vehicle, parkingNode, 28f, VehicleDrivingFlags.Emergency, 10f);
                    }
                }

                vehModel.Dismiss();
                pedModel1.Dismiss();
                pedModel2.Dismiss();
            });
        }

        public static void Process()
        {
            if (ActiveUnits.Count == 0) return;

            // 1. FAST DISPATCH (Hold Backspace)
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
                    _dispatchTimerBar.Percentage = MathHelper.Clamp(progress, 0f, 1f);

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

            // 2. FAST DISMISS (Hold Enter)
            if (Game.IsKeyDownRightNow(System.Windows.Forms.Keys.Enter))
            {
                if (!_isHoldingFastDismiss)
                {
                    _isHoldingFastDismiss = true;
                    _fastDismissStartTime = Game.GameTime;
                    _dismissTimerBar.Percentage = 0f;
                    _timerBarPool.Add(_dismissTimerBar);
                }
                else
                {
                    float progress = (Game.GameTime - _fastDismissStartTime) / 2000f;
                    _dismissTimerBar.Percentage = MathHelper.Clamp(progress, 0f, 1f);

                    if (Game.GameTime > _fastDismissStartTime + 2000)
                    {
                        ForceDismissAllUnits();
                        _isHoldingFastDismiss = false;
                        _timerBarPool.Remove(_dismissTimerBar);
                    }
                }
            }
            else
            {
                if (_isHoldingFastDismiss)
                {
                    _isHoldingFastDismiss = false;
                    _timerBarPool.Remove(_dismissTimerBar);
                }
            }

            Vector3 targetPos = Game.LocalPlayer.Character.Position;
            foreach (var unit in ActiveUnits.ToList())
            {
                if (unit.State == AIUnitState.Responding && unit.ServiceType != "Helicopter" && unit.Ambulance.Exists() && unit.Medic1.Exists())
                {
                    float distToPark = unit.Ambulance.DistanceTo(unit.SceneParkingLocation);

                    if (!unit.IsSlowingDown && distToPark < 65f)
                    {
                        unit.IsSlowingDown = true;
                        unit.Ambulance.IsSirenSilent = true;
                        unit.Medic1.Tasks.DriveToPosition(unit.Ambulance, unit.SceneParkingLocation, 12f, VehicleDrivingFlags.Normal, 8f);
                    }

                    if (distToPark < 12f || (unit.Ambulance.Speed < 0.5f && distToPark < 25f))
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
                            Game.DisplayNotification($"~b~Dispatch:~w~ ~y~{unit.UnitDisplayName}~w~ has arrived on scene.");
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
                    if (unit.ServiceType == "Helicopter")
                    {
                        Vector3 targetPos = unit.SceneParkingLocation;
                        float? gz = World.GetGroundZ(targetPos, true, true);
                        float groundZ = gz ?? targetPos.Z;

                        unit.Ambulance.Position = new Vector3(targetPos.X, targetPos.Y, groundZ + 20f);
                        unit.Ambulance.Velocity = new Vector3(0f, 0f, -1.5f);
                        unit.Ambulance.Heading = Game.LocalPlayer.Character.Heading;
                    }
                    else
                    {
                        Vector3 safePos = unit.SceneParkingLocation;
                        unit.Ambulance.Position = safePos;
                    }
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
                                unit.TransportStretcher = transportStretcher;
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
                    amb.Model.GetDimensions(out Vector3 min, out Vector3 max);
                    Vector3 rearPos = amb.GetOffsetPosition(new Vector3(0, min.Y - 0.5f, 0));

                    if (m1.Exists()) m1.Tasks.GoStraightToPosition(rearPos, 1.5f, amb.Heading, 1.0f, 10000);
                    if (m2.Exists()) m2.Tasks.GoStraightToPosition(amb.GetOffsetPosition(new Vector3(2f, min.Y - 0.5f, 0)), 1.5f, amb.Heading, 1.0f, 10000);

                    WaitUntilClose(m1, rearPos, 2.5f, 150);

                    ToggleAIDoors(amb, true);
                    GameFiber.Sleep(1000);

                    Model stretcherModel = new Model(EntryPoint.PropConfig.StretcherModel);
                    stretcherModel.LoadAndWait();
                    Rage.Object aiStretcher = new Rage.Object(stretcherModel, m1.Position);
                    stretcherModel.Dismiss();
                    transportStretcher = aiStretcher;
                    unit.TransportStretcher = transportStretcher;

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

                    amb.Model.GetDimensions(out Vector3 min2, out Vector3 max2);
                    rearPos = amb.GetOffsetPosition(new Vector3(0, min2.Y - 0.5f, 0));
                    if (m1.Exists()) m1.Tasks.GoStraightToPosition(rearPos, 1.5f, amb.Heading, 1.0f, 15000);
                    if (m2.Exists()) m2.Tasks.GoStraightToPosition(amb.GetOffsetPosition(new Vector3(2f, min2.Y - 0.5f, 0)), 1.5f, amb.Heading, 1.0f, 15000);

                    WaitUntilClose(m1, rearPos, 2.5f, 250);

                    GameFiber.Sleep(1500);
                    NativeFunction.Natives.CLEAR_PED_SECONDARY_TASK(m1);

                    var cfg = new Configuration.VehicleConfig(amb.Model.Name);
                    cfg.Load();

                    aiStretcher.AttachTo(amb, 0, cfg.StowPos, cfg.StowRot);
                    if (cfg.HideStretcherInVehicle) NativeFunction.Natives.SET_ENTITY_ALPHA(aiStretcher, 0, false);
                }

                ToggleAIDoors(amb, false);
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

            Game.DisplayNotification($"~b~{unit.UnitDisplayName}:~w~ Returning to service.");

            GameFiber.StartNew(delegate
            {
                var amb = unit.Ambulance;
                var m1 = unit.Medic1;
                var m2 = unit.Medic2;

                if (m1.Exists() && unit.ServiceType != "Helicopter") m1.Tasks.EnterVehicle(amb, -1);
                if (m2.Exists()) m2.Tasks.EnterVehicle(amb, 0);

                int headcountTimeout = 0;
                while (amb.Exists() && (unit.ServiceType == "Helicopter" ? !m2.IsInVehicle(amb, false) : (!m1.IsInVehicle(amb, false) || !m2.IsInVehicle(amb, false))) && headcountTimeout < 150)
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

                    if (unit.ServiceType == "Helicopter")
                    {
                        NativeFunction.CallByHash<int>(0xDAD029E187A2BEB4, m1, amb, 0, 0, amb.Position.X, amb.Position.Y, amb.Position.Z + 150f, 4, 30f, 5f, 0f, 30, 30, -1f, 0);
                    }
                    else
                    {
                        m1.Tasks.CruiseWithVehicle(amb, 15f, VehicleDrivingFlags.Normal);
                    }
                }

                uint startDismissTime = Game.GameTime;
                while (amb.Exists() && Game.LocalPlayer.Character.Exists() &&
                       Game.LocalPlayer.Character.DistanceTo(amb) < 120f &&
                       (Game.GameTime - startDismissTime) < 120000)
                {
                    GameFiber.Sleep(1000);
                }

                if (unit.UnitBlip != null && unit.UnitBlip.Exists()) unit.UnitBlip.Delete();

                if (m1.Exists()) m1.Delete();
                if (m2.Exists()) m2.Delete();
                if (amb.Exists()) amb.Delete();

                if (unit.TransportStretcher != null && unit.TransportStretcher.Exists()) unit.TransportStretcher.Delete();
            });
        }

        private static void ForceDismissAllUnits()
        {
            Cleanup();
            Game.DisplayNotification(Localization.Get("NOTIF_FAST_DISMISS", "~b~Dispatch:~w~ All backup units have been forcefully dismissed."));
        }

        public static void Cleanup()
        {
            if (_timerBarPool != null && _dispatchTimerBar != null) _timerBarPool.Remove(_dispatchTimerBar);
            if (_timerBarPool != null && _dismissTimerBar != null) _timerBarPool.Remove(_dismissTimerBar);

            foreach (var unit in ActiveUnits.ToList())
            {
                try
                {
                    if (unit.UnitBlip != null)
                    {
                        if (unit.UnitBlip.Exists()) unit.UnitBlip.Delete();
                        uint handle = unit.UnitBlip.Handle;
                        if (handle != 0) NativeFunction.Natives.REMOVE_BLIP(ref handle);
                    }

                    if (unit.Ambulance != null && unit.Ambulance.Exists()) unit.Ambulance.Delete();
                    if (unit.Medic1 != null && unit.Medic1.Exists()) unit.Medic1.Delete();
                    if (unit.Medic2 != null && unit.Medic2.Exists()) unit.Medic2.Delete();
                    if (unit.TransportStretcher != null && unit.TransportStretcher.Exists()) unit.TransportStretcher.Delete();

                    if (unit.AssignedPatient != null && unit.AssignedPatient.Character != null && unit.AssignedPatient.Character.Exists())
                    {
                        if (unit.State == AIUnitState.LoadedOnScene || unit.State == AIUnitState.Transporting)
                        {
                            unit.AssignedPatient.Character.Delete();
                            GameState.ActivePatients.Remove(unit.AssignedPatient);
                        }
                    }
                }
                catch { }
            }
            ActiveUnits.Clear();
            _unitCounter = 1;
        }

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

        private static void ToggleAIDoors(Vehicle amb, bool openDoors)
        {
            if (amb == null || !amb.Exists()) return;

            var cfg = new Configuration.VehicleConfig(amb.Model.Name);
            cfg.Load();

            foreach (int doorIndex in cfg.DoorIndices)
            {
                try
                {
                    if (doorIndex >= 0 && doorIndex <= 7)
                    {
                        var door = amb.Doors[doorIndex];
                        if (door.IsValid())
                        {
                            if (openDoors) door.Open(false);
                            else door.Close(false);
                        }
                    }
                }
                catch { }
            }
        }

        private static void OnFrameRender(object sender, GraphicsEventArgs e)
        {
            if ((_isHoldingFastDispatch || _isHoldingFastDismiss) && _timerBarPool != null)
            {
                _timerBarPool.Draw();
            }
        }
    }
}