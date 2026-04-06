using EmsPlus.Core;
using Rage;
using Rage.Native;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace EmsPlus.Managers
{
    public static class StretcherManager
    {
        public static Rage.Object Prop { get; private set; }
        public static bool IsAttachedToPlayer { get; private set; } = false;
        public static event Action OnUpdate;
        public enum StretcherState { Normal, Low, Sitting }
        public static StretcherState CurrentState { get; private set; } = StretcherState.Normal;

        private static uint _lastGrabTime = 0;
        private static uint _lastHeightTime = 0;
        private static uint _lastSitTime = 0;

        private static bool IsGrabPressed() => EntryPoint.KeyConfig.StretcherGrabKey?.Value?.IsPressed ?? Game.IsKeyDown(Keys.G);
        private static bool IsHeightPressed() => EntryPoint.KeyConfig.StretcherHeightKey?.Value?.IsPressed ?? Game.IsKeyDown(Keys.H);
        private static bool IsSitPressed() => EntryPoint.KeyConfig.StretcherSitKey?.Value?.IsPressed ?? Game.IsKeyDown(Keys.J);

        // --- PROPERTIES ---
        public static bool IsAttachedToVehicle
        {
            get
            {
                if (Prop != null && Prop.Exists())
                {
                    uint parentHandle = NativeFunction.Natives.GET_ENTITY_ATTACHED_TO<uint>(Prop);
                    if (parentHandle != 0)
                    {
                        if (parentHandle == Game.LocalPlayer.Character.Handle.Value) return false;
                        return true;
                    }
                }
                return false;
            }
        }

        // --- PROCESS LOOP ---
        public static void Process()
        {
            if (Prop == null || !Prop.Exists()) return;
            if (IsAttachedToVehicle) return;

            Prop.Model.GetDimensions(out Vector3 min, out Vector3 max);
            float radius = Math.Max(max.X, max.Y) + 1.2f;

            if (Game.LocalPlayer.Character.DistanceTo(Prop) < radius)
            {
                if (IsGrabPressed() && Game.GameTime > _lastGrabTime + 500)
                {
                    _lastGrabTime = Game.GameTime;
                    if (IsAttachedToPlayer) DetachFromPlayer(); else AttachToPlayer();
                }

                if (IsHeightPressed() && Game.GameTime > _lastHeightTime + 500)
                {
                    _lastHeightTime = Game.GameTime;
                    if (CurrentState == StretcherState.Low)
                        SwitchState(StretcherState.Normal);
                    else
                        SwitchState(StretcherState.Low);
                }

                if (IsSitPressed() && Game.GameTime > _lastSitTime + 500 && CurrentState != StretcherState.Low)
                {
                    _lastSitTime = Game.GameTime;
                    if (CurrentState == StretcherState.Sitting)
                        SwitchState(StretcherState.Normal);
                    else
                        SwitchState(StretcherState.Sitting);
                }
            }
        }


        // --- SWAP FUNCTION ---
        public static void SwitchState(StretcherState newState)
        {
            if (CurrentState == newState) return;
            if (Prop == null || !Prop.Exists()) return;

            Vector3 pos = Prop.Position;
            Rotator rot = Prop.Rotation;
            bool wasHolding = IsAttachedToPlayer;

            Ped patient = (GameState.CurrentPatient != null && GameState.CurrentPatient.IsOnStretcher) ? GameState.CurrentPatient.Character : null;

            if (wasHolding) Prop.Detach();
            if (patient != null && patient.Exists()) patient.Detach();

            Prop.Delete();

            string modelName = "";
            switch (newState)
            {
                case StretcherState.Normal: modelName = EntryPoint.PropConfig.StretcherModel; break;
                case StretcherState.Low: modelName = EntryPoint.PropConfig.StretcherModelLowered; break;
                case StretcherState.Sitting: modelName = EntryPoint.PropConfig.StretcherModelSitting; break;
            }

            Model m = new Model(modelName);
            m.LoadAndWait();
            Prop = new Rage.Object(m, pos);
            Prop.Rotation = rot;
            m.Dismiss();

            CurrentState = newState;

            if (patient != null && patient.Exists())
            {
                AttachPatientToStretcher(patient, newState);
            }
            if (wasHolding)
            {
                AttachToPlayer();
            }
            else
            {
                Vector3 rayStart = Prop.Position + new Vector3(0, 0, 1.0f);
                Vector3 rayEnd = Prop.Position - new Vector3(0, 0, 2.0f);

                HitResult hit = World.TraceLine(rayStart, rayEnd, TraceFlags.IntersectWorld, Prop);

                if (hit.Hit)
                {
                    Prop.Position = hit.HitPosition;
                }

                Prop.IsPositionFrozen = true;
                Prop.IsCollisionEnabled = true;
            }
        }

        private static void AttachPatientToStretcher(Ped p, StretcherState state)
        {
            var c = EntryPoint.OffsetConfig;
            var a = EntryPoint.AnimationConfig;

            float x = 0, y = 0, z = 0, pitch = 0, roll = 0, yaw = 0;
            string animDict = "", animName = "";

            switch (state)
            {
                case StretcherState.Sitting:
                    x = c.PatientAttachSittingOffsetX; y = c.PatientAttachSittingOffsetY; z = c.PatientAttachSittingOffsetZ;
                    pitch = c.PatientAttachSittingPitch; roll = c.PatientAttachSittingRoll; yaw = c.PatientAttachSittingYaw;
                    animDict = a.PatientSittingStretcherDict.Value;
                    animName = a.PatientSittingStretcherName.Value;
                    break;
                case StretcherState.Low:
                    x = c.PatientAttachLoweredOffsetX; y = c.PatientAttachLoweredOffsetY; z = c.PatientAttachLoweredOffsetZ;
                    pitch = c.PatientAttachLoweredPitch; roll = c.PatientAttachLoweredRoll; yaw = c.PatientAttachLoweredYaw;
                    animDict = a.PatientStretcherDict.Value;
                    animName = a.PatientStretcherName.Value;
                    break;
                default:
                    x = c.PatientAttachOffsetX; y = c.PatientAttachOffsetY; z = c.PatientAttachOffsetZ;
                    pitch = c.PatientAttachPitch; roll = c.PatientAttachRoll; yaw = c.PatientAttachYaw;
                    animDict = a.PatientStretcherDict.Value;
                    animName = a.PatientStretcherName.Value;
                    break;
            }

            if (string.IsNullOrEmpty(animDict) || string.IsNullOrEmpty(animName))
            {
                Game.Console.Print("[EmsPlus] Warning: Missing animation config for state " + state + ". Using default.");
                animDict = "amb@world_human_sunbathe@female@back@base";
                animName = "base";
            }

            p.AttachTo(Prop, -1, new Vector3(x, y, z), new Rotator(pitch, roll, yaw));

            GameFiber.StartNew(delegate
            {
                if (NativeFunction.Natives.DOES_ANIM_DICT_EXIST<bool>(animDict))
                {
                    NativeFunction.Natives.REQUEST_ANIM_DICT(animDict);
                    int t = 0;
                    while (!NativeFunction.Natives.HAS_ANIM_DICT_LOADED<bool>(animDict) && t < 100)
                    {
                        GameFiber.Sleep(10);
                        t++;
                    }

                    if (p.Exists())
                    {
                        p.Tasks.PlayAnimation(animDict, animName, 8.0f, AnimationFlags.Loop);
                    }
                }
                else
                {
                    Game.Console.Print($"[EmsPlus] CRITICAL: Animation Dictionary '{animDict}' does not exist in GTA V.");
                }
            });
        }

        // --- ATTACHMENT LOGIC ---
        public static void AttachToPlayer()
        {
            Prop.IsPositionFrozen = false;
            GameFiber.Yield();

            Ped player = Game.LocalPlayer.Character;

            int boneIndex = 0;

            var c = EntryPoint.OffsetConfig;

            NativeFunction.Natives.ATTACH_ENTITY_TO_ENTITY(
                Prop,
                player,
                boneIndex,
                c.StretcherAttachOffsetX, c.StretcherAttachOffsetY, c.StretcherAttachOffsetZ,
                c.StretcherAttachPitch, c.StretcherAttachRoll, c.StretcherAttachYaw,
                false, false, false, false, 5, true
            );

            IsAttachedToPlayer = true;
            OnUpdate?.Invoke();

            GameFiber.StartNew(delegate
            {
                string animDict = EntryPoint.AnimationConfig.MedicStretcherCarryDict.Value;
                string animName = EntryPoint.AnimationConfig.MedicStretcherCarryName.Value;
                NativeFunction.Natives.REQUEST_ANIM_DICT(animDict);
                while (!NativeFunction.Natives.HAS_ANIM_DICT_LOADED<bool>(animDict)) GameFiber.Yield();

                if (IsAttachedToPlayer)
                {
                    player.Tasks.PlayAnimation(animDict, animName, 8.0f, AnimationFlags.Loop | AnimationFlags.UpperBodyOnly | AnimationFlags.SecondaryTask);
                }
            });
        }

        public static void UpdateAttachedPosition()
        {
            if (Prop == null || !Prop.Exists()) return;

            int bone = Game.LocalPlayer.Character.GetBoneIndex(PedBoneId.SpineRoot);
            var c = EntryPoint.OffsetConfig;

            Prop.AttachTo(Game.LocalPlayer.Character, bone,
                new Vector3(c.StretcherAttachOffsetX, c.StretcherAttachOffsetY, c.StretcherAttachOffsetZ),
                new Rotator(c.StretcherAttachPitch, c.StretcherAttachRoll, c.StretcherAttachYaw));
        }

        public static void DetachFromPlayer()
        {
            if (Prop == null || !Prop.Exists()) return;

            Prop.Detach();
            IsAttachedToPlayer = false;

            NativeFunction.Natives.CLEAR_PED_SECONDARY_TASK(Game.LocalPlayer.Character);

            float currentYaw = Prop.Rotation.Yaw;

            Vector3 startPos = Prop.Position + new Vector3(0, 0, 0.5f);
            Vector3 endPos = Prop.Position - new Vector3(0, 0, 5.0f);

            HitResult hit = World.TraceLine(startPos, endPos, TraceFlags.IntersectWorld);

            if (hit.Hit)
            {
                Prop.Position = hit.HitPosition;

                Prop.Rotation = new Rotator(0f, 0f, currentYaw);
            }
            else
            {
                Prop.IsCollisionEnabled = true;
                Prop.IsPositionFrozen = false;

                Prop.Rotation = new Rotator(0f, 0f, currentYaw);

                GameFiber.Sleep(500);
            }

            Prop.IsPositionFrozen = true;
            Prop.IsCollisionEnabled = true;

            OnUpdate?.Invoke();
        }

        // --- SPAWNING LOGIC ---
        public static void Deploy()
        {
            if (Prop != null && Prop.Exists()) return;

            string modelName = EntryPoint.PropConfig.StretcherModel;
            Vector3 spawnPos = Game.LocalPlayer.Character.GetOffsetPosition(new Vector3(0, 2.0f, 0));
            float heading = Game.LocalPlayer.Character.Heading;

            Prop = SafeSpawn(modelName, spawnPos, heading);

            if (Prop != null)
            {
                CurrentState = StretcherState.Normal;
            }
        }
        public static void SpawnAtLocation(Vector3 pos, Rotator rot)
        {
            if (Prop != null && Prop.Exists()) return;

            string modelName = EntryPoint.PropConfig.StretcherModelLowered;
            Model model = new Model(modelName);

            model.LoadAndWait();

            if (model.IsValid && model.IsLoaded)
            {
                Prop = new Rage.Object(model, pos);
                if (Prop.Exists())
                {
                    Prop.Rotation = rot;
                    Prop.IsPositionFrozen = true;
                    Prop.IsCollisionEnabled = false;
                    Prop.IsPersistent = true;
                    CurrentState = StretcherState.Low;
                }
            }
            else
            {
                Game.Console.Print($"[EmsPlus] Error: Could not spawn stretcher. Model '{modelName}' failed to load.");
            }

            model.Dismiss();
        }

        public static void SetCollapsibleState(bool isLowered)
        {
            if (Prop == null || !Prop.Exists()) return;

            Vector3 pos = Prop.Position;
            Rotator rot = Prop.Rotation;
            bool wasAttached = IsAttachedToPlayer;
            Ped patient = (GameState.CurrentPatient != null && GameState.CurrentPatient.IsOnStretcher) ? GameState.CurrentPatient.Character : null;

            if (wasAttached) DetachFromPlayer();
            if (patient != null && patient.Exists()) patient.Detach();

            Prop.Delete();

            string modelName = isLowered ? EntryPoint.PropConfig.StretcherModelLowered : EntryPoint.PropConfig.StretcherModel;

            Prop = SafeSpawn(modelName, pos, rot.Yaw);

            if (Prop == null) return;

            Prop.Rotation = rot;
            CurrentState = isLowered ? StretcherState.Low : StretcherState.Normal;

            if (patient != null && patient.Exists())
            {
                var c = EntryPoint.OffsetConfig;
                float x = isLowered ? c.PatientAttachLoweredOffsetX : c.PatientAttachOffsetX;
                float y = isLowered ? c.PatientAttachLoweredOffsetY : c.PatientAttachOffsetY;
                float z = isLowered ? c.PatientAttachLoweredOffsetZ : c.PatientAttachOffsetZ;
                float p = isLowered ? c.PatientAttachLoweredPitch : c.PatientAttachPitch;
                float r = isLowered ? c.PatientAttachLoweredRoll : c.PatientAttachRoll;
                float yaw = isLowered ? c.PatientAttachLoweredYaw : c.PatientAttachYaw;

                patient.AttachTo(Prop, -1, new Vector3(x, y, z), new Rotator(p, r, yaw));
            }

            if (wasAttached) AttachToPlayer();
        }

        // --- HELPER: SAFE SPAWN ---
        private static Rage.Object SafeSpawn(string modelName, Vector3 targetPos, float heading)
        {
            Model model = new Model(modelName);
            model.LoadAndWait();

            if (!model.IsValid)
            {
                Game.Console.Print($"[EmsPlus] ERROR: Invalid model '{modelName}'");
                return null;
            }

            Rage.Object obj = new Rage.Object(model, targetPos);

            if (!obj.Exists()) return null;

            obj.IsPositionFrozen = true;
            obj.IsCollisionEnabled = false;

            obj.Model.GetDimensions(out Vector3 min, out Vector3 max);
            Vector3 centerOffset = (min + max) / 2.0f;

            float groundZ = targetPos.Z;
            if (NativeFunction.Natives.GET_GROUND_Z_FOR_3D_COORD<bool>(targetPos.X, targetPos.Y, targetPos.Z + 2f, out float zResult, false))
            {
                groundZ = zResult;
            }

            float verticalAdjustment = Math.Abs(min.Z);
            obj.Position = new Vector3(targetPos.X, targetPos.Y, groundZ + verticalAdjustment);
            obj.Rotation = new Rotator(0, 0, heading);

            obj.IsCollisionEnabled = true;
            obj.IsPersistent = true;

            model.Dismiss();
            return obj;
        }

        public static void DrawPrompt(Rage.Graphics g)
        {
            if (Prop == null || !Prop.Exists()) return;
            if (IsAttachedToVehicle) return;

            Prop.Model.GetDimensions(out Vector3 min, out Vector3 max);
            float radius = Math.Max(max.X, max.Y) + 1.2f;

            if (Game.LocalPlayer.Character.DistanceTo(Prop) < radius)
            {
                Vector3 promptPos = Prop.GetOffsetPosition(new Vector3(0, 0, max.Z + 0.35f));

                string grabKeyStr = EntryPoint.KeyConfig.StretcherGrabKey?.Value?.ToString() ?? "G";
                string heightKeyStr = EntryPoint.KeyConfig.StretcherHeightKey?.Value?.ToString() ?? "H";
                string sitKeyStr = EntryPoint.KeyConfig.StretcherSitKey?.Value?.ToString() ?? "J";

                string text = IsAttachedToPlayer ? Localization.Get("PROMPT_RELEASE_STRETCHER") : Localization.Get("PROMPT_GRAB_STRETCHER");

                if (CurrentState == StretcherState.Low)
                {
                    text += " | " + Localization.Get("PROMPT_RAISE_STRETCHER");
                }
                else
                {
                    text += " | " + Localization.Get("PROMPT_LOWER_STRETCHER");
                }

                if (CurrentState != StretcherState.Low)
                {
                    text += " | " + Localization.Get("PROMPT_SITTING_STRETCHER");
                }

                text = text.Replace("[G]", $"[{grabKeyStr}]")
                           .Replace("[H]", $"[{heightKeyStr}]")
                           .Replace("[J]", $"[{sitKeyStr}]");

                UI.Helpers.RenderHelper.Draw3DPrompt(g, text, promptPos, Color.Blue);
            }
        }

        public static void Cleanup()
        {
            if (Prop != null && Prop.Exists()) Prop.Delete();
            Prop = null;
            IsAttachedToPlayer = false;
            StretcherGhostManager.DeleteGhosts();
        }
    }
}