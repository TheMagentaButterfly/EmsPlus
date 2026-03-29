using EmsPlus.Medical;
using System.Collections.Generic;

namespace EmsPlus.Core
{
    public static class GameState
    {
        public static List<Patient> ActivePatients { get; set; } = new List<Patient>();

        public static Patient CurrentPatient { get; set; }

        public static Bystander CurrentBystander { get; set; }
        public static bool IsPlayerBusy { get; set; } = false;
        public static bool SuppressPrompts { get; set; } = false;

        public static void Clear()
        {
            try
            {
                if (CurrentPatient?.Character != null && CurrentPatient.Character.Exists())
                    CurrentPatient.Character.Delete();

                if (CurrentBystander?.Character != null && CurrentBystander.Character.Exists())
                    CurrentBystander.Character.Delete();
            }
            catch { }

            CurrentPatient = null;
            CurrentBystander = null;
            IsPlayerBusy = false;
        }
    }
}