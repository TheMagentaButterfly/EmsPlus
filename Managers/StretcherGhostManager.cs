using EmsPlus.Configuration;
using Rage;
using Rage.Native;

namespace EmsPlus.Managers
{
    public static class StretcherGhostManager
    {
        private static Rage.Object _ghostStretcher;
        private static Ped _ghostPatient;
        private static Ped _ghostMedic;
        private static Rage.Object _ghostKit;
        private static Vector3 _fixedSpawnPos = Vector3.Zero;

        public static void DeleteGhosts()
        {
            if (_ghostKit != null && _ghostKit.Exists())
            {
                _ghostKit.Detach();
                _ghostKit.Delete();
            }
            _ghostKit = null;

            if (_ghostPatient != null && _ghostPatient.Exists())
            {
                _ghostPatient.Detach();
                _ghostPatient.Delete();
            }
            _ghostPatient = null;

            if (_ghostStretcher != null && _ghostStretcher.Exists())
            {
                _ghostStretcher.Detach();
                _ghostStretcher.Delete();
            }
            _ghostStretcher = null;
            if (_ghostMedic != null && _ghostMedic.Exists())
            {
                _ghostMedic.Detach();
                _ghostMedic.Delete(); }
            _ghostMedic = null;


            _fixedSpawnPos = Vector3.Zero;
        }

        // 1. CARRY GHOST
        public static void UpdateCarryGhost()
        {
            if (StretcherManager.IsAttachedToPlayer && StretcherManager.Prop != null && StretcherManager.Prop.Exists())
            {
                StretcherManager.UpdateAttachedPosition();
                return;
            }

            if (EntryPoint.PropConfig == null || EntryPoint.OffsetConfig == null) return;

            if (_ghostStretcher == null || !_ghostStretcher.Exists())
            {
                Model m = new Model(EntryPoint.PropConfig.StretcherModel);
                m.LoadAndWait();

                _ghostStretcher = new Object(m, Game.LocalPlayer.Character.Position);
                _ghostStretcher.IsCollisionEnabled = false;
                //_ghostStretcher.Opacity = 150;
                m.Dismiss();
            }

            Ped player = Game.LocalPlayer.Character;
            var c = EntryPoint.OffsetConfig;

            NativeFunction.Natives.ATTACH_ENTITY_TO_ENTITY(
                _ghostStretcher,
                player,
                0,
                c.StretcherAttachOffsetX, c.StretcherAttachOffsetY, c.StretcherAttachOffsetZ,
                c.StretcherAttachPitch, c.StretcherAttachRoll, c.StretcherAttachYaw,
                false, false, false, false, 5, true
            );
        }

        // 2. PATIENT GHOST
        // mode: 0 = Normal, 1 = Lowered, 2 = Sitting
        public static void UpdatePatientGhost(int mode)
        {
            if (EntryPoint.PropConfig == null || EntryPoint.OffsetConfig == null) return;

            string targetModelName;
            switch (mode)
            {
                case 1: targetModelName = EntryPoint.PropConfig.StretcherModelLowered; break;
                case 2: targetModelName = EntryPoint.PropConfig.StretcherModelSitting; break;
                default: targetModelName = EntryPoint.PropConfig.StretcherModel; break;
            }

            Model targetModel = new Model(targetModelName);

            bool needRespawn = false;
            if (_ghostStretcher == null || !_ghostStretcher.Exists()) needRespawn = true;
            else if (_ghostStretcher.Model != targetModel)
            {
                _ghostStretcher.Delete();
                needRespawn = true;
            }

            if (needRespawn)
            {
                targetModel.LoadAndWait();
                Vector3 spawnPos = Game.LocalPlayer.Character.GetOffsetPosition(new Vector3(0f, 1.5f, -1f));
                _ghostStretcher = new Rage.Object(targetModel, spawnPos);
                _ghostStretcher.IsCollisionEnabled = false;
                // _ghostStretcher.Opacity = 200;
                _ghostStretcher.IsPositionFrozen = true;
                targetModel.Dismiss();
            }
            else
            {
                _ghostStretcher.IsPositionFrozen = true;
            }

            if (_ghostPatient == null || !_ghostPatient.Exists())
            {
                _ghostPatient = new Ped(_ghostStretcher.Position);
                _ghostPatient.BlockPermanentEvents = true;
                _ghostPatient.IsCollisionEnabled = false;
                // _ghostPatient.Opacity = 200;
                _ghostPatient.IsPositionFrozen = true;
            }

            string animDict = EntryPoint.AnimationConfig.PatientStretcherDict.Value;
            string animName = EntryPoint.AnimationConfig.PatientStretcherName.Value;

            if (mode == 2)
            {
                animDict = EntryPoint.AnimationConfig.PatientSittingStretcherDict.Value;
                animName = EntryPoint.AnimationConfig.PatientSittingStretcherName.Value;
            }

            if (!string.IsNullOrEmpty(animDict) && !string.IsNullOrEmpty(animName))
            {
                if (!NativeFunction.Natives.HAS_ANIM_DICT_LOADED<bool>(animDict))
                {
                    NativeFunction.Natives.REQUEST_ANIM_DICT(animDict);
                }
                else
                {
                    _ghostPatient.Tasks.PlayAnimation(animDict, animName, 8.0f, AnimationFlags.Loop);
                }
            }

            if (_ghostPatient != null && _ghostPatient.Exists())
            {
                _ghostPatient.Detach();

                var c = EntryPoint.OffsetConfig;
                float x = 0, y = 0, z = 0, p = 0, r = 0, yaw = 0;

                switch (mode)
                {
                    case 0: // Normal
                        x = c.PatientAttachOffsetX; y = c.PatientAttachOffsetY; z = c.PatientAttachOffsetZ;
                        p = c.PatientAttachPitch; r = c.PatientAttachRoll; yaw = c.PatientAttachYaw;
                        break;
                    case 1: // Lowered
                        x = c.PatientAttachLoweredOffsetX; y = c.PatientAttachLoweredOffsetY; z = c.PatientAttachLoweredOffsetZ;
                        p = c.PatientAttachLoweredPitch; r = c.PatientAttachLoweredRoll; yaw = c.PatientAttachLoweredYaw;
                        break;
                    case 2: // Sitting
                        x = c.PatientAttachSittingOffsetX; y = c.PatientAttachSittingOffsetY; z = c.PatientAttachSittingOffsetZ;
                        p = c.PatientAttachSittingPitch; r = c.PatientAttachSittingRoll; yaw = c.PatientAttachSittingYaw;
                        break;
                }

                _ghostPatient.AttachTo(_ghostStretcher, -1, new Vector3(x, y, z), new Rotator(p, r, yaw));
            }
        }

