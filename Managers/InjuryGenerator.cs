using EmsPlus.Medical;
using Rage;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EmsPlus.Managers
{
    public static class InjuryGenerator
    {
        private static Random Rnd = new Random();

        private static List<PedBoneId> CommonBones = new List<PedBoneId>
        {
            PedBoneId.Head, PedBoneId.Neck,
            PedBoneId.LeftUpperArm, PedBoneId.LeftForeArm,
            PedBoneId.RightUpperArm, PedBoneId.RightForearm,
            PedBoneId.LeftThigh, PedBoneId.LeftCalf,
            PedBoneId.RightThigh, PedBoneId.RightCalf
        };

        public static void GenerateRandom(Patient p, int minCount, int maxCount)
        {
            int count = Rnd.Next(minCount, maxCount + 1);
            var availableBones = new List<PedBoneId>(CommonBones);

            foreach (var injury in p.Conditions.OfType<PhysicalInjury>())

                for (int i = 0; i < count; i++)
            {
                if (availableBones.Count == 0) break;

                PedBoneId bone = availableBones[Rnd.Next(availableBones.Count)];

                string type = InjuryTypes.Bruising;
                int roll = Rnd.Next(0, 100);
                if (roll > 40) type = InjuryTypes.Laceration;
                if (roll > 85) type = InjuryTypes.Fracture;

                AddInjury(p, type, bone);
                availableBones.Remove(bone);
            }
        }
        public static void AddInjury(Patient p, string type, PedBoneId bone)
        {
            float severity = 0.5f;
            float bleed = 0f;
            int pain = 0;

            switch (type)
            {
                case InjuryTypes.Bruising: severity = 0.2f; bleed = 0f; pain = 1; break;
                case InjuryTypes.Laceration: severity = 0.6f; bleed = 2.0f; pain = 3; break;
                case InjuryTypes.Fracture: severity = 0.8f; bleed = 0.5f; pain = 6; break;
                case InjuryTypes.GunshotWound: severity = 0.9f; bleed = 4.0f; pain = 5; break;
            }

            p.Conditions.Add(new PhysicalInjury(type, bone, bleed, EmsTreatment.Bandage));
        }
    }
}