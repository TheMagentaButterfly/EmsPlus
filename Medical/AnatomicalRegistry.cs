using Rage;

namespace EmsPlus.Medical
{
    public static class AnatomicalRegistry
    {
        public static bool IsToolValidForBone(EmsTreatment tool, PedBoneId bone)
        {
            switch (tool)
            {
                case EmsTreatment.Tourniquet:
                    return bone == PedBoneId.LeftUpperArm || bone == PedBoneId.RightUpperArm ||
                           bone == PedBoneId.LeftForeArm || bone == PedBoneId.RightForearm ||
                           bone == PedBoneId.LeftThigh || bone == PedBoneId.RightThigh ||
                           bone == PedBoneId.LeftCalf || bone == PedBoneId.RightCalf;

                case EmsTreatment.TractionSplint:
                    return bone == PedBoneId.LeftThigh || bone == PedBoneId.RightThigh ||
                           bone == PedBoneId.LeftCalf || bone == PedBoneId.RightCalf;

                case EmsTreatment.JunctionalTourniquet:
                    return bone == PedBoneId.LeftThigh || bone == PedBoneId.RightThigh ||
                           bone == PedBoneId.LeftUpperArm || bone == PedBoneId.RightUpperArm ||
                           bone == PedBoneId.Neck;

                case EmsTreatment.EyePatch:
                case EmsTreatment.EyeShield:
                case EmsTreatment.AirwayManagement:
                case EmsTreatment.Oxygen:
                case EmsTreatment.HighFlowOxygen:
                case EmsTreatment.BagValveMask:
                    return bone == PedBoneId.Head;

                case EmsTreatment.CervicalCollar:
                    return bone == PedBoneId.Neck;

                case EmsTreatment.ChestSeal:
                case EmsTreatment.NeedleDecomp:
                case EmsTreatment.CPR:
                case EmsTreatment.Defibrillation:
                    return bone == PedBoneId.Spine3 || bone == PedBoneId.Spine2;

                case EmsTreatment.PelvicBinder:
                    return bone == PedBoneId.Pelvis || bone == PedBoneId.SpineRoot;

                case EmsTreatment.IVAccess:
                case EmsTreatment.SalineBag:
                case EmsTreatment.Adrenaline:
                case EmsTreatment.Analgesia:
                    return bone == PedBoneId.LeftForeArm || bone == PedBoneId.RightForearm ||
                           bone == PedBoneId.LeftHand || bone == PedBoneId.RightHand;

                default:
                    return true;
            }
        }

        public static bool IsLocalizedTreatment(EmsTreatment t)
        {
            return t == EmsTreatment.Bandage || t == EmsTreatment.Tourniquet || t == EmsTreatment.JunctionalTourniquet ||
                   t == EmsTreatment.WoundPacking || t == EmsTreatment.Splint || t == EmsTreatment.TractionSplint ||
                   t == EmsTreatment.PelvicBinder || t == EmsTreatment.CervicalCollar || t == EmsTreatment.ChestSeal ||
                   t == EmsTreatment.NeedleDecomp || t == EmsTreatment.BurnDressing || t == EmsTreatment.IcePack ||
                   t == EmsTreatment.Irrigation || t == EmsTreatment.WetDressing || t == EmsTreatment.EyePatch ||
                   t == EmsTreatment.EyeShield || t == EmsTreatment.Suture || t == EmsTreatment.DirectPressure ||
                   t == EmsTreatment.StabiliseObject;
        }

        public static bool IsUniversalTreatment(EmsTreatment t)
        {
            return t == EmsTreatment.Oxygen ||
                   t == EmsTreatment.HighFlowOxygen ||
                   t == EmsTreatment.BagValveMask ||
                   t == EmsTreatment.AirwayManagement ||
                   t == EmsTreatment.CPR ||
                   t == EmsTreatment.Defibrillation ||
                   t == EmsTreatment.IVAccess ||
                   t == EmsTreatment.SalineBag;
        }
    }
}