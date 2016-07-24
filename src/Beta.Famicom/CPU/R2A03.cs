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
        private readonly R2A03State state;
        private readonly IAudioBackend audio;
        private readonly IProducer<ClockSignal> clock;

        private bool apuToggle;

        public R2A03(R2A03Bus bus, State state, IAudioBackend audio, IProducer<ClockSignal> clock)
        {
            this.bus = bus;
            this.state = state.r2a03;
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

            if (state.dma_trigger)
            {
                state.dma_trigger = false;

                var dma_src_address = (ushort)(state.dma_segment << 8);
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
            if (state.sequence_mode == 0)
            {
                bool irq_pending = false;

                switch (state.sequence_time)
                {
                case     0: /*           */ break;
                case  7457: Quad(); /*   */ break;
                case 14913: Quad(); Half(); break;
                case 22371: Quad(); /*   */ break;
                case 29828: /*           */ irq_pending |= state.irq_enabled; break;
                case 29829: Quad(); Half(); irq_pending |= state.irq_enabled; break;
                case 29830: /*           */ irq_pending |= state.irq_enabled; break;
                }

                if (irq_pending)
                {
                    state.irq_pending = irq_pending;
                    Irq(1);
                }

                if (++state.sequence_time == 29830)
                {
                    state.sequence_time = 0;
                }
            }
            else
            {
                switch (state.sequence_time)
                {
                case  7457: Quad(); /*   */ break;
                case 14913: Quad(); Half(); break;
                case 22371: Quad(); /*   */ break;
                case 29829: /*           */ break;
                case 37281: Quad(); Half(); break;
                }

                if (++state.sequence_time == 37282)
                {
                    state.sequence_time = 0;
                }
            }

            if (state.tri.timer != 0 && --state.tri.timer == 0)
            {
                state.tri.timer = state.tri.period + 1;
                state.tri.step = (state.tri.step + 1) & 31;
            }

            if (apuToggle = !apuToggle)
            {
                if (state.sq1.timer != 0 && --state.sq1.timer == 0)
                {
                    state.sq1.timer = state.sq1.period + 1;
                    state.sq1.duty_step = (state.sq1.duty_step - 1) & 7;
                }

                if (state.sq2.timer != 0 && --state.sq2.timer == 0)
                {
                    state.sq2.timer = state.sq2.period + 1;
                    state.sq2.duty_step = (state.sq2.duty_step - 1) & 7;
                }

                if (state.noi.timer != 0 && --state.noi.timer == 0)
                {
                    state.noi.timer = state.noi.period + 1;

                    var tap0 = state.noi.lfsr;
                    var tap1 = state.noi.lfsr_mode == 1
                        ? state.noi.lfsr >> 6
                        : state.noi.lfsr >> 1
                        ;

                    var feedback = (tap0 ^ tap1) & 1;

                    state.noi.lfsr = (state.noi.lfsr >> 1) | (feedback << 14);
                }
            }
        }

        private void Half()
        {
            Duration.Tick(state.sq1.duration);
            Duration.Tick(state.sq2.duration);
            Duration.Tick(state.tri.duration);
            Duration.Tick(state.noi.duration);
        }

        private void Quad()
        {
            Envelope.Tick(state.sq1.envelope);
            Envelope.Tick(state.sq2.envelope);
            Envelope.Tick(state.noi.envelope);
        }
    }
}
