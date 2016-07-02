using Beta.GameBoy.APU;
using Beta.GameBoy.Boards;
using Beta.GameBoy.CPU;
using Beta.GameBoy.PPU;
using Beta.Platform.Core;

namespace Beta.GameBoy
{
    public partial class Driver : IDriver
    {
        public Board Board;
        public Pad Pad;
        public Tma Tma;
        public Cpu Cpu;
        public Ppu Ppu;
        public Apu Apu;

        public void Main()
        {
            while (true)
            {
                Cpu.Update();
            }
        }
    }
}
