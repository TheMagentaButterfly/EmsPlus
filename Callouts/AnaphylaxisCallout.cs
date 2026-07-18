using EmsPlus.Core;
using EmsPlus.Managers;
using EmsPlus.Medical;
using Rage;
using System.Drawing;

namespace EmsPlus.Callouts
{
    public class AnaphylaxisCallout : EmsCallout
    {
        private Ped patient;
        private Blip blip;
        private bool hasArrivedAtScene = false;

        public AnaphylaxisCallout()
        {
            AddExclusionZone(ExclusionZoneType.Highways);
            AddExclusionZone(ExclusionZoneType.Hospitals);
        }

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

            p.Dialogue.Add(new DialogueLine("Patient", "*Gasp*... Can't... breathe... my throat..."));
            p.Dialogue.Add(new DialogueLine("Patient", "It feels like it's swelling shut... please... help..."));

            p.DispatchDiagnosis = "Anaphylactic Shock";
            p.Consciousness = ConsciousnessLevel.Verbal;
            p.SpO2 = VitalState.Low;
            p.HeartRate = VitalState.Elevated;

            SpawnEmergencyUnit("firetruk", "s_m_y_fireman_01", CalloutPosition);

            Ped bystanderPed = SpawnBystander("a_f_y_tourist_01", CalloutPosition);
            if (bystanderPed != null)
            {
                GameState.CurrentBystander = new Bystander(bystanderPed);
                GameState.CurrentBystander.Dialogue.Add(new DialogueLine("Witness", "Please help! They were stung by a bee and now they can't breathe!"));
                GameState.CurrentBystander.Dialogue.Add(new DialogueLine("Paramedic", "I'm on it. I need you to stay back and keep the area clear."));
            }

            p.DispatchDiagnosis = "Anaphylactic Shock";
            p.Consciousness = ConsciousnessLevel.Verbal;

            p.SpO2 = VitalState.Low;
            p.HeartRate = VitalState.Elevated;

            p.Conditions.Add(new SystemicCondition("Anaphylaxis", EmsTreatment.Adrenaline, EmsTreatment.HighFlowOxygen));

            patient.Tasks.PlayAnimation("rcmfanatic1out_of_breath", "p_zero_tired_01e", 8.0f, AnimationFlags.Loop);

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