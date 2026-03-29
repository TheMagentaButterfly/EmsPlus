using EmsPlus.Core;
using EmsPlus.Managers;
using Rage;
using Rage.Attributes;
using System.Threading;

namespace EmsPlus
{
    public static class Commands
    {
        [ConsoleCommand(Name = "ForceDuty", Description = "Go on/off duty as a paramedic.")]
        public static void Command_ForceDuty()
        {
            if (!EmsService.IsOnDuty)
            {
                StationManager.SetDutyAtClosest();

                Game.Console.Print("EmsPlus: Going ON Duty...");
                Managers.LoadoutManager.EquipLoadout();
                EntryPoint.StartPluginLogic();
                EmsService.ToggleDuty();
            }
            else
            {
                Game.Console.Print("EmsPlus: Going OFF Duty...");
                EmsService.ToggleDuty();
                EntryPoint.StopPluginLogic();
                Game.LocalPlayer.Character.Inventory.Weapons.Clear();
            }
        }

        [ConsoleCommand(Name = "ForceRandomCallout", Description = "Force a random medical callout from the entire pool.")]
        public static void Command_ForceCallout_Random()
        {
            if (!EmsService.IsOnDuty)
            {
                Game.Console.Print("[EmsPlus] You must be On Duty to receive callouts.");
                return;
            }

            CalloutManager.StartRandomCallout();
        }

        [ConsoleCommand(Name = "StartCallout", Description = "Start a specific callout by name (e.g. StartCallout AnimalAttack).")]
        public static void Command_StartCallout(string calloutName)
        {
            if (!EmsService.IsOnDuty) return;
            CalloutManager.ForceCallout(calloutName);
        }

        [ConsoleCommand(Name = "EndCallout", Description = "Finish the current callout.")]
        public static void Command_EndCallout()
        {
            if (!EmsService.IsOnDuty) return;
            CalloutManager.EndCurrent();
        }

        [ConsoleCommand(Name = "SetStatus", Description = "Set your EMS status.")]
        public static void Command_SetStatus(EmsStatus status)
        {
            if (!EmsService.IsOnDuty)
            {
                Game.Console.Print("You must be on duty to set status.");
                return;
            }
            EmsService.SetStatus(status);
        }

        [ConsoleCommand(Name = "ReloadEmsConfigs", Description = "Quickly reloads all EmsPlus .ini and .xml configuration files.")]
        public static void Command_ReloadConfigs()
        {
            EntryPoint.ReloadAllConfigs();
        }

        [ConsoleCommand(Name = "CopyPosition", Description = "Copies your current position to the clipboard.")]
        public static void Command_GetPos()
        {
            Vector3 pos = Game.LocalPlayer.Character.Position;
            float heading = Game.LocalPlayer.Character.Heading;

            string xmlFormat = $"<Location x=\"{pos.X:F3}\" y=\"{pos.Y:F3}\" z=\"{pos.Z:F3}\" name=\"New Location\"/>";

            Thread clipboardThread = new Thread(() =>
            {
                try
                {
                    System.Windows.Forms.Clipboard.SetText(xmlFormat);
                }
                catch (System.Exception ex)
                {
                    Game.Console.Print($"[EmsPlus] Clipboard Error: {ex.Message}");
                }
            });

            clipboardThread.SetApartmentState(ApartmentState.STA);
            clipboardThread.Start();
            clipboardThread.Join();

            Game.Console.Print($"[EmsPlus] Position: {pos}");
            Game.DisplayNotification(Localization.Get("NOTIF_POSITION_COPIED", pos));
        }
    }
}