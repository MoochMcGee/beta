using Beta.Famicom.Messaging;
using Beta.Platform.Messaging;
using Beta.Platform.Processors.RP6502;

namespace Beta.Famicom.CPU
{
    public partial class R2A03
        : Core
        , IConsumer<ClockSignal>
        , IConsumer<IrqSignal>
        , IConsumer<VblSignal>
        , IConsumer<HalfFrameSignal>
        , IConsumer<QuadFrameSignal>
    {
        private readonly R2A03MemoryMap bus;
        private readonly R2A03State r2a03;
        private readonly IProducer<ClockSignal> clock;

        public R2A03(R2A03MemoryMap bus, State state, IProducer<ClockSignal> clock)
        {
            this.bus = bus;
            this.r2a03 = state.r2a03;
            this.clock = clock;
        }

        protected override void Read(int address, ref byte data)
        {
            clock.Produce(new ClockSignal(132));

            bus.Read(address, ref data);
        }

        protected override void Write(int address, byte data)
        {
            clock.Produce(new ClockSignal(132));

            bus.Write(address, data);
        }

        public override void Update()
        {
            base.Update();

            if (r2a03.dma_trigger)
            {
                r2a03.dma_trigger = false;

                var dma_src_address = r2a03.dma_segment << 8;
                var dma_dst_address = 0x2004;
                var dma_data = default(byte);

                for (var i = 0; i < 256; i++)
                {
                    dma_data = Read(dma_src_address);
                    Write(dma_dst_address, dma_data);

                    dma_src_address++;
                }
            }
        }

        public void Consume(IrqSignal e)
        {
            Irq(e.Value);
        }

        public void Consume(VblSignal e)
        {
            Nmi(e.Value);
        }

        public void Consume(ClockSignal e)
        {
            const HalfFrameSignal half = null;
            const QuadFrameSignal quad = null;

            if (r2a03.sequence_mode == 0)
            {
                switch (r2a03.sequence_time)
                {
                case     0: /*                         */ SequencerInterrupt(); break;
                case  7457: Consume(quad); /*          */ break;
                case 14913: Consume(quad); Consume(half); break;
                case 22371: Consume(quad); /*          */ break;
                case 29828: /*                         */ SequencerInterrupt(); break;
                case 29829: Consume(quad); Consume(half); SequencerInterrupt(); break;
                }

                if (++r2a03.sequence_time == 29830)
                {
                    r2a03.sequence_time = 0;
                }
            }
            else
            {
                switch (r2a03.sequence_time)
                {
                case  7457: Consume(quad); /*          */ break;
                case 14913: Consume(quad); Consume(half); break;
                case 22371: Consume(quad); /*          */ break;
                case 29829: /*                         */ break;
                case 37281: Consume(quad); Consume(half); break;
                }

                if (++r2a03.sequence_time == 37282)
                {
                    r2a03.sequence_time = 0;
                }
            }
        }

        private void SequencerInterrupt()
        {
            r2a03.sequence_irq_pending |= r2a03.sequence_irq_enabled;

            if (r2a03.sequence_irq_pending)
            {
                Irq(1);
            }
        }

        public void Consume(HalfFrameSignal e)
        {
            var sq1 = r2a03.sq1;
            var sq2 = r2a03.sq2;
            var tri = r2a03.tri;
            var noi = r2a03.noi;

            Duration.Tick(sq1.duration);
            Duration.Tick(sq2.duration);
            Duration.Tick(tri.duration);
            Duration.Tick(noi.duration);

            sq1.period = Sweep.Tick(sq1.sweep, sq1.period, ~sq1.period);
            sq2.period = Sweep.Tick(sq2.sweep, sq2.period, -sq2.period);
        }

        public void Consume(QuadFrameSignal e)
        {
            Envelope.Tick(r2a03.sq1.envelope);
            Envelope.Tick(r2a03.sq2.envelope);
            Envelope.Tick(r2a03.noi.envelope);

            var tri = r2a03.tri;
            if (tri.linear_counter_reload)
            {
                tri.linear_counter = tri.linear_counter_latch;
            }
            else
            {
                if (tri.linear_counter != 0)
                {
                    tri.linear_counter--;
                }
            }

            if (tri.linear_counter_control == false)
            {
                tri.linear_counter_reload = false;
            }
        }
    }
}
