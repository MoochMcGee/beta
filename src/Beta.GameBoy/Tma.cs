using Beta.GameBoy.Messaging;
using Beta.Platform;
using Beta.Platform.Messaging;
using Beta.Platform.Processors;

namespace Beta.GameBoy
{
    public sealed class Tma : IConsumer<ClockSignal>
    {
        private static byte[] lut =
        {
            0x01, // (1.048576MHz / 256) =   4.096KHz
            0x40, // (1.048576MHz /   4) = 262.144KHz
            0x10, // (1.048576MHz /  16) =  65.536KHz
            0x04  // (1.048576MHz /  64) =  16.384KHz
        };

        private readonly IProducer<InterruptSignal> interrupt;

        private Register16 div;
        private Register16 tma;
        private byte cnt;
        private byte mod;

        public Tma(IAddressSpace addressSpace, IProducer<InterruptSignal> interrupt)
        {
            this.interrupt = interrupt;

            addressSpace.Map(0xff04, a => div.h, (a, d) => div.h = 0);
            addressSpace.Map(0xff05, a => tma.h, (a, d) => tma.h = d);
            addressSpace.Map(0xff06, a => mod, (a, d) => mod = d);
            addressSpace.Map(0xff07, a => cnt, (a, d) => cnt = d);
        }

        public void Consume(ClockSignal e)
        {
            div.w += lut[3];

            if ((cnt & 0x4) != 0)
            {
                tma.w += lut[cnt & 3];

                if (tma.w < lut[cnt & 3])
                {
                    tma.h = mod;
                    interrupt.Produce(new InterruptSignal(LR35902.Interrupt.ELAPSE));
                }
            }
        }
    }
}
