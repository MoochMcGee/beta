using Beta.GameBoy.APU;
using Beta.GameBoy.Boards;
using Beta.GameBoy.CPU;
using Beta.GameBoy.Messaging;
using Beta.GameBoy.PPU;
using Beta.Platform.Core;
using Beta.Platform.Messaging;

namespace Beta.GameBoy
{
    public partial class Driver : IDriver, IConsumer<FrameSignal>
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

        public void Consume(FrameSignal e)
        {
            Pad.AutofireState = !Pad.AutofireState;

            Pad.Update();
        }
    }
}
