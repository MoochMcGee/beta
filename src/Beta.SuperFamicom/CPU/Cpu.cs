using Beta.Platform.Messaging;
using Beta.Platform.Processors.RP65816;
using Beta.SuperFamicom.Messaging;

namespace Beta.SuperFamicom.CPU
{
    public partial class Cpu
        : Core
        , IConsumer<HBlankSignal>
        , IConsumer<VBlankSignal>
        , IConsumer<ClockSignal>
    {
        private readonly SCpuState scpu;
        private bool old_nmi;

        public Cpu(State state, IBus bus)
            : base(bus)
        {
            this.scpu = state.scpu;
        }

        public void Consume(ClockSignal e)
        {
            for (int i = 0; i < e.Cycles; i++)
            {
                Tick();
            }
        }

        private void Tick()
        {
            scpu.h++;

            if (scpu.h == 1364)
            {
                scpu.h = 0;
                scpu.v++;

                if (scpu.v == 262)
                {
                    scpu.v = 0;
                }
            }

            var h_coincidence = (scpu.h / 4) == scpu.h_target;
            var v_coincidence = (scpu.v / 1) == scpu.v_target;
            var type = (scpu.reg4200 >> 4) & 3;

            if ((type == 1 && v_coincidence) ||
                (type == 2 && h_coincidence) ||
                (type == 3 && h_coincidence && v_coincidence))
            {
                scpu.timer_coincidence = true;
                Irq();
            }
        }

        public void Consume(HBlankSignal e)
        {
            scpu.in_hblank = e.HBlank;
        }

        public void Consume(VBlankSignal e)
        {
            scpu.in_vblank = e.VBlank;
            NmiWrapper((scpu.reg4200 & 0x80) != 0);
        }

        public void NmiWrapper(bool enable)
        {
            var new_nmi = (scpu.in_vblank && enable);
            if (new_nmi && !old_nmi)
            {
                Nmi();
            }

            old_nmi = new_nmi;
        }
    }
}
