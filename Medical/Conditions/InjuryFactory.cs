using EmsPlus.Medical.Frameworks;
using Rage;

namespace EmsPlus.Medical.Conditions
{
    public static class InjuryFactory
    {
        // =====================================================================
        // LACERATIONS & SOFT TISSUE
        // =====================================================================
        public static PhysicalInjury Laceration(PedBoneId bone) =>
            new PhysicalInjury("Laceration", bone, 0.4f, EmsTreatment.Bandage);
        public static PhysicalInjury DeepLaceration(PedBoneId bone) =>
            new PhysicalInjury("Deep Laceration", bone, 0.9f, EmsTreatment.Bandage, EmsTreatment.Suture);
        public static PhysicalInjury Abrasion(PedBoneId bone) =>
            new PhysicalInjury("Abrasion", bone, 0.05f, EmsTreatment.Bandage);
        public static PhysicalInjury Avulsion(PedBoneId bone) =>
            new PhysicalInjury("Avulsion", bone, 1.4f, EmsTreatment.Bandage, EmsTreatment.Suture);
        public static PhysicalInjury Degloving(PedBoneId bone) =>
            new PhysicalInjury("Degloving Injury", bone, 2.0f, EmsTreatment.Bandage, EmsTreatment.Tourniquet);
        public static PhysicalInjury Amputation(PedBoneId bone) =>
            new PhysicalInjury("Traumatic Amputation", bone, 3.5f, EmsTreatment.Tourniquet, EmsTreatment.Bandage);
        public static PhysicalInjury PartialAmputation(PedBoneId bone) =>
            new PhysicalInjury("Partial Amputation", bone, 2.8f, EmsTreatment.Tourniquet, EmsTreatment.Bandage);
        public static PhysicalInjury Contusion(PedBoneId bone) =>
            new PhysicalInjury("Contusion", bone, 0.05f, EmsTreatment.IcePack);
        public static PhysicalInjury Haematoma(PedBoneId bone) =>
            new PhysicalInjury("Haematoma", bone, 0.1f, EmsTreatment.IcePack, EmsTreatment.Bandage);
        public static PhysicalInjury CrushInjury(PedBoneId bone) =>
            new PhysicalInjury("Crush Injury", bone, 1.1f, EmsTreatment.Bandage, EmsTreatment.Splint, EmsTreatment.IVAccess);
        public static PhysicalInjury Evisceration() =>
            new PhysicalInjury("Abdominal Evisceration", PedBoneId.Spine1, 0.6f, EmsTreatment.WetDressing, EmsTreatment.Bandage);
        public static PhysicalInjury PenetratingAbdominal() =>
            new PhysicalInjury("Penetrating Abdominal Wound", PedBoneId.Spine1, 1.0f, EmsTreatment.Bandage, EmsTreatment.IVAccess);

        // =====================================================================
        // HAEMORRHAGE
        // =====================================================================

        public static PhysicalInjury ArterialBleed(PedBoneId bone)
        {
            if (bone == PedBoneId.Head || bone == PedBoneId.Neck)
            {
                return new PhysicalInjury("Severe Laceration", bone, 1.2f, EmsTreatment.JunctionalTourniquet, EmsTreatment.Bandage);
            }
            bool isLimb = bone == PedBoneId.LeftUpperArm || bone == PedBoneId.RightUpperArm ||
                          bone == PedBoneId.LeftForeArm || bone == PedBoneId.RightForearm ||
                          bone == PedBoneId.LeftThigh || bone == PedBoneId.RightThigh ||
                          bone == PedBoneId.LeftCalf || bone == PedBoneId.RightCalf;
            if (isLimb)
            {
                return new PhysicalInjury("Arterial Bleed", bone, 2.5f, EmsTreatment.Tourniquet, EmsTreatment.Bandage);
            }
            return new PhysicalInjury("Arterial Bleed", bone, 2.0f, EmsTreatment.WoundPacking, EmsTreatment.Bandage);
        }
        public static PhysicalInjury VenousBleed(PedBoneId bone) =>
            new PhysicalInjury("Venous Bleed", bone, 0.8f, EmsTreatment.Bandage, EmsTreatment.DirectPressure);
        public static PhysicalInjury JunctionalHaemorrhage() =>
            // Groin/axilla/neck — tourniquet can't reach, needs wound packing
            new PhysicalInjury("Junctional Haemorrhage", PedBoneId.LeftThigh, 3.0f, EmsTreatment.WoundPacking, EmsTreatment.JunctionalTourniquet);
        public static PhysicalInjury InternalHaemorrhage() =>
            // No external fix — IV fluids + hospital
            new PhysicalInjury("Internal Haemorrhage", PedBoneId.Spine1, 1.5f, EmsTreatment.IVAccess, EmsTreatment.IVFluids);
        public static PhysicalInjury ScalpLaceration() =>
            // Scalp is very vascular — bleeds a lot but rarely life-threatening alone
            new PhysicalInjury("Scalp Laceration", PedBoneId.Head, 0.7f, EmsTreatment.Bandage, EmsTreatment.DirectPressure);

