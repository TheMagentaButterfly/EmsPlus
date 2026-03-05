using EmsPlus.Core;
using EmsPlus.Framework;
using EmsPlus.Managers;
using EmsPlus.Medical.Conditions;
using EmsPlus.Medical.Frameworks;
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
            var p = GameState.CurrentPatient;

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

        private void GenerateRandomTrauma(Patient p)
        {
            int theme = rnd.Next(0, 3);
            p.DispatchDiagnosis = theme == 0 ? "High Fall / MVC" : (theme == 1 ? "Assault / Penetrating" : "Blast / Industrial");

            int injuryCount = rnd.Next(3, 6);
            var usedBones = new List<PedBoneId>();

            int failSafe = 0;

            for (int i = 0; i < injuryCount; i++)
            {
                failSafe++;
                if (failSafe > 50) break;

                PedBoneId bone = GetRandomBone(exclude: usedBones);
                PhysicalInjury injury = null;
                int roll = rnd.Next(0, 100);

                if (theme == 0) // BLUNT TRAUMA
                {
                    if (roll < 30) injury = InjuryFactory.CompoundFracture(bone);
                    else if (roll < 60) injury = InjuryFactory.Laceration(bone);
                    else if (roll < 80 && !p.Conditions.Any(c => c.Name == "Tension Pneumothorax"))
                        injury = InjuryFactory.TensionPneumothorax();
                    else injury = InjuryFactory.Laceration(bone);
                }
                else if (theme == 1) // PENETRATING TRAUMA
                {
                    if (roll < 40 && bone != PedBoneId.Head)
                    {
                        injury = InjuryFactory.ArterialBleed(bone);
                    }
                    else if (roll < 70) injury = InjuryFactory.GunshotWound(bone);
                    else injury = InjuryFactory.Laceration(bone);
                }
                else // BLAST/BURN
                {
                    if (roll < 40) injury = InjuryFactory.ThirdDegreeBurn(bone);
                    else injury = InjuryFactory.Laceration(bone);
                }

                if (injury == null)
                {
                    i--; continue;
                }

                bool boneAlreadyInjured = p.Conditions.OfType<PhysicalInjury>().Any(c => c.Bone == injury.Bone);

                if (boneAlreadyInjured)
                {
                    i--;
                    continue;
                }

                usedBones.Add(injury.Bone);
                p.Conditions.Add(injury);
            }
        }

        private void CalculateVitalsFromInjuries(Patient p)
        {
            float totalBleed = p.Conditions.OfType<PhysicalInjury>().Sum(i => i.BleedSeverity);
            bool hasChestTrauma = p.Conditions.Any(c => c.Name.Contains("Chest") || c.Name.Contains("Pneumo"));

            // Heart Rate
            if (totalBleed > 3.0f) p.HeartRate = VitalState.CriticalHigh; // Compensating for shock
            else if (totalBleed > 1.0f) p.HeartRate = VitalState.Elevated;

            // Blood Pressure
            if (totalBleed > 4.0f) p.BloodPressure = VitalState.CriticalLow; // Decompensating shock
            else if (totalBleed > 2.0f) p.BloodPressure = VitalState.Low;

            // SpO2
            if (hasChestTrauma) p.SpO2 = VitalState.Low;

            // Consciousness
            if (totalBleed > 3.5f || hasChestTrauma) p.Consciousness = ConsciousnessLevel.Unresponsive;
            else p.Consciousness = ConsciousnessLevel.Pain;
        }

        private PedBoneId GetRandomBone(List<PedBoneId> exclude)
        {
            var bones = new List<PedBoneId> {
                PedBoneId.LeftThigh, PedBoneId.RightThigh,
                PedBoneId.LeftCalf, PedBoneId.RightCalf,
                PedBoneId.LeftUpperArm, PedBoneId.RightUpperArm,
                PedBoneId.LeftForeArm, PedBoneId.RightForearm,
                PedBoneId.Head,
                PedBoneId.Spine3,
                PedBoneId.Spine
            };

            var available = bones.Where(b => !exclude.Contains(b)).ToList();
            if (available.Count == 0) return PedBoneId.Pelvis;

            return available[rnd.Next(available.Count)];
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

            if (hasArrivedAtScene && blip.Exists() && GameState.CurrentPatient != null)
            {
                if (GameState.CurrentPatient.IsOnStretcher)
                {
                    blip.Delete();
                }
            }
        }

        public override void End()
        {
            base.End();
            if (blip.Exists()) blip.Delete();
            if (patient.Exists()) patient.Dismiss();
        }
    }
}