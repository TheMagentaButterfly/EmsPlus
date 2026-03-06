using EmsPlus.Medical.Frameworks;
using Rage;

namespace EmsPlus.Medical.Conditions
{
    // =========================================================================
    // BONE REFERENCE TABLE
    // =========================================================================
    public static class InjuryBones
    {
        // ── Head & Neck ───────────────────────────────────────────────────────
        public static readonly PedBoneId Head = PedBoneId.Head;
        public static readonly PedBoneId Neck = PedBoneId.Neck;
        public static readonly PedBoneId Jaw = PedBoneId.Head;
        public static readonly PedBoneId LeftEye = PedBoneId.Head;
        public static readonly PedBoneId RightEye = PedBoneId.Head;

        // ── Spine & Torso ─────────────────────────────────────────────────────
        public static readonly PedBoneId CervicalSpine = PedBoneId.Neck;
        public static readonly PedBoneId ThoracicSpine = PedBoneId.Spine3;
        public static readonly PedBoneId LumbarSpine = PedBoneId.Spine;
        public static readonly PedBoneId Chest = PedBoneId.Spine3;
        public static readonly PedBoneId Abdomen = PedBoneId.Spine;
        public static readonly PedBoneId Pelvis = PedBoneId.Pelvis;

        // ── Left Arm ─────────────────────────────────────────────────────────
        public static readonly PedBoneId LeftShoulder = PedBoneId.LeftUpperArm;
        public static readonly PedBoneId LeftUpperArm = PedBoneId.LeftUpperArm;
        public static readonly PedBoneId LeftElbow = PedBoneId.LeftForeArm;
        public static readonly PedBoneId LeftForearm = PedBoneId.LeftForeArm;
        public static readonly PedBoneId LeftWrist = PedBoneId.LeftHand;
        public static readonly PedBoneId LeftHand = PedBoneId.LeftHand;

        // ── Right Arm ─────────────────────────────────────────────────────────
        public static readonly PedBoneId RightShoulder = PedBoneId.RightUpperArm;
        public static readonly PedBoneId RightUpperArm = PedBoneId.RightUpperArm;
        public static readonly PedBoneId RightElbow = PedBoneId.RightForearm;
        public static readonly PedBoneId RightForearm = PedBoneId.RightForearm;
        public static readonly PedBoneId RightWrist = PedBoneId.RightHand;
        public static readonly PedBoneId RightHand = PedBoneId.RightHand;

        // ── Left Leg ─────────────────────────────────────────────────────────
        public static readonly PedBoneId LeftHip = PedBoneId.LeftThigh;
        public static readonly PedBoneId LeftThigh = PedBoneId.LeftThigh;
        public static readonly PedBoneId LeftKnee = PedBoneId.LeftCalf;
        public static readonly PedBoneId LeftCalf = PedBoneId.LeftCalf;
        public static readonly PedBoneId LeftAnkle = PedBoneId.LeftFoot;
        public static readonly PedBoneId LeftFoot = PedBoneId.LeftFoot;

        // ── Right Leg ─────────────────────────────────────────────────────────
        public static readonly PedBoneId RightHip = PedBoneId.RightThigh;
        public static readonly PedBoneId RightThigh = PedBoneId.RightThigh;
        public static readonly PedBoneId RightKnee = PedBoneId.RightCalf;
        public static readonly PedBoneId RightCalf = PedBoneId.RightCalf;
        public static readonly PedBoneId RightAnkle = PedBoneId.RightFoot;
        public static readonly PedBoneId RightFoot = PedBoneId.RightFoot;

        // ── Junctional zones ──────────────────
        public static readonly PedBoneId LeftAxilla = PedBoneId.LeftUpperArm;
        public static readonly PedBoneId RightAxilla = PedBoneId.RightUpperArm;
        public static readonly PedBoneId LeftGroin = PedBoneId.LeftThigh;
        public static readonly PedBoneId RightGroin = PedBoneId.RightThigh;
    }