        // =====================================================================
        // GUNSHOT & BLAST
        // =====================================================================
        public static PhysicalInjury GunshotWound(PedBoneId bone) =>
            new PhysicalInjury("Gunshot Wound", bone, 1.8f, EmsTreatment.Bandage);
        public static PhysicalInjury ThroughAndThroughGSW(PedBoneId bone) =>
            // Entry and exit wound — both need sealing
            new PhysicalInjury("Through-and-Through GSW", bone, 2.2f, EmsTreatment.Bandage, EmsTreatment.WoundPacking);
        public static PhysicalInjury GunshotWoundChest() =>
            new PhysicalInjury("GSW to Chest", PedBoneId.Spine3, 2.0f, EmsTreatment.ChestSeal, EmsTreatment.Bandage);
        public static PhysicalInjury GunshotWoundAbdomen() =>
            new PhysicalInjury("GSW to Abdomen", PedBoneId.Spine1, 1.6f, EmsTreatment.WoundPacking, EmsTreatment.Bandage, EmsTreatment.IVAccess);
        public static PhysicalInjury GunshotWoundHead() =>
            new PhysicalInjury("GSW to Head", PedBoneId.Head, 3.2f, EmsTreatment.Bandage, EmsTreatment.DirectPressure);
        public static PhysicalInjury ShrapnelWounds(PedBoneId bone) =>
            new PhysicalInjury("Multiple Shrapnel Wounds", bone, 1.3f, EmsTreatment.Bandage, EmsTreatment.WoundPacking);
        public static PhysicalInjury BlastInjury() =>
            // Barotrauma to lungs/eardrums, possible pneumo, burns, fragmentation
            new PhysicalInjury("Blast Injury", PedBoneId.Spine3, 1.0f, EmsTreatment.Bandage, EmsTreatment.ChestSeal, EmsTreatment.Oxygen);

        // =====================================================================
        // STAB / PUNCTURE
        // =====================================================================
        public static PhysicalInjury StabWound(PedBoneId bone) =>
            new PhysicalInjury("Stab Wound", bone, 1.1f, EmsTreatment.WoundPacking, EmsTreatment.Bandage);
        public static PhysicalInjury StabWoundChest() =>
            new PhysicalInjury("Stab Wound to Chest", PedBoneId.Spine3, 1.2f, EmsTreatment.ChestSeal, EmsTreatment.Bandage);
        public static PhysicalInjury ImpaledObject(PedBoneId bone) =>
            // NEVER remove an impaled object in the field
            new PhysicalInjury("Impaled Object", bone, 0.6f, EmsTreatment.StabiliseObject, EmsTreatment.Bandage);

