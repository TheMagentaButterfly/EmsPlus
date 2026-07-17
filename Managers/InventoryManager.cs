using EmsPlus.Medical;
using Rage;
using Rage.Native;
using System.Collections.Generic;
using System.Linq;

namespace EmsPlus.Managers
{
    public class PlacedKit
    {
        public Object Prop { get; set; }
        public string KitID { get; set; }
    }

    public class EquippedKit
    {
        public string KitID { get; set; }
        public Object Prop { get; set; }
    }

    public static class InventoryManager
    {
        public static List<EquippedKit> EquippedKits { get; private set; } = new List<EquippedKit>();
        public static List<PlacedKit> PlacedKits { get; private set; } = new List<PlacedKit>();
        public static EmsTreatment ActiveTool { get; set; } = EmsTreatment.None;

        public static void ClearActiveTool() => ActiveTool = (EmsTreatment)999;
        public static Dictionary<EmsTreatment, int> CurrentSupplies { get; private set; } = new Dictionary<EmsTreatment, int>();

        static InventoryManager()
        {
            RestockSupplies(false);
        }

        public static void RestockSupplies(bool notify = true)
        {
            CurrentSupplies.Clear();
            // Trauma
            CurrentSupplies[EmsTreatment.Bandage] = 4;
            CurrentSupplies[EmsTreatment.WoundPacking] = 2;
            CurrentSupplies[EmsTreatment.Tourniquet] = 1;
            CurrentSupplies[EmsTreatment.JunctionalTourniquet] = 1;
            CurrentSupplies[EmsTreatment.IcePack] = 2;
            CurrentSupplies[EmsTreatment.StabiliseObject] = 1;
            // Immobilize
            CurrentSupplies[EmsTreatment.Splint] = 1;
            CurrentSupplies[EmsTreatment.TractionSplint] = 1;
            CurrentSupplies[EmsTreatment.PelvicBinder] = 1;
            CurrentSupplies[EmsTreatment.CervicalCollar] = 1;
            // IV / Meds
            CurrentSupplies[EmsTreatment.IVAccess] = 4;
            CurrentSupplies[EmsTreatment.SalineBag] = 2;
            CurrentSupplies[EmsTreatment.Adrenaline] = 2;
            CurrentSupplies[EmsTreatment.Naloxone] = 2;
            CurrentSupplies[EmsTreatment.Glucose] = 2;
            // Airway / Chest
            CurrentSupplies[EmsTreatment.ChestSeal] = 2;
            CurrentSupplies[EmsTreatment.NeedleDecomp] = 2;
            // Specialized
            CurrentSupplies[EmsTreatment.WetDressing] = 1;
            CurrentSupplies[EmsTreatment.BurnDressing] = 1;
            CurrentSupplies[EmsTreatment.Irrigation] = 1;
            CurrentSupplies[EmsTreatment.EyePatch] = 2;
            CurrentSupplies[EmsTreatment.EyeShield] = 2;

            if (notify)
            {
                Game.DisplayNotification(Localization.Get("NOTIF_MEDICALBAGS_RESTOCKED", "~w~Medical bags ~g~restocked~w~."));
            }
        }

        public static bool HasSupply(EmsTreatment treatment)
        {
            if (AmbulanceManager.IsPlayerInRearCabin) return true;
            if (!CurrentSupplies.ContainsKey(treatment)) return true;
            return CurrentSupplies[treatment] > 0;
        }

        public static void ConsumeSupply(EmsTreatment treatment)
        {
            if (AmbulanceManager.IsPlayerInRearCabin) return;
            if (CurrentSupplies.ContainsKey(treatment) && CurrentSupplies[treatment] > 0)
            {
                CurrentSupplies[treatment]--;
            }
        }

        public static bool IsKitAvailable(string kitID, Vector3 position, float range = 5.0f)
        {
            if (kitID == "NONE") return true;
            if (AmbulanceManager.IsPlayerInRearCabin) return true;
            if (HasKit(kitID)) return true;

            return PlacedKits.Any(k => k.KitID == kitID && k.Prop != null && k.Prop.Exists() && k.Prop.DistanceTo(position) <= range);
        }

        public static bool HasKit(string kitID)
        {
            return EquippedKits.Any(k => k.KitID == kitID);
        }

