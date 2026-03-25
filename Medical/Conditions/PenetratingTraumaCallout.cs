using EmsPlus.Core;
using EmsPlus.Framework;
using EmsPlus.Medical.Conditions;
using EmsPlus.Medical.Frameworks;
using Rage;

namespace EmsPlus.Callouts
{
    public class PenetratingTraumaCallout : EmsCallout
    {
        private Ped patient;
        private Blip blip;

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

            p.DispatchDiagnosis = "Gunshot Wound(s)";
            p.Consciousness = ConsciousnessLevel.Pain;

            p.Conditions.Add(InjuryFactory.Haemorrhage.Arterial(PedBoneId.LeftUpperArm));
            p.Conditions.Add(InjuryFactory.Chest.SuckingChestWound());

            // They are bleeding heavily, blood volume starts dropping immediately
            p.BloodVolume = 70f;

            patient.Tasks.PlayAnimation("misslamar1dead_body", "dead_idle", 8.0f, AnimationFlags.Loop);
            p.ApplyVisuals();

            blip = new Blip(patient) { Sprite = (BlipSprite)280, Color = System.Drawing.Color.Red };
            return true;
        }
        public override void End() { base.End(); if (blip.Exists()) blip.Delete(); if (patient.Exists()) patient.Dismiss(); }
    }
}