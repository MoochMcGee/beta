using Beta.Platform.Messaging;
using Beta.Platform.Processors.RP65816;
using Beta.SuperFamicom.Messaging;

namespace Beta.SuperFamicom.CPU
{
    public partial class Cpu
        : Core
        , IConsumer<HBlankSignal>
        , IConsumer<VBlankSignal>
    {
        private readonly SCpuState scpu;

        public Cpu(State state, IBus bus)
            : base(bus)
        {
            this.scpu = state.scpu;
        }

        public void Consume(HBlankSignal e)
        {
            scpu.in_hblank = e.HBlank;
        }

        public void Consume(VBlankSignal e)
        {
            scpu.in_vblank = e.VBlank;

            if (scpu.in_vblank && (scpu.reg4200 & 0x80) != 0)
            {
                Nmi();
            }
        }
    }
}
