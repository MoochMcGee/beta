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

        private readonly Sq1Registers sq1;
        private readonly Sq2Registers sq2;
        private readonly TriRegisters tri;
        private readonly NoiRegisters noi;
        private readonly DmcRegisters dmc;

        public R2A03Registers(InputConnector input, State state, IProducer<IrqSignal> irq)
        {
            this.input = input;
            this.r2a03 = state.r2a03;
            this.irq = irq;

            this.sq1 = new Sq1Registers(state);
            this.sq2 = new Sq2Registers(state);
            this.tri = new TriRegisters(state);
            this.noi = new NoiRegisters(state);
            this.dmc = new DmcRegisters(state);
        }

        public void Read(ushort address, ref byte data)
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

        public void Write(ushort address, byte data)
        {
            switch (address & ~3)
            {
            case 0x4000: sq1.Write(address, data); break;
            case 0x4004: sq2.Write(address, data); break;
            case 0x4008: tri.Write(address, data); break;
            case 0x400c: noi.Write(address, data); break;
            case 0x4010: dmc.Write(address, data); break;
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

                // if (r2a03.sequence_mode == 1)
                // {
                //     Quad();
                //     Half();
                // }
            }
        }
    }
}
