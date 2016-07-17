using System;
using System.Diagnostics;
using Beta.Famicom.Abstractions;
using Beta.Famicom.Input;
using Beta.Famicom.Messaging;
using Beta.Platform.Audio;
using Beta.Platform.Messaging;
using Beta.Platform.Processors.RP6502;

namespace Beta.Famicom.CPU
{
    public partial class R2A03
        : Core
        , IConsumer<ClockSignal>
        , IConsumer<VblNmiSignal>
    {
        private static readonly int[][] square_lut = new[]
        {
            new[] { 0, 1, 0, 0, 0, 0, 0, 0 },
            new[] { 0, 1, 1, 0, 0, 0, 0, 0 },
            new[] { 0, 1, 1, 1, 1, 0, 0, 0 },
            new[] { 1, 0, 0, 1, 1, 1, 1, 1 }
        };

        private static readonly int[] triangle_lut = new[]
        {
            15, 14, 13, 12, 11, 10,  9,  8,  7,  6,  5,  4,  3,  2,  1,  0,
             0,  1,  2,  3,  4,  5,  6,  7,  8,  9, 10, 11, 12, 13, 14, 15
        };

        private readonly R2A03Bus bus;
        private readonly R2A03State state;
        private readonly IAudioBackend audio;
        private readonly IProducer<ClockSignal> clockProducer;

        private readonly Sq1StateManager sq1;
        private readonly Sq2StateManager sq2;
        private readonly TriStateManager tri;
        private readonly NoiStateManager noi;

        private bool apuToggle;
        private int strobe;

        public Joypad Joypad1;
        public Joypad Joypad2;

        public R2A03(R2A03Bus bus, State state, IAudioBackend audio, IProducer<ClockSignal> clockProducer)
        {
            this.bus = bus;
            this.state = state.r2a03;
            this.audio = audio;
            this.clockProducer = clockProducer;

            this.sq1 = new Sq1StateManager(state);
            this.sq2 = new Sq2StateManager(state);
            this.tri = new TriStateManager(state);
            this.noi = new NoiStateManager(state);

            Single = 132;
        }

        protected override void Read(ushort address, ref byte data)
        {
            clockProducer.Produce(new ClockSignal(Single));

            bus.Read(address, ref data);
        }

        protected override void Write(ushort address, byte data)
        {
            clockProducer.Produce(new ClockSignal(Single));

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

        private void Read4015(ushort address, ref byte data)
        {
            data = (byte)(
                (state.sq1.duration.counter != 0 ? 0x01 : 0) |
                (state.sq2.duration.counter != 0 ? 0x02 : 0) |
                (state.tri.duration.counter != 0 ? 0x04 : 0) |
                (state.noi.duration.counter != 0 ? 0x08 : 0) |
                (state.irq_pending ? 0x40 : 0));

            state.irq_pending = false;
            Irq(0);
        }

        private void Read4016(ushort address, ref byte data)
        {
            data &= 0xe0;
            data |= Joypad1.GetData(strobe);
        }

        private void Read4017(ushort address, ref byte data)
        {
            data &= 0xe0;
            data |= Joypad2.GetData(strobe);
        }

        private void Write4014(ushort address, byte data)
        {
            state.dma_trigger = true;
            state.dma_segment = data;
        }

        private void Write4015(ushort address, byte data)
        {
            state.sq1.enabled = (data & 0x01) != 0;

            if (!state.sq1.enabled)
            {
                state.sq1.duration.counter = 0;
            }

            state.sq2.enabled = (data & 0x02) != 0;

            if (!state.sq2.enabled)
            {
                state.sq2.duration.counter = 0;
            }

            state.tri.enabled = (data & 0x04) != 0;

            if (!state.tri.enabled)
            {
                state.tri.duration.counter = 0;
            }

            state.noi.enabled = (data & 0x08) != 0;

            if (!state.noi.enabled)
            {
                state.noi.duration.counter = 0;
            }
        }

        private void Write4016(ushort address, byte data)
        {
            strobe = (data & 1);

            if (strobe == 0)
            {
                Joypad1.SetData();
                Joypad2.SetData();
            }
        }

        private void Write4017(ushort address, byte data)
        {
            state.irq_enabled = (data & 0x40) == 0;

            if (state.irq_enabled == false)
            {
                state.irq_pending = false;
                Irq(0);
            }

            state.sequence_mode = (data >> 7) & 1;
            state.sequence_time = 0;

            if (state.sequence_mode == 1)
            {
                Quad();
                Half();
            }
        }

        public void MapTo(IBus bus)
        {
            bus.Map("0100 0000 0000 0000", writer: sq1.Write);
            bus.Map("0100 0000 0000 0001", writer: sq1.Write);
            bus.Map("0100 0000 0000 0010", writer: sq1.Write);
            bus.Map("0100 0000 0000 0011", writer: sq1.Write);

            bus.Map("0100 0000 0000 0100", writer: sq2.Write);
            bus.Map("0100 0000 0000 0101", writer: sq2.Write);
            bus.Map("0100 0000 0000 0110", writer: sq2.Write);
            bus.Map("0100 0000 0000 0111", writer: sq2.Write);

            bus.Map("0100 0000 0000 1000", writer: tri.Write);
            bus.Map("0100 0000 0000 1001", writer: tri.Write);
            bus.Map("0100 0000 0000 1010", writer: tri.Write);
            bus.Map("0100 0000 0000 1011", writer: tri.Write);

            bus.Map("0100 0000 0000 1100", writer: noi.Write);
            bus.Map("0100 0000 0000 1101", writer: noi.Write);
            bus.Map("0100 0000 0000 1110", writer: noi.Write);
            bus.Map("0100 0000 0000 1111", writer: noi.Write);

            // bus.Map("0100 0000 0001 0000", writer: dmc.PokeReg1);
            // bus.Map("0100 0000 0001 0001", writer: dmc.PokeReg2);
            // bus.Map("0100 0000 0001 0010", writer: dmc.PokeReg3);
            // bus.Map("0100 0000 0001 0011", writer: dmc.PokeReg4);

            bus.Map("0100 0000 0001 0100", writer: Write4014);
            bus.Map("0100 0000 0001 0101", reader: Read4015, writer: Write4015);
            bus.Map("0100 0000 0001 0110", reader: Read4016, writer: Write4016);
            bus.Map("0100 0000 0001 0111", reader: Read4017, writer: Write4017);
        }

        public void Consume(VblNmiSignal e)
        {
            Nmi(e.Value);
        }

        public void Consume(ClockSignal e)
        {
            state.sample_prescaler -= 48000;

            if (state.sample_prescaler <= 0)
            {
                state.sample_prescaler += 1789772;
                Sample();
            }

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
            DurationTick(state.sq1.duration);
            DurationTick(state.sq2.duration);
            DurationTick(state.tri.duration);
            DurationTick(state.noi.duration);
        }

        private void DurationTick(Duration duration)
        {
            if (duration.counter != 0 && !duration.halted)
            {
                duration.counter--;
            }
        }

        private void Quad()
        {
            EnvelopeTick(state.sq1.envelope);
            EnvelopeTick(state.sq2.envelope);
            EnvelopeTick(state.noi.envelope);
        }

        private void EnvelopeTick(Envelope envelope)
        {
            if (envelope.start)
            {
                envelope.start = false;
                envelope.timer = envelope.period + 1;
                envelope.decay = 15;
            }
            else
            {
                if (envelope.timer == 0)
                {
                    envelope.timer = envelope.period + 1;

                    if (envelope.decay != 0 || envelope.looping)
                    {
                        envelope.decay = (envelope.decay - 1) & 15;
                    }
                }
                else
                {
                    envelope.timer--;
                }
            }
        }

        private int EnvelopeVolume(Envelope envelope)
        {
            return envelope.constant
                ? envelope.period
                : envelope.decay
                ;
        }

        private void Sample()
        {
            var sq1 = state.sq1;
            var sq1_volume = EnvelopeVolume(sq1.envelope);
            var sq1_out = sq1.duration.counter != 0
                ? sq1_volume * square_lut[sq1.duty_form][sq1.duty_step]
                : 0
                ;

            var sq2 = state.sq2;
            var sq2_volume = EnvelopeVolume(sq2.envelope);
            var sq2_out = sq2.duration.counter != 0
                ? sq2_volume * square_lut[sq2.duty_form][sq2.duty_step]
                : 0
                ;

            var tri = state.tri;
            var tri_out = tri.duration.counter != 0
                ? triangle_lut[tri.step]
                : 0
                ;

            var noi = state.noi;
            var noi_volume = EnvelopeVolume(noi.envelope);
            var noi_out = noi.duration.counter != 0
                ? noi_volume * (~noi.lfsr & 1)
                : 0
                ;

            var output = ((sq1_out + sq2_out + tri_out + noi_out) * 32767) / 16 / 4;

            audio.Render(output);
        }
    }
}