        // =====================================================================
        // FRACTURES
        // =====================================================================
        public static PhysicalInjury CompoundFracture(PedBoneId bone) =>
            new PhysicalInjury("Compound Fracture", bone, 1.2f, EmsTreatment.Bandage, EmsTreatment.Splint);
        public static PhysicalInjury SimpleFracture(PedBoneId bone) =>
            new PhysicalInjury("Closed Fracture", bone, 0.0f, EmsTreatment.Splint);
        public static PhysicalInjury FemoralFracture() =>
            // Femur can hide 1–2L of blood internally in the thigh compartment
            new PhysicalInjury("Femoral Fracture", PedBoneId.LeftThigh, 1.5f, EmsTreatment.TractionSplint, EmsTreatment.IVAccess);
        public static PhysicalInjury PelvicFracture() =>
            // Pelvic ring fractures can cause massive internal haemorrhage
            new PhysicalInjury("Pelvic Fracture", PedBoneId.Pelvis, 2.5f, EmsTreatment.PelvicBinder, EmsTreatment.IVAccess, EmsTreatment.IVFluids);
        public static PhysicalInjury RibFracture() =>
            // Multiple rib fracs = risk of flail chest and pneumo
            new PhysicalInjury("Rib Fracture", PedBoneId.Spine3, 0.0f, EmsTreatment.Oxygen);
        public static PhysicalInjury FlailChest() =>
            // Segment of chest wall moves paradoxically — severe resp compromise
            new PhysicalInjury("Flail Chest", PedBoneId.Spine3, 0.1f, EmsTreatment.BagValveMask, EmsTreatment.Oxygen);
        public static PhysicalInjury SpinalFracture(PedBoneId bone) =>
            // Movement risk — spinal immobilisation critical
            new PhysicalInjury("Suspected Spinal Fracture", bone, 0.0f, EmsTreatment.SpinalImmobilisation, EmsTreatment.CervicalCollar);
        public static PhysicalInjury SpinalFractureCervical() =>
            new PhysicalInjury("Cervical Spinal Fracture", PedBoneId.Head, 0.0f, EmsTreatment.CervicalCollar, EmsTreatment.SpinalImmobilisation);
        public static PhysicalInjury CrushFracturePelvis() =>
            new PhysicalInjury("Pelvic Crush Fracture", PedBoneId.Pelvis, 3.0f, EmsTreatment.PelvicBinder, EmsTreatment.IVAccess, EmsTreatment.IVFluids);

        // =====================================================================
        // CHEST / RESPIRATORY
        // =====================================================================
        public static PhysicalInjury SuckingChestWound() =>
            new PhysicalInjury("Sucking Chest Wound", PedBoneId.Spine3, 0.8f, EmsTreatment.ChestSeal);
        public static PhysicalInjury TensionPneumothorax() =>
            new PhysicalInjury("Tension Pneumothorax", PedBoneId.Spine3, 0.1f, EmsTreatment.NeedleDecomp);
        public static PhysicalInjury SimplePneumothorax() =>
            // Lung collapses but no tension — less immediately fatal
            new PhysicalInjury("Simple Pneumothorax", PedBoneId.Spine3, 0.0f, EmsTreatment.Oxygen, EmsTreatment.NeedleDecomp);
        public static PhysicalInjury Haemothorax() =>
            // Blood in pleural cavity — compresses lung
            new PhysicalInjury("Haemothorax", PedBoneId.Spine3, 1.2f, EmsTreatment.Oxygen, EmsTreatment.IVAccess);
        public static PhysicalInjury HaemoPneumothorax() =>
            new PhysicalInjury("Haemopneumothorax", PedBoneId.Spine3, 1.5f, EmsTreatment.ChestSeal, EmsTreatment.NeedleDecomp, EmsTreatment.IVAccess);
        public static PhysicalInjury CardiacTamponade() =>
            // Blood in pericardium — compresses heart — Beck's triad
            new PhysicalInjury("Cardiac Tamponade", PedBoneId.Spine3, 0.0f, EmsTreatment.IVAccess, EmsTreatment.IVFluids);
        public static PhysicalInjury AorticTransection() =>
            // Rapid deceleration injury — almost universally fatal without OR
            new PhysicalInjury("Aortic Transection", PedBoneId.Spine1, 4.5f, EmsTreatment.IVAccess, EmsTreatment.IVFluids);
        public static PhysicalInjury PulmonaryContusion() =>
            // Bruised lung — delayed hypoxia, worsens over hours
            new PhysicalInjury("Pulmonary Contusion", PedBoneId.Spine3, 0.0f, EmsTreatment.Oxygen, EmsTreatment.BagValveMask);

