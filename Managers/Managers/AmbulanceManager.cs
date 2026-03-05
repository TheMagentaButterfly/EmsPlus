using EmsPlus.Configuration;
using EmsPlus.Core;
using EmsPlus.Framework;
using EmsPlus.UI.NativeMenus;
using EmsPlus.UI.NativeMenus.ConfigMenu;
using Rage;
using Rage.Native;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace EmsPlus.Managers
{
    public static class AmbulanceManager
    {
        public static Vehicle CurrentVehicle { get; private set; }
        public static VehicleConfig CurrentConfig { get; private set; }
        private static Dictionary<string, VehicleConfig> ConfigCache = new Dictionary<string, VehicleConfig>();
        public static bool IsPlayerInRearCabin { get; private set; } = false;
        public static event Action OnStateUpdate;

        public static bool AreDoorsOpen { get; private set; } = false;

        private static HashSet<uint> VehiclesWithStretcher = new HashSet<uint>();
        private static HashSet<uint> KnownVehicles = new HashSet<uint>();

        public static bool IsStretcherLoaded
        {
            get { return HasStretcher(CurrentVehicle); }
        }

        public static void EnterRearCabin()
        {
            if (CurrentVehicle == null || !CurrentVehicle.Exists() || !IsStretcherLoaded) return;

            IsPlayerInRearCabin = true;
            Ped player = Game.LocalPlayer.Character;

            GameFiber.StartNew(delegate
            {
                player.IsCollisionEnabled = false;
                player.BlockPermanentEvents = true;

                player.AttachTo(CurrentVehicle, -1, CurrentConfig.MedicPos, CurrentConfig.MedicRot);

                string dict = "anim@heists@fleeca_bank@hostages@intro";
                string anim = "intro_loop_ped_a";

                NativeFunction.Natives.REQUEST_ANIM_DICT(dict);
                while (!NativeFunction.Natives.HAS_ANIM_DICT_LOADED<bool>(dict)) GameFiber.Yield();

                player.Tasks.PlayAnimation(dict, anim, 8.0f, AnimationFlags.Loop);
                OnStateUpdate?.Invoke();
            });
        }

        public static void ExitRearCabin()
        {
            IsPlayerInRearCabin = false;
            Ped player = Game.LocalPlayer.Character;

            player.Tasks.Clear();
            player.Detach();

            player.IsCollisionEnabled = true;
            player.IsPositionFrozen = false;
            player.BlockPermanentEvents = false;

            if (CurrentVehicle != null && CurrentVehicle.Exists())
            {
                Vector3 rearPos = CurrentVehicle.GetOffsetPosition(new Vector3(0, -4.0f, 0));

                float? groundZ = World.GetGroundZ(rearPos, true, true);

                player.Position = new Vector3(rearPos.X, rearPos.Y, groundZ ?? rearPos.Z);
                player.Heading = CurrentVehicle.Heading;
            }

            OnStateUpdate?.Invoke();
        }

        public static void DrawInteractionMarkers()
        {
            if (!EntryPoint.EmsPlusConfig.ShowAmbulancePrompts.Value) return;
            if (!EntryPoint.EmsPlusConfig.UseCustomInteractionPoints.Value) return;

            Vehicle veh;
            if (!TryGetClosestAmbulance(out veh) || CurrentConfig.InteractionPoints.Count == 0) return;

            bool isPlayerNearAnyPoint = false;

            for (int i = 0; i < CurrentConfig.InteractionPoints.Count; i++)
            {
                var point = CurrentConfig.InteractionPoints[i];
                Vector3 worldPos = veh.GetOffsetPosition(point.Offset);

                bool isEditing = ConfigMenuBuilder.IsEditingInteractionPoint(i);
                Color markerColor = isEditing ? Color.FromArgb(180, 255, 165, 0) : Color.FromArgb(150, 0, 150, 255);

                float markerSize = point.Scale;

                NativeFunction.Natives.DRAW_MARKER(
                    1, worldPos.X, worldPos.Y, worldPos.Z - 0.95f,
                    0, 0, 0, 0, 0, 0,
                    markerSize, markerSize, 1.0f,
                    markerColor.R, markerColor.G, markerColor.B, markerColor.A,
                    false, false, 2, false, 0, 0, false
                );

                if (Game.LocalPlayer.Character.DistanceTo(worldPos) < (markerSize / 2f) + 0.5f)
                {
                    isPlayerNearAnyPoint = true;
                }
            }

            if (isPlayerNearAnyPoint)
            {
                string keyName = EntryPoint.KeyConfig.OpenAmbulanceMenuKey.Value.ToString();
                string formattedKey = $"~w~[~y~{keyName}~w~]";
                Game.DisplayHelp(Localization.Get("HELP_OPEN_AMBULANCE_MENU", formattedKey));
            }
        }

        public static bool IsPlayerNearInteractionPoint()
        {
            Vehicle veh;
            if (!TryGetClosestAmbulance(out veh)) return false;

            if (!EntryPoint.EmsPlusConfig.UseCustomInteractionPoints.Value)
            {
                Vector3 rearPos = veh.GetOffsetPosition(new Vector3(0, -4.0f, 0));
                return Game.LocalPlayer.Character.DistanceTo(rearPos) < 2.5f;
            }

            if (CurrentConfig.InteractionPoints.Count == 0) return false;

            foreach (var point in CurrentConfig.InteractionPoints)
            {
                Vector3 worldPos = veh.GetOffsetPosition(point.Offset);
                if (Game.LocalPlayer.Character.DistanceTo(worldPos) < (point.Scale / 2f) + 0.5f)
                {
                    return true;
                }
            }
            return false;
        }

        private static void StartDoorSequence(bool walkFirst)
        {
            GameState.IsPlayerBusy = true;
            Ped player = Game.LocalPlayer.Character;

            if (MenuCore.AmbulanceMenu.Visible) MenuCore.AmbulanceMenu.Visible = false;

            GameFiber.StartNew(delegate
            {
                if (walkFirst)
                {
                    Vector3 targetPos = CurrentVehicle.GetOffsetPosition(new Vector3(0, -4.5f, 0));
                    player.Tasks.GoStraightToPosition(targetPos, 1.0f, CurrentVehicle.Heading, 0.5f, 5000);

                    while (player.DistanceTo(targetPos) > 0.6f)
                    {
                        GameFiber.Yield();
                        
                    }
                }

                string animDict = EntryPoint.AnimationConfig.InteractDict.Value;
                string animName = EntryPoint.AnimationConfig.InteractName.Value;

                NativeFunction.Natives.REQUEST_ANIM_DICT(animDict);
                while (!NativeFunction.Natives.HAS_ANIM_DICT_LOADED<bool>(animDict)) GameFiber.Yield();

                NativeFunction.Natives.TASK_TURN_PED_TO_FACE_ENTITY(player, CurrentVehicle, 1000);

                GameFiber.Sleep(600);

                player.Tasks.AchieveHeading(CurrentVehicle.Heading, 0);
                GameFiber.Sleep(200);

                player.Tasks.PlayAnimation(animDict, animName, 2.0f, AnimationFlags.None);

                GameFiber.Sleep(600);

                ToggleDoorsInternal();

                GameFiber.Sleep(1000);
                player.Tasks.Clear();

                GameState.IsPlayerBusy = false;
                OnStateUpdate?.Invoke();
                MenuCore.AmbulanceMenu.Visible = true;
            });
        }

        private static void ToggleDoorsInternal()
        {
            AreDoorsOpen = !AreDoorsOpen;
            foreach (int doorIndex in CurrentConfig.DoorIndices)
            {
                if (doorIndex >= 0 && doorIndex <= 7)
                {
                    if (AreDoorsOpen) CurrentVehicle.Doors[doorIndex].Open(false);
                    else CurrentVehicle.Doors[doorIndex].Close(false);
                }
            }
        }

        public static void ToggleDoors()
        {
            if (CurrentVehicle == null || !CurrentVehicle.Exists()) return;
            if (GameState.IsPlayerBusy) return;

            bool needToWalk = !IsPlayerAtRear();

            StartDoorSequence(needToWalk);
        }


        public static bool IsPlayerNearAmbulance()
        {
            Vehicle veh;
            if (!TryGetClosestAmbulance(out veh)) return false;

            return Game.LocalPlayer.Character.DistanceTo(veh) < 5.5f;
        }

        public static bool IsPlayerAtRear()
        {
            if (CurrentVehicle == null || !CurrentVehicle.Exists()) return false;

            Vector3 rearPos = CurrentVehicle.GetOffsetPosition(new Vector3(0, -4.0f, 0));

            return Game.LocalPlayer.Character.DistanceTo(rearPos) < 1.5f;
        }

        public static bool TryGetClosestAmbulance(out Vehicle vehicle)
        {
            vehicle = null;
            Ped player = Game.LocalPlayer.Character;

            Vehicle lastVeh = player.LastVehicle;
            if (lastVeh != null && lastVeh.Exists())
            {
                if (player.DistanceTo(lastVeh) < 15.0f && IsValidAmbulance(lastVeh))
                {
                    SetCurrentVehicle(lastVeh);
                    vehicle = lastVeh;
                    return true;
                }
            }

            try
            {
                Vehicle[] nearby = player.GetNearbyVehicles(15);
                if (nearby == null || nearby.Length == 0) return false;

                foreach (Vehicle veh in nearby)
                {
                    if (veh == null || !veh.Exists()) continue;

                    if (IsValidAmbulance(veh))
                    {
                        SetCurrentVehicle(veh);
                        vehicle = veh;
                        return true;
                    }
                }
            }
            catch (Exception ex) { Game.Console.Print("[EmsPlus] Error in TryGetClosestAmbulance: " + ex.Message); }
            return false;
        }

        private static bool IsValidAmbulance(Vehicle veh)
        {
            if (!NativeFunction.Natives.IS_ENTITY_A_VEHICLE<bool>(veh)) return false;

            Model m = veh.Model;
            if (!m.IsValid) return false;

            string modelName = m.Name.ToLower();
            return EntryPoint.EmsPlusConfig.ValidAmbulanceModels.Contains(modelName);
        }

        private static void SetCurrentVehicle(Vehicle veh)
        {
            veh.IsPersistent = true;
            CurrentVehicle = veh;
            string modelName = veh.Model.Name.ToLower();

            if (!ConfigCache.ContainsKey(modelName))
            {
                var cfg = new VehicleConfig(modelName);
                cfg.Load();
                ConfigCache.Add(modelName, cfg);
            }
            CurrentConfig = ConfigCache[modelName];

            if (!KnownVehicles.Contains(veh.Handle))
            {
                KnownVehicles.Add(veh.Handle);

                if (CurrentConfig.CanHaveStretcher)
                {
                    VehiclesWithStretcher.Add(veh.Handle);

                    if (!MenuCore.IsAnyMenuOpen)
                        CheckForDefaultStretcher();
                }
            }
        }

        public static bool GetVehicleForConfig(out Vehicle vehicle)
        {
            return InternalGetClosest(out vehicle, false);
        }

        private static bool InternalGetClosest(out Vehicle vehicle, bool enforceWhitelist)
        {
            vehicle = null;
            try
            {
                Vehicle[] nearby = Game.LocalPlayer.Character.GetNearbyVehicles(15);
                if (nearby == null || nearby.Length == 0) return false;

                foreach (Vehicle veh in nearby)
                {
                    if (veh == null || !veh.Exists()) continue;
                    if (!NativeFunction.Natives.IS_ENTITY_A_VEHICLE<bool>(veh)) continue;

                    Model m;
                    try { m = veh.Model; } catch { continue; }
                    if (m == null || !m.IsValid) continue;

                    string modelName = m.Name.ToLower();

                    if (enforceWhitelist && !EntryPoint.EmsPlusConfig.IsAllowed(modelName))
                    {
                        continue;
                    }

                    veh.IsPersistent = true;
                    CurrentVehicle = veh;

                    if (!ConfigCache.ContainsKey(modelName))
                    {
                        var cfg = new VehicleConfig(modelName);
                        cfg.Load();
                        ConfigCache.Add(modelName, cfg);
                    }

                    CurrentConfig = ConfigCache[modelName];
                    vehicle = veh;

                    if (EntryPoint.EmsPlusConfig.IsAllowed(modelName))
                    {
                        if (!KnownVehicles.Contains(veh.Handle))
                        {
                            KnownVehicles.Add(veh.Handle);
                            if (CurrentConfig.CanHaveStretcher && !MenuCore.IsAnyMenuOpen)
                                CheckForDefaultStretcher();
                        }
                    }

                    return true;
                }
            }
            catch { }
            return false;
        }

        public static void ResetVehicleState(Vehicle v)
        {
            if (v == null) return;
            if (VehiclesWithStretcher.Contains(v.Handle)) VehiclesWithStretcher.Remove(v.Handle);
            if (KnownVehicles.Contains(v.Handle)) KnownVehicles.Remove(v.Handle);
        }

        public static void CheckForDefaultStretcher()
        {
            if (CurrentVehicle == null || !CurrentVehicle.Exists()) return;
            if (CurrentConfig != null && !CurrentConfig.CanHaveStretcher)
            {
                if (VehiclesWithStretcher.Contains(CurrentVehicle.Handle))
                    VehiclesWithStretcher.Remove(CurrentVehicle.Handle);
                return;
            }
            if (!VehiclesWithStretcher.Contains(CurrentVehicle.Handle)) return;

            if (StretcherManager.Prop == null || !StretcherManager.Prop.Exists())
            {
                string modelName = EntryPoint.PropConfig.StretcherModelLowered;
                Model testModel = new Model(modelName);
                if (!testModel.IsValid)
                {
                    Game.Console.Print($"[EmsPlus] ERROR: Model '{modelName}' is not a valid vehicle/prop model!");
                    Game.DisplayNotification("~r~Error:~w~ Invalid stretcher model in Props.ini");
                    return;
                }

                StretcherManager.SpawnAtLocation(CurrentVehicle.Position, CurrentVehicle.Rotation);

                if (StretcherManager.Prop != null && StretcherManager.Prop.Exists())
                {
                    try
                    {
                        if (CurrentVehicle.Exists() && StretcherManager.Prop.Exists())
                        {
                            StretcherManager.Prop.AttachTo(CurrentVehicle, 0, CurrentConfig.StowPos, CurrentConfig.StowRot);
                        }
                    }
                    catch (Exception ex)
                    {
                        Game.Console.Print($"[EmsPlus] Failed to attach stretcher: {ex.Message}");
                    }
                }
            }
        }

        public static void LoadStretcher()
        {
            if (CurrentConfig != null && !CurrentConfig.CanHaveStretcher) return;
            if (IsStretcherLoaded) return;
            if (!StretcherManager.IsAttachedToPlayer) { Game.DisplayNotification(Localization.Get("NOTIF_MUST_HOLD_STRETCHER")); return; }
            if (CurrentVehicle == null || !CurrentVehicle.Exists()) return;
            if (!AreDoorsOpen) { return; }

            if (StretcherManager.CurrentState == StretcherManager.StretcherState.Sitting)
            {
                StretcherManager.SwitchState(StretcherManager.StretcherState.Normal);
                GameFiber.Sleep(50);
            }

            if (GameState.CurrentPatient != null && GameState.CurrentPatient.IsOnStretcher)
            {
                EmsService.SetStatus(EmsStatus.Transporting);
                HospitalManager.SetWaypointToClosest();
                Game.DisplaySubtitle(Localization.Get("SUBTITLE_TRANSPORT_PATIENT"), 5000);
            }

            InventoryManager.StoreAllKits();

            GameFiber.StartNew(delegate
            {
                Ped player = Game.LocalPlayer.Character;
                StretcherManager.DetachFromPlayer();
                if (StretcherManager.Prop != null)
                {
                    StretcherManager.SetCollapsibleState(true);
                    StretcherManager.Prop.AttachTo(CurrentVehicle, 0, CurrentConfig.SlidePos, CurrentConfig.SlideRot);
                }
                int steps = 30;
                for (int i = 0; i <= steps; i++)
                {
                    if (!CurrentVehicle.Exists()) break;
                    float t = (float)i / steps;
                    Vector3 currentPos = Vector3.Lerp(CurrentConfig.SlidePos, CurrentConfig.StowPos, t);
                    if (StretcherManager.Prop != null && StretcherManager.Prop.Exists())
                        StretcherManager.Prop.AttachTo(CurrentVehicle, 0, currentPos, CurrentConfig.StowRot);
                    GameFiber.Sleep(15);
                }
                player.Tasks.Clear();
                if (!VehiclesWithStretcher.Contains(CurrentVehicle.Handle)) VehiclesWithStretcher.Add(CurrentVehicle.Handle);
                OnStateUpdate?.Invoke();
            });
        }

        public static void UnloadStretcher()
        {
            if (!IsStretcherLoaded) return;
            if (CurrentVehicle == null || !CurrentVehicle.Exists()) return;
            if (!AreDoorsOpen) { return; }

            CheckForDefaultStretcher();
            GameFiber.StartNew(delegate
            {
                Ped player = Game.LocalPlayer.Character;
                int steps = 30;
                for (int i = 0; i <= steps; i++)
                {
                    if (!CurrentVehicle.Exists()) break;
                    float t = (float)i / steps;
                    Vector3 currentPos = Vector3.Lerp(CurrentConfig.StowPos, CurrentConfig.SlidePos, t);
                    if (StretcherManager.Prop != null && StretcherManager.Prop.Exists())
                        StretcherManager.Prop.AttachTo(CurrentVehicle, 0, currentPos, CurrentConfig.StowRot);
                    GameFiber.Sleep(15);
                }
                StretcherManager.SetCollapsibleState(false);
                StretcherManager.AttachToPlayer();
                player.Tasks.Clear();
                VehiclesWithStretcher.Remove(CurrentVehicle.Handle);
                OnStateUpdate?.Invoke();
            });
        }

        public static bool HasStretcher(Vehicle v)
        {
            if (v == null || !v.Exists()) return false;

            if (StretcherManager.Prop != null && StretcherManager.Prop.Exists())
            {
                uint parentHandle = NativeFunction.Natives.GET_ENTITY_ATTACHED_TO<uint>(StretcherManager.Prop);
                if (parentHandle == v.Handle.Value) return true;
            }

            return VehiclesWithStretcher.Contains(v.Handle);
        }

        public static void Cleanup()
        {
            if (IsPlayerInRearCabin)
            {
                Ped player = Game.LocalPlayer.Character;
                if (player != null && player.Exists())
                {
                    player.Detach();
                    player.Tasks.Clear();
                    player.IsCollisionEnabled = true;
                    player.BlockPermanentEvents = false;

                    if (CurrentVehicle != null && CurrentVehicle.Exists())
                    {
                        Vector3 rearPos = CurrentVehicle.GetOffsetPosition(new Vector3(0, -4.0f, 0));
                        player.Position = rearPos;
                    }
                }
                IsPlayerInRearCabin = false;
            }
        }
    }
}