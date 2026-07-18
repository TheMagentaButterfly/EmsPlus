using EmsPlus.Core;
using EmsPlus.Managers;
using EmsPlus.Medical;
using Rage;
using System.Drawing;

namespace EmsPlus.Callouts
{
    public class PenetratingTraumaCallout : EmsCallout
    {
        private Ped patient;
        private Blip blip;
        private bool hasArrivedAtScene = false;

        public PenetratingTraumaCallout()
        {
            AddExclusionZone(ExclusionZoneType.Hospitals);
        }

        public override bool OnBeforeCalloutDisplayed()
        {
            CalloutName = "Shooting";
            CalloutMessage = "Shots fired, one victim down. Scene is secure.";
            CalloutPosition = World.GetNextPositionOnStreet(Game.LocalPlayer.Character.Position.Around(300f, 600f));
            if (CalloutPosition == Vector3.Zero) return false;
            ShowCalloutAreaBlipBeforeAccepting(CalloutPosition, 30f);
            return true;
        }

        public override bool OnCalloutAccepted()
        {
            base.OnCalloutAccepted();
            patient = new Ped(GetSidewalkPosition(CalloutPosition));
            patient.IsPersistent = true; patient.BlockPermanentEvents = true;

            GameState.CurrentPatient = new Patient(patient);
            var p = GameState.CurrentPatient;

            SpawnEmergencyUnit("police", "s_m_y_cop_02", CalloutPosition);
            Ped cop = SpawnBystander("s_m_y_cop_01", CalloutPosition);
            if (cop != null)
            {
                GameState.CurrentBystander = new Bystander(cop);
                GameState.CurrentBystander.Dialogue.Add(new DialogueLine("Officer", "We've got one down. Shooter fled on foot heading East."));
                GameState.CurrentBystander.Dialogue.Add(new DialogueLine("Officer", "They're bleeding out fast, they took at least two hits to the torso."));
            }

            p.DispatchDiagnosis = "Gunshot Wound(s)";
            p.Consciousness = ConsciousnessLevel.Pain;

            p.Conditions.Add(InjuryFactory.Haemorrhage.Arterial(PedBoneId.LeftUpperArm));
            p.Conditions.Add(InjuryFactory.Chest.SuckingChestWound());

            p.BloodVolume = 70f;

            patient.Tasks.PlayAnimation("misslamar1dead_body", "dead_idle", 8.0f, AnimationFlags.Loop);
            p.ApplyVisuals();

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

        public override void End() { base.End(); if (blip.Exists()) blip.Delete(); if (patient.Exists()) patient.Dismiss(); }
    }
}