        // =====================================================================
        // HEAD & NEUROLOGICAL
        // =====================================================================
        public static PhysicalInjury Concussion() =>
            new PhysicalInjury("Concussion", PedBoneId.Head, 0.0f, EmsTreatment.Monitoring);
        public static PhysicalInjury SkullFracture() =>
            new PhysicalInjury("Skull Fracture", PedBoneId.Head, 0.3f, EmsTreatment.Bandage, EmsTreatment.Monitoring);
        public static PhysicalInjury IntracranialHaemorrhage() =>
            // Raised ICP — worsens with hypoxia and hypotension
            new PhysicalInjury("Intracranial Haemorrhage", PedBoneId.Head, 0.0f, EmsTreatment.Oxygen, EmsTreatment.Monitoring);
        public static PhysicalInjury SubduralHaematoma() =>
            // Bridging vein tear — can be lucid then deteriorate (talk and die)
            new PhysicalInjury("Subdural Haematoma", PedBoneId.Head, 0.0f, EmsTreatment.Oxygen, EmsTreatment.Monitoring);
        public static PhysicalInjury EpiduralHaematoma() =>
            // Arterial — rapid lucid interval then rapid deterioration
            new PhysicalInjury("Epidural Haematoma", PedBoneId.Head, 0.0f, EmsTreatment.Oxygen, EmsTreatment.Monitoring);
        public static PhysicalInjury FacialFracture(PedBoneId bone) =>
            // Risk of airway compromise
            new PhysicalInjury("Facial Fracture", bone, 0.4f, EmsTreatment.Bandage, EmsTreatment.AirwayManagement);
        public static PhysicalInjury MandibleFracture() =>
            // Jaw fracture — airway compromise risk
            new PhysicalInjury("Mandible Fracture", PedBoneId.Head, 0.3f, EmsTreatment.AirwayManagement, EmsTreatment.Bandage);
        public static PhysicalInjury OrbitFracture() =>
            new PhysicalInjury("Orbital Fracture", PedBoneId.Head, 0.15f, EmsTreatment.Bandage, EmsTreatment.EyePatch);
        public static PhysicalInjury EyeInjury() =>
            new PhysicalInjury("Penetrating Eye Injury", PedBoneId.Head, 0.1f, EmsTreatment.EyeShield, EmsTreatment.Bandage);

        // =====================================================================
        // BURNS
        // =====================================================================
        public static PhysicalInjury FirstDegreeBurn(PedBoneId bone) =>
            new PhysicalInjury("1st Degree Burn", bone, 0.0f, EmsTreatment.BurnDressing);
        public static PhysicalInjury SecondDegreeBurn(PedBoneId bone) =>
            new PhysicalInjury("2nd Degree Burn", bone, 0.1f, EmsTreatment.BurnDressing, EmsTreatment.IVAccess);
        public static PhysicalInjury ThirdDegreeBurn(PedBoneId bone) =>
            new PhysicalInjury("3rd Degree Burn", bone, 0.2f, EmsTreatment.BurnDressing, EmsTreatment.IVAccess, EmsTreatment.IVFluids);
        public static PhysicalInjury FourthDegreeBurn(PedBoneId bone) =>
            // Full thickness to bone — char burns — fluid loss massive
            new PhysicalInjury("4th Degree Burn", bone, 0.35f, EmsTreatment.BurnDressing, EmsTreatment.IVAccess, EmsTreatment.IVFluids);
        public static PhysicalInjury InhalationInjury() =>
            // Upper airway burns from hot gas — rapid airway oedema
            new PhysicalInjury("Inhalation Injury", PedBoneId.Head, 0.0f, EmsTreatment.Oxygen, EmsTreatment.AirwayManagement);
        public static PhysicalInjury ChemicalBurn(PedBoneId bone) =>
            // Alkali/acid — continues to burn until irrigated
            new PhysicalInjury("Chemical Burn", bone, 0.15f, EmsTreatment.Irrigation, EmsTreatment.BurnDressing);
        public static PhysicalInjury ElectricalBurn(PedBoneId bone) =>
            // Entry/exit wound, internal burns along current path, arrhythmia risk
            new PhysicalInjury("Electrical Burn", bone, 0.2f, EmsTreatment.BurnDressing, EmsTreatment.Monitoring, EmsTreatment.IVAccess);

        // =====================================================================
        // JOINTS & EXTREMITIES
        // =====================================================================
        public static PhysicalInjury Dislocation(PedBoneId bone) =>
            // Do NOT reduce in field without analgesia and imaging
            new PhysicalInjury("Dislocation", bone, 0.1f, EmsTreatment.Splint, EmsTreatment.Analgesia);
        public static PhysicalInjury ShoulderDislocation() =>
            new PhysicalInjury("Shoulder Dislocation", PedBoneId.LeftUpperArm, 0.0f, EmsTreatment.Splint, EmsTreatment.Analgesia);
        public static PhysicalInjury HipDislocation() =>
            // Sciatic nerve at risk — time critical
            new PhysicalInjury("Hip Dislocation", PedBoneId.LeftThigh, 0.3f, EmsTreatment.Splint, EmsTreatment.Analgesia);
        public static PhysicalInjury AnkleFracture() =>
            new PhysicalInjury("Ankle Fracture", PedBoneId.LeftFoot, 0.1f, EmsTreatment.Splint);
        public static PhysicalInjury WristFracture() =>
            new PhysicalInjury("Wrist Fracture", PedBoneId.LeftHand, 0.05f, EmsTreatment.Splint);
        public static PhysicalInjury CompartmentSyndrome(PedBoneId bone) =>
            // Pressure in fascial compartment — no prehospital fix, just monitor
            new PhysicalInjury("Compartment Syndrome", bone, 0.0f, EmsTreatment.Monitoring, EmsTreatment.IVAccess);

