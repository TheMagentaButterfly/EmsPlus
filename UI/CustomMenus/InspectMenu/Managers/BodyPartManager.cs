using EmsPlus.Medical.Frameworks;
using EmsPlus.Managers;
using Rage;
using Rage.Native;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace EmsPlus.UI.CustomMenus.InspectMenu.Managers
{
    public class BodyPartManager
    {
        private readonly List<BodyPart> _parts = new List<BodyPart>();
        private readonly Dictionary<BodyPart, float> _scales = new Dictionary<BodyPart, float>();

        public BodyPart SelectedPart { get; private set; }
        public BodyPart HoveredPart { get; private set; }
        public IReadOnlyList<BodyPart> Parts => _parts;
        private List<BodyPart> _activeKitParts = new List<BodyPart>();


        public BodyPartManager(Ped patient)
        {
            _parts.Add(new BodyPart(Localization.Get("BP_HEAD"), PedBoneId.Head));

            _parts.Add(new BodyPart(Localization.Get("BP_NECK"), PedBoneId.Neck));
            _parts.Add(new BodyPart(Localization.Get("BP_CHEST"), PedBoneId.Spine3));
            _parts.Add(new BodyPart(Localization.Get("BP_STOMACH"), PedBoneId.Spine));

            _parts.Add(new BodyPart(Localization.Get("BP_L_UPPER_ARM"), PedBoneId.LeftUpperArm));
            _parts.Add(new BodyPart(Localization.Get("BP_L_FOREARM"), PedBoneId.LeftForeArm));
            _parts.Add(new BodyPart(Localization.Get("BP_L_HAND"), PedBoneId.LeftHand));

            _parts.Add(new BodyPart(Localization.Get("BP_R_UPPER_ARM"), PedBoneId.RightUpperArm));
            _parts.Add(new BodyPart(Localization.Get("BP_R_FOREARM"), PedBoneId.RightForearm));
            _parts.Add(new BodyPart(Localization.Get("BP_R_HAND"), PedBoneId.RightHand));

            _parts.Add(new BodyPart(Localization.Get("BP_L_THIGH"), PedBoneId.LeftThigh));
            _parts.Add(new BodyPart(Localization.Get("BP_L_CALF"), PedBoneId.LeftCalf));
            _parts.Add(new BodyPart(Localization.Get("BP_L_FOOT"), PedBoneId.LeftFoot));

            _parts.Add(new BodyPart(Localization.Get("BP_R_THIGH"), PedBoneId.RightThigh));
            _parts.Add(new BodyPart(Localization.Get("BP_R_CALF"), PedBoneId.RightCalf));
            _parts.Add(new BodyPart(Localization.Get("BP_R_FOOT"), PedBoneId.RightFoot));

            foreach (var p in _parts) _scales[p] = 1f;
        }

        public void Update(Ped patient, Point mousePos)
        {
            ManageKitParts();

            BodyPart newHovered = null;
            Vector2 mouse = new Vector2(mousePos.X, mousePos.Y);

            foreach (var p in _parts)
            {
                Vector3 worldPos;
                if (p.LinkedEntity != null && p.LinkedEntity.Exists())
                {
                    worldPos = p.LinkedEntity.Position;

                    worldPos += new Vector3(0f, 0f, 0.2f);
                }
                else
                {
                    worldPos = patient.GetBonePosition(p.BoneId);
                }

                if (NativeFunction.Natives.GET_SCREEN_COORD_FROM_WORLD_COORD<bool>(
                    worldPos, out float x, out float y))
                {
                    p.ScreenPosition = new Vector2(x * Game.Resolution.Width, y * Game.Resolution.Height);
                    float dist = Vector2.Distance(mouse, p.ScreenPosition);

                    if (dist < 40f && (newHovered == null || dist < Vector2.Distance(mouse, newHovered.ScreenPosition)))
                        newHovered = p;
                }
                else p.ScreenPosition = Vector2.Zero;
            }

            foreach (var p in _parts)
            {
                p.IsHovered = (p == newHovered);
                float target = p.IsHovered ? 1.3f : (SelectedPart == p ? 1.15f : 1f);
                if (!_scales.ContainsKey(p)) _scales[p] = 1f;
                _scales[p] = MathHelper.Lerp(_scales[p], target, 0.15f);
            }

            HoveredPart = newHovered;
        }

        private void ManageKitParts()
        {
            foreach (var kit in InventoryManager.PlacedKits)
            {
                if (kit.Prop != null && kit.Prop.Exists())
                {
                    if (!_activeKitParts.Any(bp => bp.LinkedEntity != null && bp.LinkedEntity.Handle == kit.Prop.Handle))
                    {
                        string kitName = kit.KitID;
                        if (kit.KitID == "TRAUMABAG") kitName = Localization.Get("TRAUMABAG_NAME");
                        else if (kit.KitID == "OXYGENBAG") kitName = Localization.Get("OXYGENBAG_NAME");
                        else if (kit.KitID == "DEFIBRILLATOR") kitName = Localization.Get("DEFIBRILLATOR_NAME");

                        var newPart = new BodyPart(kitName, kit.Prop);

                        _activeKitParts.Add(newPart);
                        _parts.Add(newPart);
                        _scales[newPart] = 1f;
                    }
                }
            }

            for (int i = _activeKitParts.Count - 1; i >= 0; i--)
            {
                var bp = _activeKitParts[i];
                if (bp.LinkedEntity == null || !bp.LinkedEntity.Exists())
                {
                    if (SelectedPart == bp) SelectedPart = null;
                    _parts.Remove(bp);
                    _scales.Remove(bp);
                    _activeKitParts.RemoveAt(i);
                }
            }
        }

        public void SelectPart(BodyPart part) => SelectedPart = part;
        public float GetScale(BodyPart part) => _scales.TryGetValue(part, out float s) ? s : 1f;
    }
}