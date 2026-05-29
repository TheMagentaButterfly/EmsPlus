using EmsPlus.Managers;
using EmsPlus.Medical;
using Rage;
using Rage.Native;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EmsPlus
{
    [Serializable]
    public class Patient
    {
        public Ped Character { get; set; }
        public PatientDetails Details { get; set; } = new PatientDetails();
        public bool HasBeenSpokenTo { get; set; } = false;
        public List<DialogueLine> Dialogue { get; set; } = new List<DialogueLine>();
        public string DispatchDiagnosis { get; set; } = "Unknown Medical Emergency";

        public ConsciousnessLevel Consciousness { get; set; } = ConsciousnessLevel.Unresponsive;
        public VitalState HeartRate { get; set; } = VitalState.Normal;
        public VitalState BloodPressure { get; set; } = VitalState.Normal;
        public VitalState SpO2 { get; set; } = VitalState.Normal;
        public VitalState Respiration { get; set; } = VitalState.Normal;
        public VitalState BloodGlucose { get; set; } = VitalState.Normal;

        private uint _lastTick = 0;
        public float BloodVolume { get; set; } = 100f;
        public float BrainOxygen { get; set; } = 100f;

        public List<MedicalCondition> Conditions { get; private set; } = new List<MedicalCondition>();
        public HashSet<PedBoneId> InspectedBones { get; set; } = new HashSet<PedBoneId>();

        public bool IsBoneInspected(PedBoneId bone) => InspectedBones.Contains(bone);
        public void MarkBoneInspected(PedBoneId bone) => InspectedBones.Add(bone);

        public bool PlayerArrived { get; set; } = false;
        public bool IsStabilized => Conditions.All(c => c.IsTreated);
        public bool IsEcgsConnected { get; set; } = false;
        public bool IsBpCuffConnected { get; set; } = false;
        public bool IsOnStretcher { get; set; } = false;
        public bool IsReceivingOxygen { get; set; } = false;
        public bool IsIVEstablished { get; set; } = false;
        public bool IsReceivingFluids { get; set; } = false;
        public bool IsBglChecked { get; set; } = false;
        public bool IsCCollarApplied { get; set; } = false;
        public bool IsLimbSplinted { get; set; } = false;
        public bool IsCardiacArrest { get; internal set; }
        public bool IsDead { get; private set; }
        public string Mood { get; set; } = "Neutral";

        public Patient(Ped ped)
        {
            Character = ped;

            if (Character != null && Character.Exists())
            {
                Character.BlockPermanentEvents = true;

                Character.IsInvincible = true;
                Character.CanRagdoll = false;

                NativeFunction.Natives.SET_PED_CAN_RAGDOLL<bool>(Character, false);
                NativeFunction.Natives.SET_PED_CAN_RAGDOLL_FROM_PLAYER_IMPACT<bool>(Character, false);
                NativeFunction.Natives.SET_PED_CAN_EVASIVE_DIVE<bool>(Character, false);
                NativeFunction.Natives.SET_PED_CAN_PLAY_AMBIENT_ANIMS<bool>(Character, false);
                NativeFunction.Natives.SET_PED_CAN_PLAY_AMBIENT_BASE_ANIMS<bool>(Character, false);

                NativeFunction.Natives.SET_PED_CONFIG_FLAG(Character, 281, true);
            }

            Details.GenerateRandom(ped.IsMale);
            PlayStateAnimation();
        }

        private void RevivePatient()
        {
            Consciousness = ConsciousnessLevel.Alert;
            GameFiber.StartNew(delegate
            {
                if (Character != null && Character.Exists())
                {
                    Character.Health = 100;
                    Character.IsPositionFrozen = false;

                    Character.IsInvincible = true;
                    Character.CanRagdoll = false;
                    Character.BlockPermanentEvents = true;

                    NativeFunction.Natives.SET_PED_CAN_RAGDOLL<bool>(Character, true);
                    NativeFunction.Natives.SET_PED_CAN_RAGDOLL_FROM_PLAYER_IMPACT<bool>(Character, true);
                    NativeFunction.Natives.SET_PED_CAN_EVASIVE_DIVE<bool>(Character, true);
                    NativeFunction.Natives.SET_PED_CAN_PLAY_AMBIENT_ANIMS<bool>(Character, true);
                    NativeFunction.Natives.SET_PED_CAN_PLAY_AMBIENT_BASE_ANIMS<bool>(Character, true);
                    NativeFunction.Natives.SET_PED_CONFIG_FLAG(Character, 281, false);
                }

                PlayStateAnimation();
                Game.DisplayNotification(Localization.Get("NOTIF_PATIENT_REGAINED_CONSCIOUSNESS", "~g~Patient has regained consciousness."));
            });
        }

        public void Update()
        {
            if (IsDead || IsStabilized) return;

            if (Game.GameTime - _lastTick < 1000) return;
            _lastTick = Game.GameTime;

            if (!PlayerArrived)
            {
                if (Game.LocalPlayer.Character.DistanceTo(Character) < 100f)
                {
                    PlayerArrived = true;
                }
                else
                {
                    return;
                }
            }

            float degSpeed = EntryPoint.EmsPlusConfig.DegradationSpeed.Value * 0.05f;

            foreach (var condition in Conditions.Where(c => !c.IsTreated))
            {
                condition.Update(this);
            }

            float bleedRate = Conditions.OfType<PhysicalInjury>().Where(i => !i.IsTreated).Sum(i => i.BleedSeverity);

            BloodVolume -= (bleedRate * degSpeed);

            if (IsCardiacArrest)
            {
                BrainOxygen -= (degSpeed * 1.5f);
            }

            if (BloodVolume <= 0f || BrainOxygen <= 0f)
            {
                BloodVolume = System.Math.Max(0f, BloodVolume);
                BrainOxygen = System.Math.Max(0f, BrainOxygen);
                IsDead = true;
                HeartRate = VitalState.None;
                BloodPressure = VitalState.None;
                SpO2 = VitalState.None;
                Consciousness = ConsciousnessLevel.Unresponsive;

                Game.DisplayNotification(Localization.Get("NOTIF_PATIENT_FLATLINED", "~b~Dispatch:~r~ Patient has flatlined. Time of death recorded."));
            }
        }

        public void ApplyTreatment(EmsTreatment treatment, PedBoneId? targetBone = null)
        {
            if (IsDead) { Game.DisplayNotification(Localization.Get("NOTIF_CANNOT_TREAT_DECEASED", "~r~Cannot treat a deceased patient.")); return; }

            bool treatmentWasEffective = AnatomicalRegistry.IsUniversalTreatment(treatment);
            bool isLocalized = AnatomicalRegistry.IsLocalizedTreatment(treatment);

            if (treatment == EmsTreatment.CPR && IsCardiacArrest)
            {
                BrainOxygen += 15f;
                if (BrainOxygen > 100f) BrainOxygen = 100f;
                treatmentWasEffective = true;

                if (new Random().Next(0, 100) < 10 && BloodVolume > 20f)
                {
                    IsCardiacArrest = false;
                    HeartRate = VitalState.Low;
                    BloodPressure = VitalState.CriticalLow;
                    Game.DisplayNotification(Localization.Get("NOTIF_PULSE_RESTORED", "~g~PULSE RESTORED!~w~ Keep monitoring."));
                }
            }

            foreach (var condition in Conditions.Where(c => !c.IsTreated).ToList())
            {
                var required = condition.RequiredTreatments.FirstOrDefault(req => IsTreatmentMatch(req, treatment));
                if (required != EmsTreatment.None)
                {
                    if (condition is PhysicalInjury injury && isLocalized)
                    {
                        if (targetBone.HasValue && injury.Bone == targetBone.Value)
                        {
                            condition.OnTreated(this, required);
                            treatmentWasEffective = true;
                        }
                    }
                    else
                    {
                        condition.OnTreated(this, required);
                        treatmentWasEffective = true;
                    }
                }
                condition.ReactToTreatment(this, treatment);
            }

            if (IsDead) return;

            if (treatmentWasEffective)
            {
                Managers.InventoryManager.ConsumeSupply(treatment);

                if (IsStabilized)
                {
                    if (Consciousness != ConsciousnessLevel.Alert && !IsCardiacArrest) RevivePatient();
                }
            }
        }

        private bool IsTreatmentMatch(EmsTreatment required, EmsTreatment applied)
        {
            if (required == applied) return true;
            if (required == EmsTreatment.Bandage && (applied == EmsTreatment.BurnDressing || applied == EmsTreatment.WoundPacking || applied == EmsTreatment.WetDressing)) return true;
            if (required == EmsTreatment.Tourniquet && applied == EmsTreatment.JunctionalTourniquet) return true;
            return false;
        }

        public void ApplyVisuals() => PatientVisuals.ApplyBloodEffects(this);

        public void PlayStateAnimation()
        {
            if (Character == null || !Character.Exists()) return;

            string dict = "";
            string name = "";

            if (IsDead)
            {
                dict = EntryPoint.AnimationConfig.PatientUnconDict.Value;
                name = EntryPoint.AnimationConfig.PatientUnconName.Value;
            }
            else if (Consciousness == ConsciousnessLevel.Unresponsive)
            {
                dict = EntryPoint.AnimationConfig.PatientUnconDict.Value;
                name = EntryPoint.AnimationConfig.PatientUnconName.Value;
            }
            else if (Consciousness == ConsciousnessLevel.Pain)
            {
                dict = EntryPoint.AnimationConfig.PatientHunchedDict.Value;
                name = EntryPoint.AnimationConfig.PatientHunchedName.Value;
            }
            else
            {
                dict = EntryPoint.AnimationConfig.PatientReviveDict.Value;
                name = EntryPoint.AnimationConfig.PatientReviveName.Value;
            }

            GameFiber.StartNew(delegate
            {
                NativeFunction.Natives.REQUEST_ANIM_DICT(dict);
                int timeout = 0;
                while (!NativeFunction.Natives.HAS_ANIM_DICT_LOADED<bool>(dict) && timeout < 200)
                {
                    GameFiber.Sleep(10);
                    timeout++;
                }

                if (Character.Exists())
                {
                    Character.Tasks.PlayAnimation(dict, name, 8.0f, AnimationFlags.Loop);
                }
            });
        }
    }
}