        // =====================================================================
        // ENVIRONMENTAL & TOXICOLOGICAL
        // =====================================================================
        public static PhysicalInjury Drowning() =>
            new PhysicalInjury("Near Drowning", PedBoneId.Spine3, 0.0f, EmsTreatment.Oxygen, EmsTreatment.BagValveMask, EmsTreatment.Monitoring);
        public static PhysicalInjury Hypothermia() =>
            // Core temp <35°C — watch for rewarming arrhythmias
            new PhysicalInjury("Hypothermia", PedBoneId.Spine1, 0.0f, EmsTreatment.ActiveRewarming, EmsTreatment.Monitoring);
        public static PhysicalInjury Hyperthermia() =>
            // Heat stroke — core >40°C — cool aggressively
            new PhysicalInjury("Heat Stroke", PedBoneId.Spine1, 0.0f, EmsTreatment.ActiveCooling, EmsTreatment.IVAccess, EmsTreatment.IVFluids);
        public static PhysicalInjury CarbonMonoxidePoisoning() =>
            // CO binds haemoglobin — SpO2 reads falsely normal — only high flow O2 helps
            new PhysicalInjury("Carbon Monoxide Poisoning", PedBoneId.Spine1, 0.0f, EmsTreatment.HighFlowOxygen, EmsTreatment.Monitoring);
        public static PhysicalInjury AnaphylaxisInjury() =>
            // Trigger-based — airway swelling, circulatory collapse
            new PhysicalInjury("Anaphylaxis", PedBoneId.Spine1, 0.0f, EmsTreatment.Adrenaline, EmsTreatment.Oxygen, EmsTreatment.IVAccess);
        public static PhysicalInjury SeizureInjury() =>
            // Not a bleed source but needs airway management and monitoring
            new PhysicalInjury("Seizure", PedBoneId.Head, 0.0f, EmsTreatment.AirwayManagement, EmsTreatment.Monitoring, EmsTreatment.RecoveryPosition);
        public static PhysicalInjury Rhabdomyolysis() =>
            // Muscle crush — myoglobin dumps into circulation — renal failure risk
            new PhysicalInjury("Rhabdomyolysis", PedBoneId.Spine1, 0.0f, EmsTreatment.IVAccess, EmsTreatment.IVFluids, EmsTreatment.Monitoring);

        // =====================================================================
        // VASCULAR
        // =====================================================================
        public static PhysicalInjury AorticAneurysm() =>
            new PhysicalInjury("Aortic Aneurysm Rupture", PedBoneId.Spine1, 5.0f, EmsTreatment.IVAccess, EmsTreatment.IVFluids);
        public static PhysicalInjury FemoralArteryLaceration() =>
            new PhysicalInjury("Femoral Artery Laceration", PedBoneId.LeftThigh, 4.0f, EmsTreatment.Tourniquet, EmsTreatment.WoundPacking);
        public static PhysicalInjury BrachialArteryLaceration() =>
            new PhysicalInjury("Brachial Artery Laceration", PedBoneId.LeftUpperArm, 3.0f, EmsTreatment.Tourniquet, EmsTreatment.WoundPacking);
        public static PhysicalInjury JugularLaceration() =>
            // Also air embolism risk
            new PhysicalInjury("Jugular Vein Laceration", PedBoneId.Head, 2.0f, EmsTreatment.WoundPacking, EmsTreatment.DirectPressure);
        public static PhysicalInjury NeckPenetratingTrauma() =>
            // Zone-based — massive haemorrhage + airway + spinal risk
            new PhysicalInjury("Penetrating Neck Trauma", PedBoneId.Neck, 2.5f, EmsTreatment.DirectPressure, EmsTreatment.AirwayManagement);
    }
}