        public static void EquipKit(string kitID)
        {
            if (HasKit(kitID))
            {
                StowKit(kitID);
                return;
            }

            Ped player = Game.LocalPlayer.Character;
            string animDict = EntryPoint.AnimationConfig.InteractDict.Value;
            string animName = EntryPoint.AnimationConfig.InteractName.Value;

            NativeFunction.Natives.REQUEST_ANIM_DICT(animDict);
            while (!NativeFunction.Natives.HAS_ANIM_DICT_LOADED<bool>(animDict)) GameFiber.Yield();

            player.Tasks.PlayAnimation(animDict, animName, 2.0f, AnimationFlags.None);
            GameFiber.Wait(1000);

            string modelName = "";
            if (kitID == "TRAUMABAG") modelName = EntryPoint.PropConfig.TraumaBagModel;
            else if (kitID == "OXYGENBAG") modelName = EntryPoint.PropConfig.OxygenBagModel;
            else if (kitID == "DEFIBRILLATOR") modelName = EntryPoint.PropConfig.DefibrillatorModel;

            if (string.IsNullOrEmpty(modelName)) return;

            Model m = new Model(modelName);
            m.LoadAndWait();
            if (!m.IsValid) return;

            Object newProp = new Object(m, player.Position);
            m.Dismiss();

            EquippedKits.Add(new EquippedKit { KitID = kitID, Prop = newProp });
            ReAttachProps();
        }

        public static void StowKit(string kitID)
        {
            var kit = EquippedKits.FirstOrDefault(k => k.KitID == kitID);
            if (kit != null)
            {
                if (kit.Prop != null && kit.Prop.Exists()) kit.Prop.Delete();
                EquippedKits.Remove(kit);
            }
        }

        public static void StowAllKits()
        {
            foreach (var kit in EquippedKits)
            {
                if (kit.Prop != null && kit.Prop.Exists()) kit.Prop.Delete();
            }
            EquippedKits.Clear();
        }

        public static void ReAttachProps()
        {
            Ped player = Game.LocalPlayer.Character;
            var c = EntryPoint.OffsetConfig;

            foreach (var kit in EquippedKits)
            {
                if (kit.Prop == null || !kit.Prop.Exists()) continue;

                int boneIndex = 0;
                float x = 0, y = 0, z = 0, p = 0, r = 0, yaw = 0;

                if (kit.KitID == "TRAUMABAG")
                {
                    boneIndex = player.GetBoneIndex(ParseBone(c.TraumaAttachBone));
                    x = c.TraumaAttachX; y = c.TraumaAttachY; z = c.TraumaAttachZ;
                    p = c.TraumaAttachPitch; r = c.TraumaAttachRoll; yaw = c.TraumaAttachYaw;
                }
                else if (kit.KitID == "OXYGENBAG")
                {
                    boneIndex = player.GetBoneIndex(ParseBone(c.OxygenAttachBone));
                    x = c.OxygenAttachX; y = c.OxygenAttachY; z = c.OxygenAttachZ;
                    p = c.OxygenAttachPitch; r = c.OxygenAttachRoll; yaw = c.OxygenAttachYaw;
                }
                else if (kit.KitID == "DEFIBRILLATOR")
                {
                    boneIndex = player.GetBoneIndex(ParseBone(c.DefibAttachBone));
                    x = c.DefibAttachX; y = c.DefibAttachY; z = c.DefibAttachZ;
                    p = c.DefibAttachPitch; r = c.DefibAttachRoll; yaw = c.DefibAttachYaw;
                }

                NativeFunction.Natives.ATTACH_ENTITY_TO_ENTITY(kit.Prop, player, boneIndex, x, y, z, p, r, yaw, true, true, false, false, 2, true);
            }
        }

