using EmsPlus.Core;
using EmsPlus.Managers;
using EmsPlus.Medical;
using Rage;
using System.Drawing;

namespace EmsPlus.Callouts
{
    public class PaletoRescueCallout : EmsCallout
    {
        private Ped patient;
        private Vector3 spawnPos;
        private Blip blip;
        private bool hasArrivedAtScene = false;

        public PaletoRescueCallout()
        {
            AllowedStationIDs.Add("PALETO");
        }

        public override bool OnBeforeCalloutDisplayed()
        {
            CalloutName = "Mountain Rescue";
            CalloutMessage = "Hiker fell from a trail near Mount Chiliad.";

            Vector3 stationPos = StationManager.ActiveStation.Position;
            spawnPos = World.GetNextPositionOnStreet(stationPos.Around(400f, 1200f));

            CalloutPosition = spawnPos;
            ShowCalloutAreaBlipBeforeAccepting(spawnPos, 50f);
            return true;
        }

        public override bool OnCalloutAccepted()
        {
            base.OnCalloutAccepted();

            patient = new Ped(spawnPos);
            patient.IsPersistent = true;
            patient.IsPositionFrozen = true;
            patient.BlockPermanentEvents = true;

            GameState.CurrentPatient = new Patient(patient);
            var p = GameState.CurrentPatient;

            p.Dialogue.Add(new DialogueLine("Patient", "I shouldn't have gone off the trail..."));
            p.Dialogue.Add(new DialogueLine("Patient", "My leg... it looks... it looks wrong. Please make the pain stop."));

            p.DispatchDiagnosis = "Major Trauma (MVA)";
            p.Consciousness = ConsciousnessLevel.Pain;

            p.Conditions.Add(new PhysicalInjury("Compound Fracture", PedBoneId.LeftCalf, 3.0f, EmsTreatment.Splint));
            p.Conditions.Add(new PhysicalInjury("Head Trauma", PedBoneId.Head, 0.5f, EmsTreatment.CervicalCollar));

            p.ApplyVisuals();
            patient.Tasks.PlayAnimation("misschinese2_crystalmaze", "2int_loop_a_taocheng", 8.0f, AnimationFlags.Loop);

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

            if (patient.Exists() && !GameState.IsPlayerBusy)
            {
                patient.Dismiss();
            }
        }
    }
}