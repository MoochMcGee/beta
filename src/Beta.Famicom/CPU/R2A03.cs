using Beta.Famicom.Messaging;
using Beta.Platform.Audio;
using Beta.Platform.Messaging;
using Beta.Platform.Processors.RP6502;

namespace Beta.Famicom.CPU
{
    public partial class R2A03
        : Core
        , IConsumer<ClockSignal>
        , IConsumer<IrqSignal>
        , IConsumer<VblSignal>
    {
        private readonly R2A03Bus bus;
        private readonly R2A03State r2a03;
        private readonly IAudioBackend audio;
        private readonly IProducer<ClockSignal> clock;

        public R2A03(R2A03Bus bus, State state, IAudioBackend audio, IProducer<ClockSignal> clock)
        {
            this.bus = bus;
            this.r2a03 = state.r2a03;
            this.audio = audio;
            this.clock = clock;
        }

        protected override void Read(ushort address, ref byte data)
        {
            clock.Produce(new ClockSignal(132));

            bus.Read(address, ref data);
        }

        protected override void Write(ushort address, byte data)
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

                var dma_src_address = (ushort)(r2a03.dma_segment << 8);
                var dma_dst_address = (ushort)(0x2004);
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
            if (r2a03.sequence_mode == 0)
            {
                switch (r2a03.sequence_time)
                {
                case     0: /*           */ SequencerInterrupt(); break;
                case  7457: Quad(); /*   */ break;
                case 14913: Quad(); Half(); break;
                case 22371: Quad(); /*   */ break;
                case 29828: /*           */ SequencerInterrupt(); break;
                case 29829: Quad(); Half(); SequencerInterrupt(); break;
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
                case  7457: Quad(); /*   */ break;
                case 14913: Quad(); Half(); break;
                case 22371: Quad(); /*   */ break;
                case 29829: /*           */ break;
                case 37281: Quad(); Half(); break;
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

        private void Half()
        {
            Duration.Tick(r2a03.sq1.duration);
            Duration.Tick(r2a03.sq2.duration);
            Duration.Tick(r2a03.tri.duration);
            Duration.Tick(r2a03.noi.duration);
        }

        private void Quad()
        {
            Envelope.Tick(r2a03.sq1.envelope);
            Envelope.Tick(r2a03.sq2.envelope);
            Envelope.Tick(r2a03.noi.envelope);

            if (r2a03.tri.linear_counter_reload)
            {
                r2a03.tri.linear_counter = r2a03.tri.linear_counter_latch;
            }
            else
            {
                if (r2a03.tri.linear_counter != 0)
                {
                    r2a03.tri.linear_counter--;
                }
            }

            if (r2a03.tri.linear_counter_control == false)
            {
                r2a03.tri.linear_counter_reload = false;
            }
        }
    }
}