        public static void PlaceKitsOnGround(Ped targetPed = null)
        {
            if (EquippedKits.Count == 0) return;

            Vector3 basePos;
            float baseHeading;

            if (targetPed != null && targetPed.Exists())
            {
                Vector3 headPos = targetPed.GetBonePosition(PedBoneId.Head);
                Vector3 pelvisPos = targetPed.GetBonePosition(PedBoneId.SpineRoot);
                basePos = headPos;
                baseHeading = (float)System.Math.Atan2(pelvisPos.Y - headPos.Y, pelvisPos.X - headPos.X) * 57.29578f;
            }
            else
            {
                basePos = Game.LocalPlayer.Character.GetOffsetPosition(new Vector3(0.5f, 0.5f, 0));
                baseHeading = Game.LocalPlayer.Character.Heading;
            }

            int index = 0;
            foreach (var kit in EquippedKits.ToList())
            {
                if (kit.Prop != null && kit.Prop.Exists()) kit.Prop.Delete();

                string modelName = "";
                if (kit.KitID == "TRAUMABAG") modelName = EntryPoint.PropConfig.TraumaBagModel;
                else if (kit.KitID == "OXYGENBAG") modelName = EntryPoint.PropConfig.OxygenBagModel;
                else if (kit.KitID == "DEFIBRILLATOR") modelName = EntryPoint.PropConfig.DefibrillatorModel;

                if (string.IsNullOrEmpty(modelName)) continue;

                Model m = new Model(modelName);
                m.LoadAndWait();
                if (!m.IsValid) { m.Dismiss(); continue; }

                Vector3 targetPos = basePos;

                if (targetPed != null && targetPed.Exists())
                {
                    Vector3 headPos = targetPed.GetBonePosition(PedBoneId.Head);
                    Vector3 pelvisPos = targetPed.GetBonePosition(PedBoneId.SpineRoot);
                    Vector3 bodyUpDir = (headPos - pelvisPos); bodyUpDir.Normalize();
                    Vector3 bodyRightDir = Vector3.Cross(bodyUpDir, Vector3.WorldUp); bodyRightDir.Normalize();

                    if (kit.KitID == "TRAUMABAG") targetPos = headPos - (bodyRightDir * 0.75f);
                    else if (kit.KitID == "DEFIBRILLATOR") targetPos = headPos + (bodyRightDir * 0.75f);
                    else if (kit.KitID == "OXYGENBAG") targetPos = headPos + (bodyUpDir * 0.55f);
                }
                else
                {
                    targetPos = basePos + new Vector3(index * 0.6f - 0.6f, 0, 0);
                }

                float groundZ;
                NativeFunction.Natives.GET_GROUND_Z_FOR_3D_COORD(targetPos.X, targetPos.Y, targetPos.Z + 1.0f, out groundZ, false);

                Rage.Object newProp = new Rage.Object(m, new Vector3(targetPos.X, targetPos.Y, groundZ));

                if (newProp != null && newProp.Exists())
                {
                    newProp.Heading = baseHeading;
                    newProp.IsPositionFrozen = true;
                    newProp.IsCollisionEnabled = false;

                    PlacedKits.Add(new PlacedKit { Prop = newProp, KitID = kit.KitID });
                }

                m.Dismiss();
                index++;
            }

            EquippedKits.Clear();
        }

        private static PedBoneId ParseBone(string boneName)
        {
            if (boneName == "LeftHand") return PedBoneId.LeftHand;
            if (boneName == "Back") return PedBoneId.Spine3;
            return PedBoneId.RightHand;
        }

        public static void PickupKit(Entity propEntity)
        {
            var kitEntry = PlacedKits.FirstOrDefault(k => k.Prop == propEntity);
            if (kitEntry != null)
            {
                if (kitEntry.Prop != null && kitEntry.Prop.Exists()) kitEntry.Prop.Delete();
                PlacedKits.Remove(kitEntry);
                EquipKit(kitEntry.KitID);
            }
        }

        public static void PickupKitFromGround()
        {
            Vector3 playerPos = Game.LocalPlayer.Character.Position;
            var kitEntry = PlacedKits.OrderBy(k => k.Prop.DistanceTo(playerPos)).FirstOrDefault();
            if (kitEntry != null && kitEntry.Prop.DistanceTo(playerPos) < 2.5f)
            {
                PickupKit(kitEntry.Prop);
            }
        }

        public static void StoreAllKits()
        {
            if (PlacedKits == null || PlacedKits.Count == 0) return;

            for (int i = PlacedKits.Count - 1; i >= 0; i--)
            {
                var kit = PlacedKits[i];
                if (kit.Prop != null && kit.Prop.Exists())
                {
                    try
                    {
                        kit.Prop.Detach();
                        kit.Prop.Delete();
                    }
                    catch { }
                }
            }

            PlacedKits.Clear();
        }

        public static void Cleanup()
        {
            StowAllKits();
            foreach (var k in PlacedKits)
            {
                if (k.Prop != null && k.Prop.Exists()) k.Prop.Delete();
            }
            PlacedKits.Clear();
            StoreAllKits();
            ActiveTool = EmsTreatment.None;
        }
    }
}