using Rage;

namespace EmsPlus.Medical.Frameworks
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