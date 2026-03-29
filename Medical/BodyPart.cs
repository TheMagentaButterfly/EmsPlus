using Rage;
using System.Collections.Generic;

namespace EmsPlus.Medical
{
    public class BodyPart
    {
        public string Name { get; set; }
        public PedBoneId BoneId { get; set; }
        public List<string> Injuries { get; set; } = new List<string>();

        public Entity LinkedEntity { get; set; }

        public Vector2 ScreenPosition { get; set; }
        public bool IsHovered { get; set; }

        public BodyPart(string name, PedBoneId bone)
        {
            Name = name;
            BoneId = bone;
            LinkedEntity = null;
        }

        public BodyPart(string name, Entity entity)
        {
            Name = name;
            LinkedEntity = entity;
            BoneId = PedBoneId.Pelvis;
        }
    }
}