    // =========================================================================
    // INJURY FACTORY
    // =========================================================================
    public static class InjuryFactory
    {
        // =====================================================================
        public static class SoftTissue
        // =====================================================================
        {
            public static PhysicalInjury Laceration(PedBoneId bone) =>
                new PhysicalInjury("Laceration", bone, 0.4f,
                    EmsTreatment.Bandage);

            public static PhysicalInjury DeepLaceration(PedBoneId bone) =>
                new PhysicalInjury("Deep Laceration", bone, 0.9f,
                    EmsTreatment.Bandage, EmsTreatment.Suture);

            public static PhysicalInjury Abrasion(PedBoneId bone) =>
                new PhysicalInjury("Abrasion", bone, 0.05f,
                    EmsTreatment.Bandage);

            public static PhysicalInjury Avulsion(PedBoneId bone) =>
                new PhysicalInjury("Avulsion", bone, 1.4f,
                    EmsTreatment.Bandage, EmsTreatment.Suture);

            public static PhysicalInjury Degloving(PedBoneId bone) =>
                new PhysicalInjury("Degloving Injury", bone, 2.0f,
                    EmsTreatment.Bandage, EmsTreatment.Tourniquet);

            public static PhysicalInjury Contusion(PedBoneId bone) =>
                new PhysicalInjury("Contusion", bone, 0.05f,
                    EmsTreatment.IcePack);

            public static PhysicalInjury Haematoma(PedBoneId bone) =>
                new PhysicalInjury("Haematoma", bone, 0.1f,
                    EmsTreatment.IcePack, EmsTreatment.Bandage);

            public static PhysicalInjury Evisceration() =>
                new PhysicalInjury("Abdominal Evisceration", InjuryBones.Abdomen, 0.6f,
                    EmsTreatment.WetDressing, EmsTreatment.Bandage);

            public static PhysicalInjury PenetratingAbdominal() =>
                new PhysicalInjury("Penetrating Abdominal Wound", InjuryBones.Abdomen, 1.0f,
                    EmsTreatment.Bandage, EmsTreatment.IVAccess);

            public static PhysicalInjury CrushInjury(PedBoneId bone) =>
                new PhysicalInjury("Crush Injury", bone, 1.1f,
                    EmsTreatment.Bandage, EmsTreatment.Splint, EmsTreatment.IVAccess);
        }

        // =====================================================================
        public static class Amputation
        // =====================================================================
        {
            public static PhysicalInjury Complete(PedBoneId bone) =>
                new PhysicalInjury("Traumatic Amputation", bone, 3.5f,
                    EmsTreatment.Tourniquet, EmsTreatment.Bandage);

            public static PhysicalInjury Partial(PedBoneId bone) =>
                new PhysicalInjury("Partial Amputation", bone, 2.8f,
                    EmsTreatment.Tourniquet, EmsTreatment.Bandage);

            public static PhysicalInjury LeftArm() =>
                new PhysicalInjury("Traumatic Amputation (L Arm)", InjuryBones.LeftForearm, 3.5f,
                    EmsTreatment.Tourniquet, EmsTreatment.Bandage);

            public static PhysicalInjury RightArm() =>
                new PhysicalInjury("Traumatic Amputation (R Arm)", InjuryBones.RightForearm, 3.5f,
                    EmsTreatment.Tourniquet, EmsTreatment.Bandage);

            public static PhysicalInjury LeftLeg() =>
                new PhysicalInjury("Traumatic Amputation (L Leg)", InjuryBones.LeftThigh, 4.0f,
                    EmsTreatment.Tourniquet, EmsTreatment.Bandage);

            public static PhysicalInjury RightLeg() =>
                new PhysicalInjury("Traumatic Amputation (R Leg)", InjuryBones.RightThigh, 4.0f,
                    EmsTreatment.Tourniquet, EmsTreatment.Bandage);
        }

