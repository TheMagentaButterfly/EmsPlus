using Rage;

namespace EmsPlus.Medical.Conditions
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
                    return bone == PedBoneId.Head;

                case EmsTreatment.CervicalCollar:
                    return bone == PedBoneId.Neck;

                case EmsTreatment.ChestSeal:
                case EmsTreatment.NeedleDecomp:
                case EmsTreatment.CPR:
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

        // Helper to determine if a tool goes ON a specific bone, or treats the whole body.
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
    }
}