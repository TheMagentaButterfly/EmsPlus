using EmsPlus.Configuration;
using EmsPlus.Core;
using EmsPlus.Framework;
using EmsPlus.Managers;
using EmsPlus.Medical.Conditions;
using EmsPlus.Medical.Frameworks;
using Rage;
using System;
using System.Drawing;

namespace EmsPlus.Callouts
{
    public class HomeEmergencyCallout : EmsCallout
    {
        private Ped patient;
        private Blip blip;
        private bool hasArrivedAtScene = false;
        private bool hasSpawnedPatient = false;
        private Random rnd = new Random();

        public InteriorDefinition TargetInterior { get; set; }
        public EntranceDefinition TargetEntrance { get; set; }

        public override bool OnBeforeCalloutDisplayed()
        {
            CalloutName = "Medical Emergency at Residence";
            CalloutMessage = "Caller reported a person unresponsive inside their apartment.";

            if (EntryPoint.InteriorConfig.Definitions.Count == 0)
            {
                Game.Console.Print("[EmsPlus] Aborting HomeEmergencyCallout: No interiors loaded in XML!");
                return false;
            }

            TargetInterior = EntryPoint.InteriorConfig.Definitions[rnd.Next(EntryPoint.InteriorConfig.Definitions.Count)];
            if (TargetInterior.Entrances.Count == 0) return false;

            TargetEntrance = TargetInterior.Entrances[rnd.Next(TargetInterior.Entrances.Count)];

            CalloutPosition = TargetEntrance.Coords;
            ShowCalloutAreaBlipBeforeAccepting(CalloutPosition, 30f);

            return true;
        }

        public override bool OnCalloutAccepted()
        {
            base.OnCalloutAccepted();

            // Unlock the specific dispatched door
            InteriorManager.EnableTargetEntrance(TargetInterior, TargetEntrance);

            blip = new Blip(CalloutPosition);
            blip.Color = Color.Yellow;
            blip.Name = "Medical Emergency";
            blip.IsRouteEnabled = true;

            return true;
        }

        public override void Process()
        {
            base.Process();

            // 1. Arrive at building
            if (!hasArrivedAtScene && Game.LocalPlayer.Character.DistanceTo(TargetEntrance.Coords) < 15f)
            {
                hasArrivedAtScene = true;
                Game.DisplayNotification($"~b~Dispatch:~w~ Arrived at location. Proceed inside {TargetEntrance.Name}.");
            }

            // 2. Spawn Patient when player enters the correct building
            if (InteriorManager.CurrentInterior != null && !hasSpawnedPatient)
            {
                hasSpawnedPatient = true;

                Vector3 spawnLoc = TargetInterior.PatientSpawnPoints.Count > 0
                    ? TargetInterior.PatientSpawnPoints[rnd.Next(TargetInterior.PatientSpawnPoints.Count)]
                    : TargetInterior.ExitCoords;

                patient = new Ped(spawnLoc);
                patient.IsPersistent = true;
                patient.BlockPermanentEvents = true;
                patient.IsPositionFrozen = true;

                GameState.CurrentPatient = new Patient(patient);
                var p = GameState.CurrentPatient;

                p.DispatchDiagnosis = "Cardiac / Respiratory Arrest";
                p.Conditions.Add(new SystemicCondition("Drug Overdose", EmsTreatment.Naloxone, EmsTreatment.AirwayManagement));
                p.HeartRate = VitalState.CriticalLow;
                p.SpO2 = VitalState.CriticalLow;
                p.Consciousness = ConsciousnessLevel.Unresponsive;

                p.ApplyVisuals();
                patient.Tasks.PlayAnimation("misslamar1dead_body", "dead_idle", 8.0f, AnimationFlags.Loop);

                if (blip.Exists()) blip.Delete();

                blip = patient.AttachBlip();
                blip.Sprite = (BlipSprite)280;
                blip.Color = Color.Yellow;
                blip.Name = "Patient";
                blip.IsRouteEnabled = false;
            }

            // 3. Clear Blip when loaded
            if (hasSpawnedPatient && blip.Exists() && GameState.CurrentPatient != null)
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
            if (blip != null && blip.Exists()) blip.Delete();
            if (patient != null && patient.Exists()) patient.Dismiss();
        }
    }
}