        // =====================================================================
        public static class Haemorrhage
        // =====================================================================
        {
            public static PhysicalInjury Arterial(PedBoneId bone)
            {
                bool isLimb =
                    bone == InjuryBones.LeftUpperArm || bone == InjuryBones.RightUpperArm ||
                    bone == InjuryBones.LeftForearm || bone == InjuryBones.RightForearm ||
                    bone == InjuryBones.LeftThigh || bone == InjuryBones.RightThigh ||
                    bone == InjuryBones.LeftCalf || bone == InjuryBones.RightCalf;

                bool isJunctional =
                    bone == InjuryBones.LeftGroin || bone == InjuryBones.RightGroin ||
                    bone == InjuryBones.LeftAxilla || bone == InjuryBones.RightAxilla ||
                    bone == InjuryBones.Neck;

                if (isLimb)
                    return new PhysicalInjury("Arterial Bleed", bone, 2.5f,
                        EmsTreatment.Tourniquet, EmsTreatment.Bandage);

                if (isJunctional)
                    return new PhysicalInjury("Junctional Arterial Bleed", bone, 3.0f,
                        EmsTreatment.WoundPacking, EmsTreatment.JunctionalTourniquet);

                return new PhysicalInjury("Non-Compressible Haemorrhage", bone, 2.0f,
                    EmsTreatment.WoundPacking, EmsTreatment.IVAccess, EmsTreatment.IVFluids);
            }

            public static PhysicalInjury Venous(PedBoneId bone) =>
                new PhysicalInjury("Venous Bleed", bone, 0.8f,
                    EmsTreatment.Bandage, EmsTreatment.DirectPressure);

            public static PhysicalInjury Junctional() =>
                new PhysicalInjury("Junctional Haemorrhage", InjuryBones.LeftGroin, 3.0f,
                    EmsTreatment.WoundPacking, EmsTreatment.JunctionalTourniquet);

            public static PhysicalInjury Internal() =>
                new PhysicalInjury("Internal Haemorrhage", InjuryBones.Abdomen, 1.5f,
                    EmsTreatment.IVAccess, EmsTreatment.IVFluids);

            public static PhysicalInjury Scalp() =>
                new PhysicalInjury("Scalp Laceration", InjuryBones.Head, 0.7f,
                    EmsTreatment.Bandage, EmsTreatment.DirectPressure);

            public static PhysicalInjury FemoralArtery() =>
                new PhysicalInjury("Femoral Artery Laceration", InjuryBones.LeftThigh, 4.0f,
                    EmsTreatment.Tourniquet, EmsTreatment.WoundPacking);

            public static PhysicalInjury BrachialArtery() =>
                new PhysicalInjury("Brachial Artery Laceration", InjuryBones.LeftUpperArm, 3.0f,
                    EmsTreatment.Tourniquet, EmsTreatment.WoundPacking);

            public static PhysicalInjury Jugular() =>
                new PhysicalInjury("Jugular Vein Laceration", InjuryBones.Neck, 2.0f,
                    EmsTreatment.WoundPacking, EmsTreatment.DirectPressure);

            public static PhysicalInjury AorticAneurysm() =>
                new PhysicalInjury("Aortic Aneurysm Rupture", InjuryBones.Abdomen, 5.0f,
                    EmsTreatment.IVAccess, EmsTreatment.IVFluids);
        }

        // =====================================================================
        public static class Gunshot
        // =====================================================================
        {
            public static PhysicalInjury Wound(PedBoneId bone) =>
                new PhysicalInjury("Gunshot Wound", bone, 1.8f,
                    EmsTreatment.Bandage);

            public static PhysicalInjury ThroughAndThrough(PedBoneId bone) =>
                new PhysicalInjury("Through-and-Through GSW", bone, 2.2f,
                    EmsTreatment.Bandage, EmsTreatment.WoundPacking);

            public static PhysicalInjury Chest() =>
                new PhysicalInjury("GSW to Chest", InjuryBones.Chest, 2.0f,
                    EmsTreatment.ChestSeal, EmsTreatment.Bandage);

            public static PhysicalInjury Abdomen() =>
                new PhysicalInjury("GSW to Abdomen", InjuryBones.Abdomen, 1.6f,
                    EmsTreatment.WoundPacking, EmsTreatment.Bandage, EmsTreatment.IVAccess);

            public static PhysicalInjury Head() =>
                new PhysicalInjury("GSW to Head", InjuryBones.Head, 3.2f,
                    EmsTreatment.Bandage, EmsTreatment.DirectPressure);

            public static PhysicalInjury Neck() =>
                new PhysicalInjury("GSW to Neck", InjuryBones.Neck, 2.8f,
                    EmsTreatment.DirectPressure, EmsTreatment.AirwayManagement);

            public static PhysicalInjury Pelvis() =>
                new PhysicalInjury("GSW to Pelvis", InjuryBones.Pelvis, 2.4f,
                    EmsTreatment.WoundPacking, EmsTreatment.PelvicBinder, EmsTreatment.IVAccess);
        }

        // =====================================================================
        public static class Blast
        // =====================================================================
        {
            public static PhysicalInjury ShrapnelWounds(PedBoneId bone) =>
                new PhysicalInjury("Multiple Shrapnel Wounds", bone, 1.3f,
                    EmsTreatment.Bandage, EmsTreatment.WoundPacking);

            public static PhysicalInjury BlastInjury() =>
                new PhysicalInjury("Blast Injury", InjuryBones.Chest, 1.0f,
                    EmsTreatment.Bandage, EmsTreatment.ChestSeal, EmsTreatment.Oxygen);

            public static PhysicalInjury EardrumRupture() =>
                new PhysicalInjury("Tympanic Membrane Rupture", InjuryBones.Head, 0.0f,
                    EmsTreatment.Monitoring);
        }

