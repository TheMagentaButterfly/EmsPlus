using Rage;
using Rage.Native;
using EmsPlus.Managers;

namespace EmsPlus.Core
{
    public static class EmsService
    {
        public static bool IsOnDuty { get; private set; } = false;
        public static EmsStatus CurrentStatus { get; private set; } = EmsStatus.OffDuty;

        public static void ToggleDuty()
        {
            IsOnDuty = !IsOnDuty;
            Ped player = Game.LocalPlayer.Character;

            if (IsOnDuty)
            {
                Game.DisplayNotification(Localization.Get("NOTIF_ON_DUTY", "~b~EmsPlus:~w~ You are now ~g~On Duty~w~."));
                SetStatus(EmsStatus.Available);

                LoadoutManager.EquipLoadout();
                InventoryManager.RestockSupplies(false);

                player.RelationshipGroup = "COP";
                NativeFunction.Natives.SET_PED_AS_COP(player, true);

                Game.SetRelationshipBetweenRelationshipGroups("COP", "COP", Relationship.Companion);
                Game.SetRelationshipBetweenRelationshipGroups("COP", "FIREMAN", Relationship.Companion);
                Game.SetRelationshipBetweenRelationshipGroups("COP", "MEDIC", Relationship.Companion);
                Game.SetRelationshipBetweenRelationshipGroups("COP", "ARMY", Relationship.Companion);

                Game.SetRelationshipBetweenRelationshipGroups("FIREMAN", "COP", Relationship.Companion);
                Game.SetRelationshipBetweenRelationshipGroups("MEDIC", "COP", Relationship.Companion);
                Game.SetRelationshipBetweenRelationshipGroups("ARMY", "COP", Relationship.Companion);

                NativeFunction.Natives.SET_MAX_WANTED_LEVEL(0);
                NativeFunction.Natives.SET_DISPATCH_COPS_FOR_PLAYER(Game.LocalPlayer, false);
                Game.LocalPlayer.WantedLevel = 0;
                Game.LocalPlayer.IsIgnoredByPolice = true;

                API.Functions.InvokeDutyStateChanged(true);
            }
            else
            {
                Game.DisplayNotification(Localization.Get("NOTIF_ON_DUTY", "~b~EmsPlus:~w~ You are now ~r~Off Duty~w~."));
                SetStatus(EmsStatus.OffDuty);

                DialogueManager.Cleanup();

                player.RelationshipGroup = "PLAYER";
                NativeFunction.Natives.SET_PED_AS_COP(player, false);

                NativeFunction.Natives.SET_MAX_WANTED_LEVEL(5);
                NativeFunction.Natives.SET_DISPATCH_COPS_FOR_PLAYER(Game.LocalPlayer, true);
                Game.LocalPlayer.IsIgnoredByPolice = false;

                API.Functions.InvokeDutyStateChanged(false);
            }
        }

        private static void SetTaxiDisabled(bool disabled)
        {
            NativeFunction.Natives.SET_PED_CONFIG_FLAG(Game.LocalPlayer.Character, 224, disabled);
        }

        private static void SetEmsRelationships(bool onDuty)
        {
            Ped player = Game.LocalPlayer.Character;

            string[] emergencyGroups = { "COP", "FIREMAN", "MEDIC", "SECURITY_GUARD", "PRIVATE_SECURITY" };

            if (onDuty)
            {
                player.RelationshipGroup = "MEDIC";

                uint playerGroupHash = Game.GetHashKey("MEDIC");

                foreach (string groupName in emergencyGroups)
                {
                    uint targetGroupHash = Game.GetHashKey(groupName);

                    NativeFunction.Natives.SET_RELATIONSHIP_BETWEEN_GROUPS(0, targetGroupHash, playerGroupHash);
                    NativeFunction.Natives.SET_RELATIONSHIP_BETWEEN_GROUPS(0, playerGroupHash, targetGroupHash);
                }

                NativeFunction.Natives.SET_EVERYONE_IGNORE_PLAYER(Game.LocalPlayer, true);
            }
            else
            {
                player.RelationshipGroup = "PLAYER";
                NativeFunction.Natives.SET_EVERYONE_IGNORE_PLAYER(Game.LocalPlayer, false);

                uint playerGroupHash = Game.GetHashKey("PLAYER");
                foreach (string groupName in emergencyGroups)
                {
                    uint targetGroupHash = Game.GetHashKey(groupName);
                    NativeFunction.Natives.SET_RELATIONSHIP_BETWEEN_GROUPS(3, targetGroupHash, playerGroupHash);
                    NativeFunction.Natives.SET_RELATIONSHIP_BETWEEN_GROUPS(3, playerGroupHash, targetGroupHash);
                }
            }
        }

        public static void SetStatus(EmsStatus newStatus)
        {
            CurrentStatus = newStatus;
            string statusNameKey = newStatus.ToString().ToUpperInvariant();
            string localizedStatus = Localization.Get(statusNameKey);

            Game.DisplayNotification(Localization.GetFormat("NOTIF_STATUS_UPDATE", "~b~Status Update:~w~ {0}", localizedStatus));
        }
    }
}