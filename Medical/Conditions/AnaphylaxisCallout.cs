using EmsPlus.Core;
using EmsPlus.Framework;
using EmsPlus.Medical.Conditions;
using EmsPlus.Medical.Frameworks;
using Rage;

namespace EmsPlus.Callouts
{
    public class AnaphylaxisCallout : EmsCallout
    {
        private Ped patient;
        private Blip blip;

        public override bool OnBeforeCalloutDisplayed()
        {
            CalloutName = "Allergic Reaction";
            CalloutMessage = "Severe allergic reaction to bee sting. Airway swelling reported.";
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

            p.DispatchDiagnosis = "Anaphylactic Shock";
            p.Consciousness = ConsciousnessLevel.Verbal;

            p.SpO2 = VitalState.Low; // Airway is closing
            p.HeartRate = VitalState.Elevated;

            p.Conditions.Add(new SystemicCondition("Anaphylaxis", EmsTreatment.Adrenaline, EmsTreatment.HighFlowOxygen));

            patient.Tasks.PlayAnimation("rcmfanatic1out_of_breath", "p_zero_tired_01e", 8.0f, AnimationFlags.Loop);

            blip = new Blip(patient) { Sprite = (BlipSprite)280, Color = System.Drawing.Color.Orange };
            return true;
        }
        public override void End() { base.End(); if (blip.Exists()) blip.Delete(); if (patient.Exists()) patient.Dismiss(); }
    }
}