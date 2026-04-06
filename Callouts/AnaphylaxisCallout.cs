using EmsPlus.Core;
using EmsPlus.Managers;
using EmsPlus.Medical;
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
            patient.IsPersistent = true;
            patient.BlockPermanentEvents = true;
            GameState.CurrentPatient = new Patient(patient);
            var p = GameState.CurrentPatient;

            // 3. Define the PATIENT's dialogue
            p.Dialogue.Add(new DialogueLine("Patient", "*Gasp*... Can't... breathe... my throat..."));
            p.Dialogue.Add(new DialogueLine("Patient", "It feels like it's swelling shut... please... help..."));

            p.DispatchDiagnosis = "Anaphylactic Shock";
            p.Consciousness = ConsciousnessLevel.Verbal;
            p.SpO2 = VitalState.Low;
            p.HeartRate = VitalState.Elevated;

            SpawnEmergencyUnit("firetruk", "s_m_y_fireman_01", CalloutPosition);

            // 1. Create the bystander
            Ped bystanderPed = SpawnBystander("a_f_y_tourist_01", CalloutPosition);
            if (bystanderPed != null)
            {
                GameState.CurrentBystander = new Bystander(bystanderPed);
                // 2. Define their dialogue
                GameState.CurrentBystander.Dialogue.Add(new DialogueLine("Witness", "Oh thank god, you're here! He just got stung by a bee!"));
                GameState.CurrentBystander.Dialogue.Add(new DialogueLine("Witness", "He's allergic... he said his throat was closing up and then he just... he can't breathe!"));
                GameState.CurrentBystander.Dialogue.Add(new DialogueLine("Paramedic", "Okay, I've got it from here. Stand back and give us some space."));
            }

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