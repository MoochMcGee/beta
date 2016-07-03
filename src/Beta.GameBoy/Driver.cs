using Beta.GameBoy.CPU;
using Beta.Platform.Core;

namespace Beta.GameBoy
{
    public partial class Driver : IDriver
    {
        private readonly Cpu cpu;

        public Driver(Cpu cpu)
        {
            this.cpu = cpu;
        }

        public void Main()
        {
            while (true)
            {
                cpu.Update();
            }
        }
    }
}
