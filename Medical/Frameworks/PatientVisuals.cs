using Rage;
using Rage.Native;
using System.Linq;

namespace EmsPlus.Medical.Frameworks
{
    public static class PatientVisuals
    {
        public static void ApplyBloodEffects(Patient p)
        {
            if (p.Character == null || !p.Character.Exists()) return;

            NativeFunction.Natives.CLEAR_PED_BLOOD_DAMAGE(p.Character);

            foreach (var injury in p.Conditions.OfType<PhysicalInjury>())
            {
                if (injury.BleedSeverity <= 0f || injury.IsTreated) continue;

                GameFiber.StartNew(delegate
                {
                    string dict = "core";
                    NativeFunction.Natives.REQUEST_NAMED_PTFX_ASSET(dict);
                    while (!NativeFunction.Natives.HAS_NAMED_PTFX_ASSET_LOADED<bool>(dict)) GameFiber.Yield();

                    NativeFunction.Natives.USE_PARTICLE_FX_ASSET(dict);
                    NativeFunction.Natives.START_PARTICLE_FX_NON_LOOPED_ON_PED_BONE(
                        "blood_stab", p.Character, 0f, 0f, 0f, 0f, 0f, 0f,
                        (int)injury.Bone, 1.5f, false, false, false);

                    if (injury.BleedSeverity >= 1.0f)
                    {
                        NativeFunction.Natives.USE_PARTICLE_FX_ASSET(dict);
                        Vector3 bonePos = p.Character.GetBonePosition(injury.Bone);
                        NativeFunction.Natives.START_PARTICLE_FX_NON_LOOPED_AT_COORD(
                            "blood_stab_puddle", bonePos.X, bonePos.Y, p.Character.Position.Z,
                            0f, 0f, 0f, 2.0f, false, false, false);
                    }
                });

                string pack = injury.BleedSeverity > 2.0f ? "Explosion_Med" : "BigHitByVehicle";
                NativeFunction.Natives.APPLY_PED_DAMAGE_PACK(p.Character, pack, 1.0f, 1.0f);
            }
        }
    }
}