using Beta.GameBoy.Messaging;
using Beta.Platform.Messaging;
using Beta.Platform.Processors;

namespace Beta.GameBoy.CPU
{
    public class Cpu : LR35902, IConsumer<InterruptSignal>
    {
        private IAddressSpace addressSpace;
        private byte ef;
        private byte rf;

        public Cpu(IAddressSpace addressSpace, IProducer<ClockSignal> clockProducer)
            : base(clockProducer)
        {
            this.addressSpace = addressSpace;

            addressSpace.Map(0xff0f, a => rf, (a, data) => rf = data);
            addressSpace.Map(0xffff, a => ef, (a, data) => ef = data);

            Single = 4;
        }

        protected override void Dispatch()
        {
            if (interrupt.ff2 == 1)
            {
                interrupt.ff2 = 0;
                interrupt.ff1 = 1;
            }

            clockProducer.Produce(new ClockSignal(Single));
        }

        protected override byte Read(ushort address)
        {
            Dispatch();

            return addressSpace.Read(address);
        }

        protected override void Write(ushort address, byte data)
        {
            Dispatch();

            addressSpace.Write(address, data);
        }

        public override void Update()
        {
            base.Update();

            var flags = (rf & ef) & -interrupt.ff1;
            if (flags != 0)
            {
                interrupt.ff1 = 0;

                if ((flags & 0x01) != 0) { rf ^= 0x01; Rst(0x40); return; }
                if ((flags & 0x02) != 0) { rf ^= 0x02; Rst(0x48); return; }
                if ((flags & 0x04) != 0) { rf ^= 0x04; Rst(0x50); return; }
                if ((flags & 0x08) != 0) { rf ^= 0x08; Rst(0x58); return; }
                if ((flags & 0x10) != 0) { rf ^= 0x10; Rst(0x60); return; }
            }
        }

        public void Consume(InterruptSignal e)
        {
            rf |= e.Flag;

            if ((ef & e.Flag) != 0)
            {
                Halt = false;

                if (e.Flag == Interrupt.JOYPAD)
                {
                    Stop = false;
                }
            }
        }
    }
}
