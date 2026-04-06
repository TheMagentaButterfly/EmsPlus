using EmsPlus.Core;
using EmsPlus.Managers;
using EmsPlus.Medical;
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

            SpawnEmergencyUnit("firetruk", "s_m_y_fireman_01", CalloutPosition);
            Ped bystanderPed = SpawnBystander("a_f_y_tourist_01", CalloutPosition);
            if (bystanderPed != null)
            {
                GameState.CurrentBystander = new Bystander(bystanderPed);
                GameState.CurrentBystander.Dialogue.Add(new DialogueLine("Witness", "They just collapsed while we were waiting for the bus!"));
                GameState.CurrentBystander.Dialogue.Add(new DialogueLine("Witness", "I don't think they're breathing, I started chest compressions immediately."));
            }

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