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
        private readonly BusA bus_a;

        private bool old_nmi;

        public Cpu(State state, BusA bus)
            : base(bus)
        {
            this.scpu = state.scpu;
            this.bus_a = bus;
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
            scpu.dram_prescaler--;

            if (scpu.dram_prescaler == 0)
            {
                scpu.dram_prescaler = 8;
                scpu.dram_timer -= 8;

                if ((scpu.dram_timer & ~7) == 0)
                {
                    scpu.dram_timer += 1364;
                    bus_a.AddCycles(40);
                }
            }

            scpu.time_prescaler--;

            if (scpu.time_prescaler == 0)
            {
                scpu.time_prescaler = 4;
                scpu.h++;

                if (scpu.h == 341)
                {
                    scpu.h = 0;
                    scpu.v++;

                    if (scpu.v == 262)
                    {
                        scpu.v = 0;
                    }
                }

                var h_coincidence = scpu.h == scpu.h_target;
                var v_coincidence = scpu.v == scpu.v_target;
                var type = (scpu.reg4200 >> 4) & 3;

                if ((type == 1 && h_coincidence) ||
                    (type == 2 && v_coincidence) ||
                    (type == 3 && h_coincidence && v_coincidence))
                {
                    scpu.timer_coincidence = true;
                    Irq();
                }
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
