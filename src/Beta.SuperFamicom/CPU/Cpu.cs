using Beta.Platform.Messaging;
using Beta.Platform.Processors.RP65816;
using Beta.SuperFamicom.Messaging;

namespace Beta.SuperFamicom.CPU
{
    public partial class Cpu : Core
    {
        private readonly IProducer<ClockSignal> clock;
        private readonly SCpuState scpu;

        private bool old_nmi;
        private byte open;
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
            var speed = GetSpeed(bank, address);
            clock.Produce(new ClockSignal(speed));

            Bus.Read(bank, address, ref open);

            return open;
        }

        protected override void Write(byte bank, ushort address, byte data)
        {
            var speed = GetSpeed(bank, address);
            clock.Produce(new ClockSignal(speed));

            Bus.Write(bank, address, open = data);
        }

        private int GetSpeed(byte bank, ushort address)
        {
            var addr = (bank << 16) | address;

            if ((addr & 0x408000) != 0)
            {
                return (addr & 0x800000) != 0 && scpu.fast_cart
                    ? 6
                    : 8
                    ;
            }

            if (((addr + 0x6000) & 0x4000) != 0) return 8;
            if (((addr - 0x4000) & 0x7E00) != 0) return 6;

            return 12;
        }

        public void Consume(ClockSignal e)
        {
            var amount = e.Cycles;

            if (scpu.mdma_count != 0 && --scpu.mdma_count == 0)
            {
                if (scpu.mdma_en != 0)
                {
                    int time = Dma.Run(t);
                    clock.Produce(new ClockSignal(amount - (time % amount)));

                    scpu.mdma_en = 0;
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
