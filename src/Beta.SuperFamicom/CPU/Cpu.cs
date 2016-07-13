using Beta.Platform.Messaging;
using Beta.Platform.Processors.RP65816;
using Beta.SuperFamicom.Messaging;

namespace Beta.SuperFamicom.CPU
{
    public partial class Cpu
        : Core
        , IConsumer<ClockSignal>
        , IConsumer<HBlankSignal>
        , IConsumer<VBlankSignal>
    {
        private readonly IProducer<ClockSignal> clock;
        private readonly SCpuState scpu;

        private bool old_nmi;
        private int t;

        public BusA Bus;
        public Dma Dma;

        public Cpu(IProducer<ClockSignal> clock, State state)
        {
            this.scpu = state.scpu;
            this.clock = clock;
        }

        protected override void InternalOperation()
        {
            clock.Produce(new ClockSignal(6));
        }

        protected override byte Read(byte bank, ushort address)
        {
            return Bus.Read(bank, address);
        }

        protected override void Write(byte bank, ushort address, byte data)
        {
            Bus.Write(bank, address, data);
        }

        public void Consume(ClockSignal e)
        {
            var amount = e.Cycles;

            if (Dma.mdma_count != 0 && --Dma.mdma_count == 0)
            {
                if (Dma.mdma_en != 0)
                {
                    int time = Dma.Run(t);
                    clock.Produce(new ClockSignal(amount - (time % amount)));

                    Dma.mdma_en = 0;
                }
            }

            t += e.Cycles;

            for (int i = 0; i < e.Cycles; i++)
            {
                Tick(e.Cycles);
            }
        }

        private void Tick(int amount)
        {
            scpu.dram_prescaler--;

            if (scpu.dram_prescaler == 0)
            {
                scpu.dram_prescaler = 8;
                scpu.dram_timer -= 8;

                if ((scpu.dram_timer & ~7) == 0)
                {
                    scpu.dram_timer += 1364;
                    clock.Produce(new ClockSignal(40));
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
