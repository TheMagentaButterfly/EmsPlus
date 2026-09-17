using EmsPlus.Medical;
using Rage;
using Rage.Native;
using System.Collections.Generic;
using System.Linq;

namespace EmsPlus.Managers
{
    public class PlacedKit { public Object Prop { get; set; } public string KitID { get; set; } }
    public class EquippedKit { public string KitID { get; set; } public Object Prop { get; set; } }

    public static class InventoryManager
    {
        public static List<EquippedKit> EquippedKits { get; private set; } = new List<EquippedKit>();
        public static List<PlacedKit> PlacedKits { get; private set; } = new List<PlacedKit>();
        public static EmsTreatment ActiveTool { get; set; } = EmsTreatment.None;

        public static Dictionary<EmsTreatment, int> CurrentSupplies { get; private set; } = new Dictionary<EmsTreatment, int>();

        static InventoryManager() => RestockSupplies();

        public static void RestockSupplies()
        {
            CurrentSupplies.Clear();
            CurrentSupplies[EmsTreatment.Bandage] = 4;
            CurrentSupplies[EmsTreatment.WoundPacking] = 2;
            CurrentSupplies[EmsTreatment.Tourniquet] = 1;
            CurrentSupplies[EmsTreatment.JunctionalTourniquet] = 1;
            CurrentSupplies[EmsTreatment.IcePack] = 2;
            CurrentSupplies[EmsTreatment.StabiliseObject] = 1;
            CurrentSupplies[EmsTreatment.Splint] = 1;
            CurrentSupplies[EmsTreatment.TractionSplint] = 1;
            CurrentSupplies[EmsTreatment.PelvicBinder] = 1;
            CurrentSupplies[EmsTreatment.CervicalCollar] = 1;
            CurrentSupplies[EmsTreatment.IVAccess] = 4;
            CurrentSupplies[EmsTreatment.SalineBag] = 2;
            CurrentSupplies[EmsTreatment.Adrenaline] = 2;
            CurrentSupplies[EmsTreatment.Naloxone] = 2;
            CurrentSupplies[EmsTreatment.Glucose] = 2;
            CurrentSupplies[EmsTreatment.ChestSeal] = 2;
            CurrentSupplies[EmsTreatment.NeedleDecomp] = 2;
            CurrentSupplies[EmsTreatment.WetDressing] = 1;
            CurrentSupplies[EmsTreatment.BurnDressing] = 1;
            CurrentSupplies[EmsTreatment.Irrigation] = 1;
            CurrentSupplies[EmsTreatment.EyePatch] = 2;
            CurrentSupplies[EmsTreatment.EyeShield] = 2;
        }

        public static bool HasSupply(EmsTreatment treatment) =>
            AmbulanceManager.IsPlayerInRearCabin || !CurrentSupplies.ContainsKey(treatment) || CurrentSupplies[treatment] > 0;

        public static void ConsumeSupply(EmsTreatment treatment)
        {
            if (!AmbulanceManager.IsPlayerInRearCabin && CurrentSupplies.ContainsKey(treatment) && CurrentSupplies[treatment] > 0)
                CurrentSupplies[treatment]--;
        }

        public static bool IsKitAvailable(string kitID, Vector3 position, float range = 5.0f)
        {
            if (kitID == "NONE" || AmbulanceManager.IsPlayerInRearCabin || HasKit(kitID)) return true;
            return PlacedKits.Any(k => k.KitID == kitID && k.Prop != null && k.Prop.Exists() && k.Prop.DistanceTo(position) <= range);
        }

        public static bool HasKit(string kitID) => EquippedKits.Any(k => k.KitID == kitID);

