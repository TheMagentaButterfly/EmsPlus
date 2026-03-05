using Rage;
using System.Collections.Generic;

namespace EmsPlus.Medical.Frameworks
{
    public class BodyPart
    {
        public string Name { get; set; }
        public PedBoneId BoneId { get; set; }
        public List<string> Injuries { get; set; } = new List<string>(); // e.g., "Gunshot Wound", "Bruising"

        public Entity LinkedEntity { get; set; }

        // UI State
        public Vector2 ScreenPosition { get; set; }
        public bool IsHovered { get; set; }

        // Constructor for Bones
        public BodyPart(string name, PedBoneId bone)
        {
            Name = name;
            BoneId = bone;
            LinkedEntity = null;
        }

        // Constructor for Props (Kits)
        public BodyPart(string name, Entity entity)
        {
            Name = name;
            LinkedEntity = entity;
            BoneId = PedBoneId.Pelvis;
        }
    }
}