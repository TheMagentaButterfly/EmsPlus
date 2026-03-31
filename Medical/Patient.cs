using EmsPlus.Managers;
using Rage;
using Rage.Native;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EmsPlus.Medical
{
    [Serializable]
    public class Patient
    {
        public Ped Character { get; set; }
        public PatientHistory History { get; set; } = new PatientHistory();

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

        public Patient(Ped ped)
        {
            Character = ped;
            if (Character != null && Character.Exists())
            {
                Character.CanRagdoll = false;
                Character.BlockPermanentEvents = true;
            }
        }

        public void Update()
        {
            if (IsDead || IsStabilized) return;

            if (Game.GameTime - _lastTick < 1000) return;
            _lastTick = Game.GameTime;

            float degSpeed = EntryPoint.EmsPlusConfig.DegradationSpeed.Value * 0.5f;

            foreach (var condition in Conditions.Where(c => !c.IsTreated))
            {
                condition.Update(this);
            }

            if (IsCardiacArrest || SpO2 == VitalState.None || SpO2 == VitalState.CriticalLow)
            {
                HeartRate = VitalState.None;
                BloodPressure = VitalState.None;
                SpO2 = VitalState.None;

                BrainOxygen -= (1.5f * degSpeed);

                if (BrainOxygen <= 0f && EntryPoint.EmsPlusConfig.EnablePatientDeath.Value)
                {
                    IsDead = true;
                    Consciousness = ConsciousnessLevel.Unresponsive;
                    Game.DisplayNotification("~b~Patient has ~r~died~b~.");

                    if (Character.Exists() && Character.IsAlive) Character.Kill();
                }
            }
        }

        public void ApplyTreatment(EmsTreatment treatment, PedBoneId? targetBone = null)
        {
            if (IsDead)
            {
                Game.DisplayNotification("~r~Cannot treat a deceased patient.");
                return;
            }

            bool treatmentWasEffective = false;
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
                    Game.DisplayNotification("~g~PULSE RESTORED!~w~ Keep monitoring.");
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
                InventoryManager.ConsumeSupply(treatment);

                if (IsStabilized)
                {
                    if (Consciousness != ConsciousnessLevel.Alert && !IsCardiacArrest) RevivePatient();
                }
            }
        }

        private bool IsTreatmentMatch(EmsTreatment required, EmsTreatment applied)
        {
            if (required == applied) return true;

            if (required == EmsTreatment.Bandage)
            {
                if (applied == EmsTreatment.BurnDressing || applied == EmsTreatment.WoundPacking || applied == EmsTreatment.WetDressing) return true;
            }

            if (required == EmsTreatment.Tourniquet && applied == EmsTreatment.JunctionalTourniquet) return true;

            return false;
        }

        private bool IsLocalizedTreatment(EmsTreatment t)
        {
            return t == EmsTreatment.Bandage || t == EmsTreatment.Tourniquet || t == EmsTreatment.JunctionalTourniquet ||
                   t == EmsTreatment.WoundPacking || t == EmsTreatment.Splint || t == EmsTreatment.TractionSplint ||
                   t == EmsTreatment.PelvicBinder || t == EmsTreatment.CervicalCollar || t == EmsTreatment.ChestSeal ||
                   t == EmsTreatment.NeedleDecomp || t == EmsTreatment.BurnDressing || t == EmsTreatment.IcePack ||
                   t == EmsTreatment.Irrigation || t == EmsTreatment.WetDressing || t == EmsTreatment.EyePatch ||
                   t == EmsTreatment.EyeShield || t == EmsTreatment.Suture || t == EmsTreatment.DirectPressure ||
                   t == EmsTreatment.StabiliseObject;
        }

        private void RevivePatient()
        {
            Consciousness = ConsciousnessLevel.Alert;
            GameFiber.StartNew(delegate
            {
                Character.Health = 100;
                Character.BlockPermanentEvents = true;
                Character.IsPositionFrozen = false;
                Character.CanRagdoll = true;

                string animDict = EntryPoint.AnimationConfig.PatientReviveDict.Value;
                string animName = EntryPoint.AnimationConfig.PatientReviveName.Value;

                if (IsOnStretcher && StretcherManager.Prop != null && StretcherManager.Prop.Exists())
                {
                    Character.IsCollisionEnabled = false;
                    animDict = EntryPoint.AnimationConfig.PatientStretcherDict.Value;
                    animName = EntryPoint.AnimationConfig.PatientStretcherName.Value;
                }
                else
                {
                    Character.Tasks.Clear();
                }

                NativeFunction.Natives.REQUEST_ANIM_DICT(animDict);
                while (!NativeFunction.Natives.HAS_ANIM_DICT_LOADED<bool>(animDict)) GameFiber.Yield();
                Character.Tasks.PlayAnimation(animDict, animName, 8.0f, AnimationFlags.Loop);

                Game.DisplayNotification(Localization.Get("NOTIF_PATIENT_REGAINED_CONSCIOUSNESS"));
            });
        }

        public void ApplyVisuals() => PatientVisuals.ApplyBloodEffects(this);
        public string GetSamplersString() => History.GetLimitedHistoryString();
    }
}