        public static void EquipKit(string kitID)
        {
            if (HasKit(kitID)) { StowKit(kitID); return; }

            Ped player = Game.LocalPlayer.Character;
            string animDict = EntryPoint.AnimationConfig.InteractDict.Value;
            string animName = EntryPoint.AnimationConfig.InteractName.Value;

            NativeFunction.Natives.REQUEST_ANIM_DICT(animDict);
            while (!NativeFunction.Natives.HAS_ANIM_DICT_LOADED<bool>(animDict)) GameFiber.Yield();

            player.Tasks.PlayAnimation(animDict, animName, 2.0f, AnimationFlags.None);
            GameFiber.Wait(1000);

            string modelName = kitID == "TRAUMABAG" ? EntryPoint.PropConfig.TraumaBagModel :
                               kitID == "OXYGENBAG" ? EntryPoint.PropConfig.OxygenBagModel :
                               kitID == "DEFIBRILLATOR" ? EntryPoint.PropConfig.DefibrillatorModel : "";

            if (string.IsNullOrEmpty(modelName)) return;

            Model m = new Model(modelName);
            m.LoadAndWait();
            if (!m.IsValid) return;

            Object newProp = new Object(m, player.Position);
            m.Dismiss();

            EquippedKits.Add(new EquippedKit { KitID = kitID, Prop = newProp });
            ReAttachProps();
        }

        public static void EquipAllKits()
        {
            Ped player = Game.LocalPlayer.Character;
            string animDict = EntryPoint.AnimationConfig.InteractDict.Value;
            string animName = EntryPoint.AnimationConfig.InteractName.Value;

            NativeFunction.Natives.REQUEST_ANIM_DICT(animDict);
            while (!NativeFunction.Natives.HAS_ANIM_DICT_LOADED<bool>(animDict)) GameFiber.Yield();

            player.Tasks.PlayAnimation(animDict, animName, 2.0f, AnimationFlags.None);
            GameFiber.Wait(1000);

            foreach (var id in new[] { "TRAUMABAG", "OXYGENBAG", "DEFIBRILLATOR" })
            {
                if (HasKit(id)) continue;
                string modelName = id == "TRAUMABAG" ? EntryPoint.PropConfig.TraumaBagModel :
                                   id == "OXYGENBAG" ? EntryPoint.PropConfig.OxygenBagModel : EntryPoint.PropConfig.DefibrillatorModel;

                Model m = new Model(modelName);
                m.LoadAndWait();
                if (m.IsValid) EquippedKits.Add(new EquippedKit { KitID = id, Prop = new Object(m, player.Position) });
                m.Dismiss();
            }
            ReAttachProps();
        }

        public static void StowKit(string kitID)
        {
            var kit = EquippedKits.FirstOrDefault(k => k.KitID == kitID);
            if (kit != null)
            {
                if (kit.Prop?.Exists() == true) kit.Prop.Delete();
                EquippedKits.Remove(kit);
            }
        }

        public static void StowAllKits()
        {
            EquippedKits.ForEach(k => { if (k.Prop?.Exists() == true) k.Prop.Delete(); });
            EquippedKits.Clear();
        }

        public static void ReAttachProps()
        {
            Ped player = Game.LocalPlayer.Character;
            var c = EntryPoint.OffsetConfig;

            foreach (var kit in EquippedKits)
            {
                if (kit.Prop?.Exists() != true) continue;
                int bone = 0; float x = 0, y = 0, z = 0, p = 0, r = 0, yaw = 0;

                if (kit.KitID == "TRAUMABAG") { bone = player.GetBoneIndex(ParseBone(c.TraumaAttachBone)); x = c.TraumaAttachX; y = c.TraumaAttachY; z = c.TraumaAttachZ; p = c.TraumaAttachPitch; r = c.TraumaAttachRoll; yaw = c.TraumaAttachYaw; }
                else if (kit.KitID == "OXYGENBAG") { bone = player.GetBoneIndex(ParseBone(c.OxygenAttachBone)); x = c.OxygenAttachX; y = c.OxygenAttachY; z = c.OxygenAttachZ; p = c.OxygenAttachPitch; r = c.OxygenAttachRoll; yaw = c.OxygenAttachYaw; }
                else if (kit.KitID == "DEFIBRILLATOR") { bone = player.GetBoneIndex(ParseBone(c.DefibAttachBone)); x = c.DefibAttachX; y = c.DefibAttachY; z = c.DefibAttachZ; p = c.DefibAttachPitch; r = c.DefibAttachRoll; yaw = c.DefibAttachYaw; }

                NativeFunction.Natives.ATTACH_ENTITY_TO_ENTITY(kit.Prop, player, bone, x, y, z, p, r, yaw, true, true, false, false, 2, true);
            }
        }

