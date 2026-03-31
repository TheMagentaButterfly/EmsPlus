using EmsPlus.Core;
using EmsPlus.Medical;
using Rage;

namespace EmsPlus.Callouts
{
    public class FallFromHeightCallout : EmsCallout
    {
        private Ped patient;
        private Blip blip;

        public override bool OnBeforeCalloutDisplayed()
        {
            CalloutName = "Fall From Height";
            CalloutMessage = "Construction worker fell from scaffolding. Traumatic injuries.";
            CalloutPosition = World.GetNextPositionOnStreet(Game.LocalPlayer.Character.Position.Around(300f, 600f));
            if (CalloutPosition == Vector3.Zero) return false;
            ShowCalloutAreaBlipBeforeAccepting(CalloutPosition, 30f);
            return true;
        }

        public override bool OnCalloutAccepted()
        {
            base.OnCalloutAccepted();
            patient = new Ped("s_m_y_construct_01", GetSidewalkPosition(CalloutPosition), 0f);
            patient.IsPersistent = true; patient.BlockPermanentEvents = true;

            GameState.CurrentPatient = new Patient(patient);
            var p = GameState.CurrentPatient;

            p.DispatchDiagnosis = "Traumatic Fall";
            p.Consciousness = ConsciousnessLevel.Pain;

            p.Conditions.Add(InjuryFactory.Fracture.Compound(PedBoneId.RightThigh));
            p.Conditions.Add(new PhysicalInjury("Suspected Spinal Trauma", PedBoneId.Neck, 0f, EmsTreatment.CervicalCollar));

            patient.Tasks.PlayAnimation("misschinese2_crystalmaze", "2int_loop_a_taocheng", 8.0f, AnimationFlags.Loop);
            p.ApplyVisuals();

            blip = new Blip(patient) { Sprite = (BlipSprite)280, Color = System.Drawing.Color.Yellow };
            return true;
        }
        public override void End() { base.End(); if (blip.Exists()) blip.Delete(); if (patient.Exists()) patient.Dismiss(); }
    }
}