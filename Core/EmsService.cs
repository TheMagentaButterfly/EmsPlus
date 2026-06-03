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
                SetStatus(EmsStatus.Available);

                LoadoutManager.EquipLoadout();
                InventoryManager.RestockSupplies(false);

                player.RelationshipGroup = "MEDIC";

                NativeFunction.Natives.SET_PED_AS_COP(player, true);

                SetEmsRelationships(true);
                SetTaxiDisabled(true);

                NativeFunction.Natives.SET_MAX_WANTED_LEVEL(0);
                NativeFunction.Natives.SET_DISPATCH_COPS_FOR_PLAYER(Game.LocalPlayer, false);
                Game.LocalPlayer.WantedLevel = 0;
                Game.LocalPlayer.IsIgnoredByPolice = true;

                API.Functions.InvokeDutyStateChanged(true);
            }
            else
            {
                SetStatus(EmsStatus.OffDuty);

                DialogueManager.Cleanup();

                player.RelationshipGroup = "PLAYER";
                NativeFunction.Natives.SET_PED_AS_COP(player, false);

                SetEmsRelationships(false);
                SetTaxiDisabled(false);

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
            string[] emergencyGroups = { "COP", "FIREMAN", "MEDIC", "ARMY", "SECURITY_GUARD", "PRIVATE_SECURITY", "PLAYER" };

            if (onDuty)
            {
                foreach (string groupA in emergencyGroups)
                {
                    foreach (string groupB in emergencyGroups)
                    {
                        Game.SetRelationshipBetweenRelationshipGroups(groupA, groupB, Relationship.Respect);
                    }
                }

                NativeFunction.Natives.SET_EVERYONE_IGNORE_PLAYER(Game.LocalPlayer, true);
                NativeFunction.Natives.SET_POLICE_IGNORE_PLAYER(Game.LocalPlayer, true);
            }
            else
            {
                NativeFunction.Natives.SET_EVERYONE_IGNORE_PLAYER(Game.LocalPlayer, false);
                NativeFunction.Natives.SET_POLICE_IGNORE_PLAYER(Game.LocalPlayer, false);

                foreach (string group in emergencyGroups)
                {
                    Game.SetRelationshipBetweenRelationshipGroups("PLAYER", group, Relationship.Neutral);
                    Game.SetRelationshipBetweenRelationshipGroups(group, "PLAYER", Relationship.Neutral);
                }
            }
        }

        public static void SetStatus(EmsStatus newStatus)
        {
            CurrentStatus = newStatus;

            string defaultText = newStatus.ToString();
            switch (newStatus)
            {
                case EmsStatus.AvailableAtStation: defaultText = "Available at Station"; break;
                case EmsStatus.OnScene: defaultText = "On Scene"; break;
                case EmsStatus.RequestToSpeak: defaultText = "Request to speak"; break;
                case EmsStatus.OffDuty: defaultText = "Off Duty"; break;
                case EmsStatus.AtDestination: defaultText = "At Destination"; break;
                case EmsStatus.UrgentRequestToSpeak: defaultText = "Urgent request to speak"; break;
            }

            string statusNameKey = $"STATUS_{newStatus.ToString().ToUpperInvariant()}";

            string localizedStatus = Localization.Get(statusNameKey, defaultText);

            Game.DisplayNotification(Localization.GetFormat("NOTIF_STATUS_UPDATE", "~b~Status Update:~w~ {0}", localizedStatus));
        }
    }
}