        public static void PlaceKitsOnGround(Ped targetPed = null)
        {
            if (EquippedKits.Count == 0) return;
            Vector3 basePos; float baseHeading;

            if (targetPed?.Exists() == true)
            {
                Vector3 head = targetPed.GetBonePosition(PedBoneId.Head);
                Vector3 pelvis = targetPed.GetBonePosition(PedBoneId.SpineRoot);
                basePos = head;
                baseHeading = (float)System.Math.Atan2(pelvis.Y - head.Y, pelvis.X - head.X) * 57.29578f;
            }
            else
            {
                basePos = Game.LocalPlayer.Character.GetOffsetPosition(new Vector3(0.5f, 0.5f, 0));
                baseHeading = Game.LocalPlayer.Character.Heading;
            }

            int idx = 0;
            foreach (var kit in EquippedKits.ToList())
            {
                kit.Prop?.Delete();
                string modelName = kit.KitID == "TRAUMABAG" ? EntryPoint.PropConfig.TraumaBagModel :
                                   kit.KitID == "OXYGENBAG" ? EntryPoint.PropConfig.OxygenBagModel : EntryPoint.PropConfig.DefibrillatorModel;

                Model m = new Model(modelName);
                m.LoadAndWait();
                if (!m.IsValid) { m.Dismiss(); continue; }

                Vector3 targetPos = basePos;
                if (targetPed?.Exists() == true)
                {
                    Vector3 up = targetPed.GetBonePosition(PedBoneId.Head) - targetPed.GetBonePosition(PedBoneId.SpineRoot); up.Normalize();
                    Vector3 right = Vector3.Cross(up, Vector3.WorldUp); right.Normalize();
                    if (kit.KitID == "TRAUMABAG") targetPos -= (right * 0.75f);
                    else if (kit.KitID == "DEFIBRILLATOR") targetPos += (right * 0.75f);
                    else if (kit.KitID == "OXYGENBAG") targetPos += (up * 0.55f);
                }
                else targetPos += new Vector3(idx * 0.6f - 0.6f, 0, 0);

                NativeFunction.Natives.GET_GROUND_Z_FOR_3D_COORD(targetPos.X, targetPos.Y, targetPos.Z + 1.0f, out float groundZ, false);
                var prop = new Rage.Object(m, new Vector3(targetPos.X, targetPos.Y, groundZ));
                if (prop?.Exists() == true)
                {
                    prop.Heading = baseHeading;
                    prop.IsPositionFrozen = true;
                    prop.IsCollisionEnabled = false;
                    PlacedKits.Add(new PlacedKit { Prop = prop, KitID = kit.KitID });
                }
                m.Dismiss();
                idx++;
            }
            EquippedKits.Clear();
        }

        private static PedBoneId ParseBone(string b) => b == "LeftHand" ? PedBoneId.LeftHand : b == "Back" ? PedBoneId.Spine3 : PedBoneId.RightHand;

        public static void PickupKit(Entity propEntity)
        {
            var kit = PlacedKits.FirstOrDefault(k => k.Prop == propEntity);
            if (kit != null)
            {
                kit.Prop?.Delete();
                PlacedKits.Remove(kit);
                EquipKit(kit.KitID);
            }
        }

        public static void StoreAllKits()
        {
            PlacedKits.ForEach(k => { if (k.Prop?.Exists() == true) { k.Prop.Detach(); k.Prop.Delete(); } });
            PlacedKits.Clear();
        }

        public static void Cleanup()
        {
            StowAllKits();
            StoreAllKits();
            ActiveTool = EmsTreatment.None;
        }
    }
}