        // =====================================================================
        public static class StabAndPuncture
        // =====================================================================
        {
            public static PhysicalInjury StabWound(PedBoneId bone) =>
                new PhysicalInjury("Stab Wound", bone, 1.1f,
                    EmsTreatment.WoundPacking, EmsTreatment.Bandage);

            public static PhysicalInjury StabChest() =>
                new PhysicalInjury("Stab Wound to Chest", InjuryBones.Chest, 1.2f,
                    EmsTreatment.ChestSeal, EmsTreatment.Bandage);

            public static PhysicalInjury StabAbdomen() =>
                new PhysicalInjury("Stab Wound to Abdomen", InjuryBones.Abdomen, 1.0f,
                    EmsTreatment.WoundPacking, EmsTreatment.Bandage);

            public static PhysicalInjury StabNeck() =>
                new PhysicalInjury("Stab Wound to Neck", InjuryBones.Neck, 2.0f,
                    EmsTreatment.DirectPressure, EmsTreatment.AirwayManagement);

            public static PhysicalInjury ImpaledObject(PedBoneId bone) =>
                new PhysicalInjury("Impaled Object", bone, 0.6f,
                    EmsTreatment.StabiliseObject, EmsTreatment.Bandage);

            public static PhysicalInjury PenetratingNeck() =>
                new PhysicalInjury("Penetrating Neck Trauma", InjuryBones.Neck, 2.5f,
                    EmsTreatment.DirectPressure, EmsTreatment.AirwayManagement);
        }

        // =====================================================================
        public static class Fracture
        // =====================================================================
        {
            public static PhysicalInjury Compound(PedBoneId bone) =>
                new PhysicalInjury("Compound Fracture", bone, 1.2f,
                    EmsTreatment.Bandage, EmsTreatment.Splint);

            public static PhysicalInjury Simple(PedBoneId bone) =>
                new PhysicalInjury("Closed Fracture", bone, 0.0f,
                    EmsTreatment.Splint);

            public static PhysicalInjury Femoral() =>
                new PhysicalInjury("Femoral Fracture", InjuryBones.LeftThigh, 1.5f,
                    EmsTreatment.TractionSplint, EmsTreatment.IVAccess);

            public static PhysicalInjury Pelvic() =>
                new PhysicalInjury("Pelvic Fracture", InjuryBones.Pelvis, 2.5f,
                    EmsTreatment.PelvicBinder, EmsTreatment.IVAccess, EmsTreatment.IVFluids);

            public static PhysicalInjury PelvicCrush() =>
                new PhysicalInjury("Pelvic Crush Fracture", InjuryBones.Pelvis, 3.0f,
                    EmsTreatment.PelvicBinder, EmsTreatment.IVAccess, EmsTreatment.IVFluids);

            public static PhysicalInjury Rib() =>
                new PhysicalInjury("Rib Fracture", InjuryBones.Chest, 0.0f,
                    EmsTreatment.Oxygen);

            public static PhysicalInjury FlailChest() =>
                new PhysicalInjury("Flail Chest", InjuryBones.Chest, 0.1f,
                    EmsTreatment.BagValveMask, EmsTreatment.Oxygen);

            public static PhysicalInjury Spinal(PedBoneId bone) =>
                new PhysicalInjury("Suspected Spinal Fracture", bone, 0.0f,
                    EmsTreatment.SpinalImmobilisation, EmsTreatment.CervicalCollar);

            public static PhysicalInjury CervicalSpine() =>
                new PhysicalInjury("Cervical Spinal Fracture", InjuryBones.CervicalSpine, 0.0f,
                    EmsTreatment.CervicalCollar, EmsTreatment.SpinalImmobilisation);

            public static PhysicalInjury Ankle() =>
                new PhysicalInjury("Ankle Fracture", InjuryBones.LeftAnkle, 0.1f,
                    EmsTreatment.Splint);

            public static PhysicalInjury Wrist() =>
                new PhysicalInjury("Wrist Fracture", InjuryBones.LeftWrist, 0.05f,
                    EmsTreatment.Splint);

            public static PhysicalInjury Skull() =>
                new PhysicalInjury("Skull Fracture", InjuryBones.Head, 0.3f,
                    EmsTreatment.Bandage, EmsTreatment.Monitoring);
        }

