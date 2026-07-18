using EmsPlus.Core;
using EmsPlus.Managers;
using EmsPlus.Medical;
using Rage;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace EmsPlus.Callouts
{
    public class ProceduralTraumaCallout : EmsCallout
    {
        private Ped patient;
        private Vector3 spawnPos;
        private Blip blip;
        private bool hasArrivedAtScene = false;
        private Random rnd = new Random();

        private static readonly PedBoneId[] SpawnableBones =
        {
            InjuryBones.LeftThigh,    InjuryBones.RightThigh,
            InjuryBones.LeftCalf,     InjuryBones.RightCalf,
            InjuryBones.LeftUpperArm, InjuryBones.RightUpperArm,
            InjuryBones.LeftForearm,  InjuryBones.RightForearm,
            InjuryBones.LeftHand,     InjuryBones.RightHand,
            InjuryBones.LeftFoot,     InjuryBones.RightFoot,
            InjuryBones.Head,
            InjuryBones.Chest,
            InjuryBones.Abdomen,
            InjuryBones.Pelvis,
            InjuryBones.LumbarSpine,
            InjuryBones.ThoracicSpine,
        };

        private enum TraumaTheme { Blunt, Penetrating, Blast }


        public override bool OnBeforeCalloutDisplayed()
        {
            CalloutName = "Major Trauma";
            CalloutMessage = "High-velocity accident reported. Multiple traumatic injuries suspected.";

            Vector3 centerPoint = StationManager.ActiveStation?.Position
                                  ?? Game.LocalPlayer.Character.Position;

            spawnPos = World.GetNextPositionOnStreet(centerPoint.Around(150f, 1000f));
            if (spawnPos == Vector3.Zero) return false;

            CalloutPosition = spawnPos;
            ShowCalloutAreaBlipBeforeAccepting(spawnPos, 30f);

            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            base.OnCalloutAccepted();

            Vector3 sidewalk = GetSidewalkPosition(spawnPos);
            patient = new Ped(sidewalk);
            patient.IsPersistent = true;
            patient.BlockPermanentEvents = true;

            GameState.CurrentPatient = new Patient(patient);
            Patient p = GameState.CurrentPatient;

            GenerateRandomTrauma(p);
            CalculateVitalsFromInjuries(p);

            p.ApplyVisuals();
            patient.Tasks.PlayAnimation("misslamar1dead_body", "dead_idle", 8.0f, AnimationFlags.Loop);

            blip = new Blip(patient);
            blip.Color = Color.Red;
            blip.Name = "Medical Emergency";
            blip.IsRouteEnabled = true;

            return true;
        }

        public override void Process()
        {
            base.Process();

            if (!hasArrivedAtScene && Game.LocalPlayer.Character.DistanceTo(patient) < 25f)
            {
                hasArrivedAtScene = true;

                if (blip.Exists()) blip.Delete();

                blip = patient.AttachBlip();
                blip.Sprite = (BlipSprite)280;
                blip.Color = Color.Yellow;
                blip.Name = "Patient";
                blip.IsRouteEnabled = false;
            }

            if (hasArrivedAtScene && blip.Exists()
                && GameState.CurrentPatient != null
                && GameState.CurrentPatient.IsOnStretcher)
            {
                blip.Delete();
            }
        }

        public override void End()
        {
            base.End();
            if (blip.Exists()) blip.Delete();
            if (patient.Exists()) patient.Dismiss();
        }

        private void GenerateRandomTrauma(Patient p)
        {
            TraumaTheme theme = (TraumaTheme)rnd.Next(0, 3);

            switch (theme)
            {
                case TraumaTheme.Blunt: p.DispatchDiagnosis = "High Fall / MVC"; break;
                case TraumaTheme.Penetrating: p.DispatchDiagnosis = "Assault / Penetrating"; break;
                case TraumaTheme.Blast: p.DispatchDiagnosis = "Blast / Industrial"; break;
            }

            int injuryCount = rnd.Next(3, 6);
            HashSet<PedBoneId> usedBones = new HashSet<PedBoneId>();
            int failSafe = 0;

            for (int i = 0; i < injuryCount; i++)
            {
                if (++failSafe > 50) break;

                PedBoneId bone = PickUnusedBone(usedBones);
                PhysicalInjury injury = null;

                switch (theme)
                {
                    case TraumaTheme.Blunt: injury = RollBluntInjury(p, bone); break;
                    case TraumaTheme.Penetrating: injury = RollPenetratingInjury(p, bone); break;
                    case TraumaTheme.Blast: injury = RollBlastInjury(p, bone); break;
                }

                if (injury == null) { i--; continue; }
                if (usedBones.Contains(injury.Bone)) { i--; continue; }

                usedBones.Add(injury.Bone);
                p.Conditions.Add(injury);
            }
        }

        // ── Blunt trauma (MVC / fall) ─────────────────────────────────────────
        private PhysicalInjury RollBluntInjury(Patient p, PedBoneId bone)
        {
            int roll = Roll();

            if (bone == InjuryBones.LumbarSpine || bone == InjuryBones.ThoracicSpine)
                return roll < 50
                    ? InjuryFactory.Fracture.Compound(bone)
                    : InjuryFactory.SoftTissue.CrushInjury(bone);

            if (bone == InjuryBones.Pelvis)
                return roll < 60
                    ? InjuryFactory.Fracture.Pelvic()
                    : InjuryFactory.Fracture.PelvicCrush();

            if ((bone == InjuryBones.LeftThigh || bone == InjuryBones.RightThigh) && roll < 40)
                return InjuryFactory.Fracture.Femoral();

            if (bone == InjuryBones.Chest)
            {
                bool chestFree = !HasCondition(p, "Tension Pneumothorax");
                if (roll < 25 && chestFree) return InjuryFactory.Chest.TensionPneumothorax();
                if (roll < 50) return InjuryFactory.Chest.PulmonaryContusion();
                if (roll < 70) return InjuryFactory.Fracture.FlailChest();
                return InjuryFactory.Fracture.Rib();
            }

            if (bone == InjuryBones.Abdomen)
                return roll < 50
                    ? InjuryFactory.Haemorrhage.Internal()
                    : InjuryFactory.SoftTissue.PenetratingAbdominal();

            if (bone == InjuryBones.Head)
            {
                if (roll < 30) return InjuryFactory.HeadAndNeuro.EpiduralHaematoma();
                if (roll < 55) return InjuryFactory.HeadAndNeuro.SubduralHaematoma();
                if (roll < 75) return InjuryFactory.Fracture.Skull();
                return InjuryFactory.HeadAndNeuro.Concussion();
            }

            if (roll < 35) return InjuryFactory.Fracture.Compound(bone);
            if (roll < 65) return InjuryFactory.SoftTissue.Laceration(bone);
            if (roll < 80) return InjuryFactory.SoftTissue.CrushInjury(bone);
            return InjuryFactory.SoftTissue.DeepLaceration(bone);
        }

        private PhysicalInjury RollPenetratingInjury(Patient p, PedBoneId bone)
        {
            int roll = Roll();

            if (bone == InjuryBones.Chest)
            {
                if (roll < 35) return InjuryFactory.Gunshot.Chest();
                if (roll < 65) return InjuryFactory.StabAndPuncture.StabChest();
                if (roll < 80) return InjuryFactory.Chest.SuckingChestWound();
                return InjuryFactory.Chest.Haemothorax();
            }

            if (bone == InjuryBones.Abdomen)
            {
                if (roll < 45) return InjuryFactory.Gunshot.Abdomen();
                if (roll < 75) return InjuryFactory.StabAndPuncture.StabAbdomen();
                return InjuryFactory.Haemorrhage.Internal();
            }

            if (bone == InjuryBones.Head)
            {
                if (roll < 50) return InjuryFactory.Gunshot.Head();
                return InjuryFactory.HeadAndNeuro.IntracranialHaemorrhage();
            }

            if (bone == InjuryBones.Neck)
            {
                if (roll < 50) return InjuryFactory.Gunshot.Neck();
                return InjuryFactory.StabAndPuncture.PenetratingNeck();
            }

            if (bone == InjuryBones.Pelvis)
                return InjuryFactory.Gunshot.Pelvis();

            bool isLimb = bone == InjuryBones.LeftThigh || bone == InjuryBones.RightThigh ||
                          bone == InjuryBones.LeftCalf || bone == InjuryBones.RightCalf ||
                          bone == InjuryBones.LeftUpperArm || bone == InjuryBones.RightUpperArm ||
                          bone == InjuryBones.LeftForearm || bone == InjuryBones.RightForearm;

            if (isLimb)
            {
                if (roll < 35) return InjuryFactory.Haemorrhage.Arterial(bone);
                if (roll < 65) return InjuryFactory.Gunshot.ThroughAndThrough(bone);
                if (roll < 80) return InjuryFactory.Gunshot.Wound(bone);
                return InjuryFactory.StabAndPuncture.StabWound(bone);
            }

            return roll < 50
                ? InjuryFactory.Gunshot.Wound(bone)
                : InjuryFactory.SoftTissue.DeepLaceration(bone);
        }

        private PhysicalInjury RollBlastInjury(Patient p, PedBoneId bone)
        {
            int roll = Roll();

            if (bone == InjuryBones.Chest)
            {
                if (roll < 30) return InjuryFactory.Blast.BlastInjury();
                if (roll < 55) return InjuryFactory.Chest.HaemoPneumothorax();
                if (roll < 75) return InjuryFactory.Fracture.FlailChest();
                return InjuryFactory.Chest.PulmonaryContusion();
            }

            if (bone == InjuryBones.Head)
            {
                if (roll < 40) return InjuryFactory.Burns.Inhalation();
                if (roll < 65) return InjuryFactory.Blast.EardrumRupture();
                return InjuryFactory.HeadAndNeuro.Concussion();
            }

            if (bone == InjuryBones.Abdomen)
                return InjuryFactory.Haemorrhage.Internal();

            if (roll < 25) return InjuryFactory.Burns.FourthDegree(bone);
            if (roll < 50) return InjuryFactory.Burns.ThirdDegree(bone);
            if (roll < 70) return InjuryFactory.Burns.SecondDegree(bone);
            if (roll < 85) return InjuryFactory.Blast.ShrapnelWounds(bone);
            return InjuryFactory.SoftTissue.Laceration(bone);
        }

        private static void CalculateVitalsFromInjuries(Patient p)
        {
            List<PhysicalInjury> injuries = p.Conditions.OfType<PhysicalInjury>().ToList();
            float totalBleed = injuries.Sum(i => i.BleedSeverity);

            bool hasChestTrauma = injuries.Any(i =>
                i.Bone == InjuryBones.Chest &&
                (i.Name.Contains("Pneumo") || i.Name.Contains("Chest") ||
                 i.Name.Contains("Haemo") || i.Name.Contains("Flail") ||
                 i.Name.Contains("Contusion")));

            bool hasTBI = injuries.Any(i =>
                i.Bone == InjuryBones.Head &&
                (i.Name.Contains("Haematoma") || i.Name.Contains("Haemorrhage")));

            if (totalBleed > 4.0f) p.HeartRate = VitalState.CriticalHigh;
            else if (totalBleed > 2.5f) p.HeartRate = VitalState.Elevated;
            else if (totalBleed > 1.0f) p.HeartRate = VitalState.Elevated;
            else p.HeartRate = VitalState.Normal;

            if (totalBleed > 4.5f) p.BloodPressure = VitalState.CriticalLow;
            else if (totalBleed > 3.0f) p.BloodPressure = VitalState.Low;
            else if (totalBleed > 1.5f) p.BloodPressure = VitalState.Low;
            else p.BloodPressure = VitalState.Normal;

            if (hasChestTrauma)
                p.SpO2 = totalBleed > 2.0f ? VitalState.CriticalLow : VitalState.Low;
            else
                p.SpO2 = VitalState.Normal;

            if (totalBleed > 4.0f) p.Consciousness = ConsciousnessLevel.Unresponsive;
            else if (totalBleed > 2.5f) p.Consciousness = ConsciousnessLevel.Pain;
            else if (hasTBI) p.Consciousness = ConsciousnessLevel.Pain;
            else p.Consciousness = ConsciousnessLevel.Verbal;
        }

        private PedBoneId PickUnusedBone(HashSet<PedBoneId> used)
        {
            List<PedBoneId> available = new List<PedBoneId>();
            foreach (PedBoneId b in SpawnableBones)
                if (!used.Contains(b)) available.Add(b);

            return available.Count > 0
                ? available[rnd.Next(available.Count)]
                : InjuryBones.Abdomen;
        }

        private static bool HasCondition(Patient p, string name) =>
            p.Conditions.Any(c => c.Name == name);

        private int Roll() => rnd.Next(0, 100);
    }
}