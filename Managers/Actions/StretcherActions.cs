using EmsPlus.Core;
using Rage;
using Rage.Native;
using System;

namespace EmsPlus.Managers.Actions
{
    public static class StretcherActions
    {
        public static void DeployStretcher()
        {
            GameState.IsPlayerBusy = true;
            GameFiber.StartNew(delegate {
                StretcherManager.Deploy();
                GameState.IsPlayerBusy = false;
            });
        }

        public static void LoadPatient()
        {
            if (AmbulanceManager.IsStretcherLoaded)
            {
                Game.DisplayNotification(Localization.Get("ERR_STRETCHER_IN_AMBULANCE", "~r~Cannot load patient:~w~ stretcher is already in ambulance."));
                return;
            }

            var c = EntryPoint.OffsetConfig;
            var a = EntryPoint.AnimationConfig;

            float x = 0, y = 0, z = 0, p = 0, r = 0, yaw = 0;
            string targetDict = "", targetName = "";

            switch (StretcherManager.CurrentState)
            {
                case StretcherManager.StretcherState.Sitting:
                    x = c.PatientAttachSittingOffsetX; y = c.PatientAttachSittingOffsetY; z = c.PatientAttachSittingOffsetZ;
                    p = c.PatientAttachSittingPitch; r = c.PatientAttachSittingRoll; yaw = c.PatientAttachSittingYaw;
                    targetDict = a.PatientSittingStretcherDict.Value;
                    targetName = a.PatientSittingStretcherName.Value;
                    break;
                case StretcherManager.StretcherState.Low:
                    x = c.PatientAttachLoweredOffsetX; y = c.PatientAttachLoweredOffsetY; z = c.PatientAttachLoweredOffsetZ;
                    p = c.PatientAttachLoweredPitch; r = c.PatientAttachLoweredRoll; yaw = c.PatientAttachLoweredYaw;
                    targetDict = a.PatientStretcherDict.Value;
                    targetName = a.PatientStretcherName.Value;
                    break;
                default: // Normal
                    x = c.PatientAttachOffsetX; y = c.PatientAttachOffsetY; z = c.PatientAttachOffsetZ;
                    p = c.PatientAttachPitch; r = c.PatientAttachRoll; yaw = c.PatientAttachYaw;
                    targetDict = a.PatientStretcherDict.Value;
                    targetName = a.PatientStretcherName.Value;
                    break;
            }

            ActionsCore.Run("", 4000, "amb@medic@standing@tendtodead@base", "base", () => {
                Ped pat = GameState.CurrentPatient.Character;

                NativeFunction.Natives.REQUEST_ANIM_DICT(targetDict);
                int timeout = 0;
                while (!NativeFunction.Natives.HAS_ANIM_DICT_LOADED<bool>(targetDict) && timeout < 200)
                {
                    GameFiber.Sleep(10);
                    timeout++;
                }

                pat.Tasks.PlayAnimation(targetDict, targetName, 8.0f, AnimationFlags.Loop);

                pat.AttachTo(StretcherManager.Prop, -1, new Vector3(x, y, z), new Rotator(p, r, yaw));

                GameState.CurrentPatient.IsOnStretcher = true;
            });
        }

        public static void UnloadPatient()
        {
            ActionsCore.Run("", 3000, "amb@medic@standing@tendtodead@base", "base", () => {

                if (GameState.CurrentPatient == null || !GameState.CurrentPatient.Character.Exists() ||
                    StretcherManager.Prop == null || !StretcherManager.Prop.Exists())
                {
                    return;
                }

                Ped patient = GameState.CurrentPatient.Character;
                var stretcher = StretcherManager.Prop;

                Rotator parallelRotation = new Rotator(0f, 0f, stretcher.Rotation.Yaw);

                Vector3 targetXY = stretcher.Position + (stretcher.RightVector * 1.2f);

                patient.Detach();

                float safeZ = Math.Max(stretcher.Position.Z, Game.LocalPlayer.Character.Position.Z) + 0.5f;

                patient.Position = new Vector3(targetXY.X, targetXY.Y, safeZ);
                patient.Rotation = parallelRotation;

                patient.IsCollisionEnabled = true;
                patient.IsPositionFrozen = false;

                GameFiber.StartNew(delegate
                {
                    int timeout = 0;
                    while (timeout < 500)
                    {
                        GameFiber.Sleep(10);
                        timeout += 10;

                        if (patient.Velocity.Length() < 0.1f && patient.HeightAboveGround < 0.2f)
                        {
                            break;
                        }
                    }

                    if (patient.Exists())
                    {
                        patient.IsPositionFrozen = true;

                        string animDict = EntryPoint.AnimationConfig.PatientUnconDict.Value;
                        string animName = EntryPoint.AnimationConfig.PatientUnconName.Value;
                        NativeFunction.Natives.REQUEST_ANIM_DICT(animDict);
                        if (NativeFunction.Natives.HAS_ANIM_DICT_LOADED<bool>(animDict))
                        {
                            patient.Tasks.PlayAnimation(animDict, animName, 8.0f, AnimationFlags.Loop);
                        }
                    }
                });

                GameState.CurrentPatient.IsOnStretcher = false;
            });
        }
    }
}