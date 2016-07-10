using Beta.Platform.Core;
using Beta.SuperFamicom.CPU;
using Beta.SuperFamicom.PAD;
using Beta.SuperFamicom.PPU;
using Beta.SuperFamicom.SMP;

namespace Beta.SuperFamicom
{
    public class Driver : IDriver
    {
        public BusA Bus;
        public Dma Dma;
        public Cpu Cpu;
        public Ppu Ppu;
        public Smp Smp;
        public Pad Joypad1;
        public Pad Joypad2;

        public void Main()
        {
            while (true)
            {
                Cpu.Update();
            }
        }
    }
}
