using Rage;

namespace EmsPlus.Medical
{
    public class Bystander
    {
        public Ped Character { get; }

        public Bystander(Ped character)
        {
            Character = character;
        }
    }
}