        // 3. VEHICLE GHOST
        public static void UpdateVehicleGhost(Vehicle v, VehicleConfig cfg, int mode)
        {
            if (v == null || !v.Exists() || !cfg.CanHaveStretcher)
            {
                DeleteGhosts();
                return;
            }

            // MODE 2: MEDIC SEAT
            if (mode == 2)
            {
                if (_ghostStretcher != null && _ghostStretcher.Exists()) _ghostStretcher.Delete();
                if (_ghostPatient != null && _ghostPatient.Exists()) _ghostPatient.Delete();

                if (_ghostMedic == null || !_ghostMedic.Exists())
                {
                    _ghostMedic = new Ped("s_m_m_paramedic_01", v.Position, 0f);
                    _ghostMedic.BlockPermanentEvents = true;
                    _ghostMedic.IsPositionFrozen = true;
                    _ghostMedic.IsCollisionEnabled = false;
                }

                _ghostMedic.AttachTo(v, -1, cfg.MedicPos, cfg.MedicRot);

                string dict = EntryPoint.AnimationConfig.MedicSitDict.Value;
                string name = EntryPoint.AnimationConfig.MedicSitName.Value;
                NativeFunction.Natives.REQUEST_ANIM_DICT(dict);
                if (NativeFunction.Natives.HAS_ANIM_DICT_LOADED<bool>(dict))
                {
                    _ghostMedic.Tasks.PlayAnimation(dict, name, 8.0f, AnimationFlags.Loop);
                }
            }
            // MODE 0 / 1: STRETCHER
            else
            {
                if (_ghostMedic != null && _ghostMedic.Exists()) _ghostMedic.Delete();

                string targetModelName = (mode == 1) ? EntryPoint.PropConfig.StretcherModel : EntryPoint.PropConfig.StretcherModelLowered;
                Model targetModel = new Model(targetModelName);

                if (_ghostPatient != null && _ghostPatient.Exists())
                {
                    _ghostPatient.Delete();
                    _ghostPatient = null;
                }

                bool needToSpawn = true;
                if (_ghostStretcher != null && _ghostStretcher.Exists())
                {
                    if (_ghostStretcher.Model == targetModel) needToSpawn = false;
                    else _ghostStretcher.Delete();
                }

                if (needToSpawn)
                {
                    targetModel.LoadAndWait();
                    _ghostStretcher = new Rage.Object(targetModel, v.Position);
                    _ghostStretcher.IsCollisionEnabled = false;
                    targetModel.Dismiss();
                }

                if (mode == 1) _ghostStretcher.AttachTo(v, 0, cfg.SlidePos, cfg.SlideRot);
                else _ghostStretcher.AttachTo(v, 0, cfg.StowPos, cfg.StowRot);
            }
        }
    }
}