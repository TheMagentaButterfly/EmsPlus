using EmsPlus.Managers;
using EmsPlus.Medical.Conditions;
using EmsPlus.Medical.Frameworks;
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
        public PatientHistory History { get; set; } = new PatientHistory();

        public string DispatchDiagnosis { get; set; } = "Unknown Medical Emergency";
        public ConsciousnessLevel Consciousness { get; set; } = ConsciousnessLevel.Unresponsive;

        public VitalState HeartRate { get; set; } = VitalState.Normal;
        public VitalState BloodPressure { get; set; } = VitalState.Normal;
        public VitalState SpO2 { get; set; } = VitalState.Normal;
        public VitalState Respiration { get; set; } = VitalState.Normal;
        public VitalState BloodGlucose { get; set; } = VitalState.Normal;

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
            if (IsStabilized) return;

            foreach (var condition in Conditions.Where(c => !c.IsTreated))
            {
                condition.Update(this);
            }
        }

        public void ApplyTreatment(EmsTreatment treatment, PedBoneId? targetBone = null)
        {
            bool treatmentWasEffective = false;
            bool isLocalized = AnatomicalRegistry.IsLocalizedTreatment(treatment);

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
                //Game.DisplayNotification($"~g~Treatment Successful:~w~ Applied {treatment}");
                if (IsStabilized)
                {
                    if (Consciousness != ConsciousnessLevel.Alert) RevivePatient();
                }
            }
            else
            {
                //Game.DisplayNotification($"~y~Treatment Ineffective:~w~ {treatment} had no positive effect.");
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

                if (IsOnStretcher && Managers.StretcherManager.Prop != null && Managers.StretcherManager.Prop.Exists())
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