using Beta.Platform;
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

        public Driver(BusA bus, Cpu cpu, Dma dma, Ppu ppu, Smp smp)
        {
            this.Bus = bus;
            this.Cpu = cpu;
            this.Dma = dma;
            this.Ppu = ppu;
            this.Smp = smp;
        }

        public void main()
        {
            while (true)
            {
                Cpu.Update();
            }
        }
    }
}
