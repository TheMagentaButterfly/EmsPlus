using EmsPlus.Core;
using EmsPlus.Framework;
using EmsPlus.Medical.Conditions;
using EmsPlus.Medical.Frameworks;
using Rage;

namespace EmsPlus.Callouts
{
    public class CardiacArrestCallout : EmsCallout
    {
        private Ped patient;
        private Blip blip;

        public override bool OnBeforeCalloutDisplayed()
        {
            CalloutName = "Cardiac Arrest";
            CalloutMessage = "Subject collapsed, CPR in progress by bystanders.";
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

            p.DispatchDiagnosis = "Cardiac Arrest (V-Fib)";
            p.Consciousness = ConsciousnessLevel.Unresponsive;

            p.IsCardiacArrest = true;
            p.BrainOxygen = 40f;

            p.Conditions.Add(new SystemicCondition("Ventricular Fibrillation", EmsTreatment.Defibrillation, EmsTreatment.Adrenaline));

            patient.Tasks.PlayAnimation("misslamar1dead_body", "dead_idle", 8.0f, AnimationFlags.Loop);

            blip = new Blip(patient) { Sprite = (BlipSprite)280, Color = System.Drawing.Color.Red };
            return true;
        }

        public override void End() { base.End(); if (blip.Exists()) blip.Delete(); if (patient.Exists()) patient.Dismiss(); }
    }
}