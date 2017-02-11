using Beta.Famicom.Input;
using Beta.Famicom.Messaging;
using Beta.Platform.Messaging;

namespace Beta.Famicom.CPU
{
    public sealed class R2A03Registers
    {
        private readonly InputConnector input;
        private readonly R2A03State r2a03;
        private readonly IProducer<IrqSignal> irq;
        private readonly IProducer<HalfFrameSignal> half;
        private readonly IProducer<QuadFrameSignal> quad;

        public R2A03Registers(
            InputConnector input,
            State state,
            IProducer<IrqSignal> irq,
            IProducer<HalfFrameSignal> half,
            IProducer<QuadFrameSignal> quad)
        {
            this.input = input;
            this.r2a03 = state.r2a03;
            this.irq = irq;
            this.half = half;
            this.quad = quad;
        }

        public void Read(int address, ref byte data)
        {
            // switch (address & ~3)
            // {
            // case 0x4000: sq1.Read(address, ref data); break;
            // case 0x4004: sq2.Read(address, ref data); break;
            // case 0x4008: tri.Read(address, ref data); break;
            // case 0x400c: noi.Read(address, ref data); break;
            // case 0x4010: dmc.Read(address, ref data); break;
            // }

            if (address == 0x4014) { }

            if (address == 0x4015)
            {
                data = (byte)(
                    (r2a03.sq1.duration.counter != 0 ? 0x01 : 0) |
                    (r2a03.sq2.duration.counter != 0 ? 0x02 : 0) |
                    (r2a03.tri.duration.counter != 0 ? 0x04 : 0) |
                    (r2a03.noi.duration.counter != 0 ? 0x08 : 0) |
                    (r2a03.sequence_irq_pending ? 0x40 : 0));

                r2a03.sequence_irq_pending = false;
                irq.Produce(new IrqSignal(0));
            }

            if (address == 0x4016)
            {
                data &= 0xe0;
                data |= input.ReadJoypad1();
            }

            if (address == 0x4017)
            {
                data &= 0xe0;
                data |= input.ReadJoypad2();
            }
        }

        public void Write(int address, byte data)
        {
            switch (address & ~3)
            {
            case 0x4000: SQ1.Write(r2a03.sq1, address, data); break;
            case 0x4004: SQ2.Write(r2a03.sq2, address, data); break;
            case 0x4008: TRI.Write(r2a03.tri, address, data); break;
            case 0x400c: NOI.Write(r2a03.noi, address, data); break;
            case 0x4010: DMC.Write(r2a03.dmc, address, data); break;
            }

            if (address == 0x4014)
            {
                r2a03.dma_segment = data;
                r2a03.dma_trigger = true;
            }

            if (address == 0x4015)
            {
                r2a03.sq1.enabled = (data & 0x01) != 0;
                r2a03.sq2.enabled = (data & 0x02) != 0;
                r2a03.tri.enabled = (data & 0x04) != 0;
                r2a03.noi.enabled = (data & 0x08) != 0;

                if (!r2a03.sq1.enabled) { r2a03.sq1.duration.counter = 0; }
                if (!r2a03.sq2.enabled) { r2a03.sq2.duration.counter = 0; }
                if (!r2a03.tri.enabled) { r2a03.tri.duration.counter = 0; }
                if (!r2a03.noi.enabled) { r2a03.noi.duration.counter = 0; }
            }

            if (address == 0x4016)
            {
                input.Write(data);
            }

            if (address == 0x4017)
            {
                r2a03.sequence_irq_enabled = (data & 0x40) == 0;

                if (r2a03.sequence_irq_enabled == false)
                {
                    r2a03.sequence_irq_pending = false;
                    irq.Produce(new IrqSignal(0));
                }

                r2a03.sequence_mode = (data >> 7) & 1;
                r2a03.sequence_time = 0;

                if (r2a03.sequence_mode == 1)
                {
                    half.Produce(null);
                    quad.Produce(null);
                }
            }
        }
    }
}