        // =====================================================================
        public static class Chest
        // =====================================================================
        {
            public static PhysicalInjury SuckingChestWound() =>
                new PhysicalInjury("Sucking Chest Wound", InjuryBones.Chest, 0.8f,
                    EmsTreatment.ChestSeal);

            public static PhysicalInjury TensionPneumothorax() =>
                new PhysicalInjury("Tension Pneumothorax", InjuryBones.Chest, 0.1f,
                    EmsTreatment.NeedleDecomp);

            public static PhysicalInjury SimplePneumothorax() =>
                new PhysicalInjury("Simple Pneumothorax", InjuryBones.Chest, 0.0f,
                    EmsTreatment.Oxygen, EmsTreatment.NeedleDecomp);

            public static PhysicalInjury Haemothorax() =>
                new PhysicalInjury("Haemothorax", InjuryBones.Chest, 1.2f,
                    EmsTreatment.Oxygen, EmsTreatment.IVAccess);

            public static PhysicalInjury HaemoPneumothorax() =>
                new PhysicalInjury("Haemopneumothorax", InjuryBones.Chest, 1.5f,
                    EmsTreatment.ChestSeal, EmsTreatment.NeedleDecomp, EmsTreatment.IVAccess);

            public static PhysicalInjury CardiacTamponade() =>
                new PhysicalInjury("Cardiac Tamponade", InjuryBones.Chest, 0.0f,
                    EmsTreatment.IVAccess, EmsTreatment.IVFluids);

            public static PhysicalInjury AorticTransection() =>
                new PhysicalInjury("Aortic Transection", InjuryBones.Chest, 4.5f,
                    EmsTreatment.IVAccess, EmsTreatment.IVFluids);

            public static PhysicalInjury PulmonaryContusion() =>
                new PhysicalInjury("Pulmonary Contusion", InjuryBones.Chest, 0.0f,
                    EmsTreatment.Oxygen, EmsTreatment.BagValveMask);
        }

        // =====================================================================
        public static class HeadAndNeuro
        // =====================================================================
        {
            public static PhysicalInjury Concussion() =>
                new PhysicalInjury("Concussion", InjuryBones.Head, 0.0f,
                    EmsTreatment.Monitoring);

            public static PhysicalInjury IntracranialHaemorrhage() =>
                new PhysicalInjury("Intracranial Haemorrhage", InjuryBones.Head, 0.0f,
                    EmsTreatment.Oxygen, EmsTreatment.Monitoring);

            public static PhysicalInjury SubduralHaematoma() =>
                new PhysicalInjury("Subdural Haematoma", InjuryBones.Head, 0.0f,
                    EmsTreatment.Oxygen, EmsTreatment.Monitoring);

            public static PhysicalInjury EpiduralHaematoma() =>
                new PhysicalInjury("Epidural Haematoma", InjuryBones.Head, 0.0f,
                    EmsTreatment.Oxygen, EmsTreatment.Monitoring);

            public static PhysicalInjury FacialFracture(PedBoneId bone) =>
                new PhysicalInjury("Facial Fracture", bone, 0.4f,
                    EmsTreatment.Bandage, EmsTreatment.AirwayManagement);

            public static PhysicalInjury MandibleFracture() =>
                new PhysicalInjury("Mandible Fracture", InjuryBones.Jaw, 0.3f,
                    EmsTreatment.AirwayManagement, EmsTreatment.Bandage);

            public static PhysicalInjury OrbitalFracture() =>
                new PhysicalInjury("Orbital Fracture", InjuryBones.Head, 0.15f,
                    EmsTreatment.Bandage, EmsTreatment.EyePatch);

            public static PhysicalInjury EyeInjury() =>
                new PhysicalInjury("Penetrating Eye Injury", InjuryBones.LeftEye, 0.1f,
                    EmsTreatment.EyeShield, EmsTreatment.Bandage);

            public static PhysicalInjury Seizure() =>
                new PhysicalInjury("Seizure", InjuryBones.Head, 0.0f,
                    EmsTreatment.AirwayManagement, EmsTreatment.Monitoring, EmsTreatment.RecoveryPosition);
        }

