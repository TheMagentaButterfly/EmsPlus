using EmsPlus.Medical;
using System.Collections.Generic;

namespace EmsPlus.Core
{
    public static class GameState
    {
        public static List<Patient> ActivePatients { get; set; } = new List<Patient>();
        private static Patient _currentPatient;
        public static Patient CurrentPatient
        {
            get => _currentPatient;
            set
            {
                _currentPatient = value;
                if (value != null && !ActivePatients.Contains(value))
                {
                    ActivePatients.Add(value);
                }
            }
        }

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