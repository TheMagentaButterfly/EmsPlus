using EmsPlus.Medical.Conditions;
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

    public static class InventoryManager
    {
        public static string CurrentKitID { get; private set; } = "NONE";
        public static List<PlacedKit> PlacedKits { get; private set; } = new List<PlacedKit>();
        public static EmsTreatment ActiveTool { get; set; } = EmsTreatment.None;
        public static void ClearActiveTool() => ActiveTool = (EmsTreatment)999;
        private static Object _equippedProp;
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
            //CurrentSupplies[EmsTreatment.IVAccess] = 4;
            //CurrentSupplies[EmsTreatment.SalineBag] = 2;
            //CurrentSupplies[EmsTreatment.Adrenaline] = 2;
            //CurrentSupplies[EmsTreatment.Naloxone] = 2;
            //CurrentSupplies[EmsTreatment.Glucose] = 2;
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
                Game.DisplayNotification(Localization.Get("NOTIF_MEDICALBAGS_RESTOCKED"));
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
            if (CurrentKitID == kitID) return true;

            return PlacedKits.Any(k => k.KitID == kitID && k.Prop != null && k.Prop.Exists() && k.Prop.DistanceTo(position) <= range);
        }

        public static void EquipKit(string kitID)
        {
            if (PlacedKits.Any(k => k.KitID == kitID))
            {
                return;
            }
            Ped player = Game.LocalPlayer.Character;
            string animDict = EntryPoint.AnimationConfig.InteractDict.Value;
            string animName = EntryPoint.AnimationConfig.InteractName.Value;

            NativeFunction.Natives.REQUEST_ANIM_DICT(animDict);
            while (!NativeFunction.Natives.HAS_ANIM_DICT_LOADED<bool>(animDict)) GameFiber.Yield();

            player.Tasks.PlayAnimation(animDict, animName, 2.0f, AnimationFlags.None);
            GameFiber.Wait(1000);

            var kitDef = EntryPoint.KitConfig.Definitions.Find(k => k.ID.Equals(kitID, System.StringComparison.OrdinalIgnoreCase));
            if (kitDef == null) return;

            if (CurrentKitID == kitID) { StowKit(); return; }
            StowKit();

            CurrentKitID = kitID;
            SpawnAndAttach(kitDef.Model);

            string localizedKitName = Localization.Get($"KIT_NAME_{kitID.ToUpperInvariant()}");
        }

        public static void StowKit()
        {
            if (_equippedProp != null && _equippedProp.Exists())
            {
                _equippedProp.Delete();
            }
            _equippedProp = null;
            CurrentKitID = "NONE";
        }

        public static bool HasKit(string kitID)
        {
            return CurrentKitID == kitID;
        }

        public static void PlaceKitOnGround(Ped targetPed = null)
        {
            if (CurrentKitID == "NONE") return;

            var kitDef = EntryPoint.KitConfig.Definitions.Find(k => k.ID == CurrentKitID);
            if (kitDef == null) return;

            Vector3 targetPos;
            float heading;

            if (targetPed != null && targetPed.Exists())
            {
                Vector3 headPos = targetPed.GetBonePosition(PedBoneId.Head);
                Vector3 pelvisPos = targetPed.GetBonePosition(PedBoneId.SpineRoot);

                Vector3 bodyUpDir = (headPos - pelvisPos);
                bodyUpDir.Normalize();

                Vector3 bodyRightDir = Vector3.Cross(bodyUpDir, Vector3.WorldUp);
                bodyRightDir.Normalize();

                if (CurrentKitID == "TRAUMABAG") targetPos = headPos - (bodyRightDir * 0.75f);
                else if (CurrentKitID == "DEFIBRILLATOR") targetPos = headPos + (bodyRightDir * 0.75f);
                else if (CurrentKitID == "OXYGENBAG") targetPos = headPos + (bodyUpDir * 0.55f);
                else targetPos = pelvisPos + (bodyUpDir * 0.3f) + (bodyRightDir * 0.8f);

                heading = (float)System.Math.Atan2(pelvisPos.Y - targetPos.Y, pelvisPos.X - targetPos.X) * 57.29578f;
            }
            else
            {
                targetPos = Game.LocalPlayer.Character.GetOffsetPosition(new Vector3(0.5f, 0.5f, 0));
                heading = Game.LocalPlayer.Character.Heading;
            }

            float groundZ;
            NativeFunction.Natives.GET_GROUND_Z_FOR_3D_COORD(targetPos.X, targetPos.Y, targetPos.Z + 1.0f, out groundZ, false);

            if (_equippedProp != null && _equippedProp.Exists()) _equippedProp.Delete();
            _equippedProp = null;

            Model m = new Model(kitDef.Model);
            m.LoadAndWait();
            Rage.Object newProp = new Rage.Object(m, new Vector3(targetPos.X, targetPos.Y, groundZ));
            newProp.Heading = heading;
            newProp.IsPositionFrozen = true;
            newProp.IsCollisionEnabled = false;
            m.Dismiss();

            PlacedKits.Add(new PlacedKit { Prop = newProp, KitID = CurrentKitID });
            CurrentKitID = "NONE";
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
            Game.Console.Print("[EmsPlus] Equipment stored in ambulance.");
        }

        public static void Cleanup()
        {
            StowKit();
            foreach (var k in PlacedKits)
            {
                if (k.Prop != null && k.Prop.Exists()) k.Prop.Delete();
            }
            PlacedKits.Clear();
            StoreAllKits();
            ActiveTool = EmsTreatment.None;
        }

        private static void SpawnAndAttach(string modelName)
        {
            Ped player = Game.LocalPlayer.Character;
            Model m = new Model(modelName);
            m.LoadAndWait();
            _equippedProp = new Rage.Object(m, player.Position);
            ReAttachProp();
            m.Dismiss();
        }

        public static void ReAttachProp()
        {
            if (_equippedProp == null || !_equippedProp.Exists()) return;

            Ped player = Game.LocalPlayer.Character;
            int boneIndex = player.GetBoneIndex(PedBoneId.RightHand);
            var c = EntryPoint.OffsetConfig;

            float x = 0, y = 0, z = 0, p = 0, r = 0, yaw = 0;

            if (CurrentKitID == "TRAUMABAG") { x = c.TraumaAttachX; y = c.TraumaAttachY; z = c.TraumaAttachZ; p = c.TraumaAttachPitch; r = c.TraumaAttachRoll; yaw = c.TraumaAttachYaw; }
            else if (CurrentKitID == "OXYGENBAG") { x = c.OxygenAttachX; y = c.OxygenAttachY; z = c.OxygenAttachZ; p = c.OxygenAttachPitch; r = c.OxygenAttachRoll; yaw = c.OxygenAttachYaw; }
            else if (CurrentKitID == "DEFIBRILLATOR") { x = c.DefibAttachX; y = c.DefibAttachY; z = c.DefibAttachZ; p = c.DefibAttachPitch; r = c.DefibAttachRoll; yaw = c.DefibAttachYaw; }

            NativeFunction.Natives.ATTACH_ENTITY_TO_ENTITY(_equippedProp, player, boneIndex, x, y, z, p, r, yaw, true, true, false, false, 2, true);
        }
    }
}