        // =====================================================================
        public static class Burns
        // =====================================================================
        {
            public static PhysicalInjury FirstDegree(PedBoneId bone) =>
                new PhysicalInjury("1st Degree Burn", bone, 0.0f,
                    EmsTreatment.BurnDressing);

            public static PhysicalInjury SecondDegree(PedBoneId bone) =>
                new PhysicalInjury("2nd Degree Burn", bone, 0.1f,
                    EmsTreatment.BurnDressing, EmsTreatment.IVAccess);

            public static PhysicalInjury ThirdDegree(PedBoneId bone) =>
                new PhysicalInjury("3rd Degree Burn", bone, 0.2f,
                    EmsTreatment.BurnDressing, EmsTreatment.IVAccess, EmsTreatment.IVFluids);

            public static PhysicalInjury FourthDegree(PedBoneId bone) =>
                new PhysicalInjury("4th Degree Burn", bone, 0.35f,
                    EmsTreatment.BurnDressing, EmsTreatment.IVAccess, EmsTreatment.IVFluids);

            public static PhysicalInjury Inhalation() =>
                new PhysicalInjury("Inhalation Injury", InjuryBones.Head, 0.0f,
                    EmsTreatment.Oxygen, EmsTreatment.AirwayManagement);

            public static PhysicalInjury Chemical(PedBoneId bone) =>
                new PhysicalInjury("Chemical Burn", bone, 0.15f,
                    EmsTreatment.Irrigation, EmsTreatment.BurnDressing);

            public static PhysicalInjury Electrical(PedBoneId bone) =>
                new PhysicalInjury("Electrical Burn", bone, 0.2f,
                    EmsTreatment.BurnDressing, EmsTreatment.Monitoring, EmsTreatment.IVAccess);
        }

        // =====================================================================
        public static class Orthopaedic
        // =====================================================================
        {
            public static PhysicalInjury Dislocation(PedBoneId bone) =>
                new PhysicalInjury("Dislocation", bone, 0.1f,
                    EmsTreatment.Splint, EmsTreatment.Analgesia);

            public static PhysicalInjury ShoulderDislocation() =>
                new PhysicalInjury("Shoulder Dislocation", InjuryBones.LeftShoulder, 0.0f,
                    EmsTreatment.Splint, EmsTreatment.Analgesia);

            public static PhysicalInjury HipDislocation() =>
                new PhysicalInjury("Hip Dislocation", InjuryBones.LeftHip, 0.3f,
                    EmsTreatment.Splint, EmsTreatment.Analgesia);

            public static PhysicalInjury CompartmentSyndrome(PedBoneId bone) =>
                new PhysicalInjury("Compartment Syndrome", bone, 0.0f,
                    EmsTreatment.Monitoring, EmsTreatment.IVAccess);
        }

        // =====================================================================
        public static class Environmental
        // =====================================================================
        {
            public static PhysicalInjury NearDrowning() =>
                new PhysicalInjury("Near Drowning", InjuryBones.Chest, 0.0f,
                    EmsTreatment.Oxygen, EmsTreatment.BagValveMask, EmsTreatment.Monitoring);

            public static PhysicalInjury Hypothermia() =>
                new PhysicalInjury("Hypothermia", InjuryBones.Abdomen, 0.0f,
                    EmsTreatment.ActiveRewarming, EmsTreatment.Monitoring);

            public static PhysicalInjury HeatStroke() =>
                new PhysicalInjury("Heat Stroke", InjuryBones.Abdomen, 0.0f,
                    EmsTreatment.ActiveCooling, EmsTreatment.IVAccess, EmsTreatment.IVFluids);

            public static PhysicalInjury CarbonMonoxidePoisoning() =>
                new PhysicalInjury("Carbon Monoxide Poisoning", InjuryBones.Abdomen, 0.0f,
                    EmsTreatment.HighFlowOxygen, EmsTreatment.Monitoring);

            public static PhysicalInjury Anaphylaxis() =>
                new PhysicalInjury("Anaphylaxis", InjuryBones.Abdomen, 0.0f,
                    EmsTreatment.Adrenaline, EmsTreatment.Oxygen, EmsTreatment.IVAccess);

            public static PhysicalInjury Rhabdomyolysis() =>
                new PhysicalInjury("Rhabdomyolysis", InjuryBones.Abdomen, 0.0f,
                    EmsTreatment.IVAccess, EmsTreatment.IVFluids, EmsTreatment.Monitoring);
        }
    }
}