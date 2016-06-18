using Beta.Platform.Processors.RP65816;

namespace Beta.SuperFamicom.CPU
{
    public partial class Cpu : Core
    {
        public Cpu(IBus bus)
            : base(bus)
        {
        }

        public void EnterHBlank() { }

        public void EnterVBlank() { }

        public void LeaveHBlank() { }

        public void LeaveVBlank() { }
    }
}
