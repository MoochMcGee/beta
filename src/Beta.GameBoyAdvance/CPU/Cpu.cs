using Beta.GameBoyAdvance.APU;
using Beta.GameBoyAdvance.PPU;
using Beta.Platform;
using Beta.Platform.Processors;
using half = System.UInt16;

namespace Beta.GameBoyAdvance.CPU
{
    public class Cpu : Arm7
    {
        private Driver gameSystem;
        private Apu apu;
        private Ppu ppu;
        private Timer[] timer;

        public DmaController Dma;
        public Register16 ief;
        public Register16 irf;
        public bool ime;

        public Cpu(Driver gameSystem)
        {
            this.gameSystem = gameSystem;

            Dma = new DmaController
            {
                Channels = new[]
                {
                    new Dma(gameSystem, Source.DMA_0),
                    new Dma(gameSystem, Source.DMA_1),
                    new Dma(gameSystem, Source.DMA_2),
                    new Dma(gameSystem, Source.DMA_3)
                }
            };

            timer = new[]
            {
                new Timer(gameSystem, Source.TIMER_0, 0),
                new Timer(gameSystem, Source.TIMER_1, 1),
                new Timer(gameSystem, Source.TIMER_2, 2),
                new Timer(gameSystem, Source.TIMER_3, 3)
            };

            timer[0].NextTimer = timer[1];
            timer[1].NextTimer = timer[2];
            timer[2].NextTimer = timer[3];
            timer[3].NextTimer = null;
        }

        private byte Read200(uint address)
        {
            return ief.l;
        }

        private byte Read201(uint address)
        {
            return ief.h;
        }

        private byte Read202(uint address)
        {
            return irf.l;
        }

        private byte Read203(uint address)
        {
            return irf.h;
        }

        private void Write200(uint address, byte data)
        {
            ief.l = data;
        }

        private void Write201(uint address, byte data)
        {
            ief.h = data;
        }

        private void Write202(uint address, byte data)
        {
            irf.l &= (byte)~data;
        }

        private void Write203(uint address, byte data)
        {
            irf.h &= (byte)~data;
        }

        private byte Read208(uint address)
        {
            return (byte)(ime ? 1 : 0);
        }

        private byte Read209(uint address)
        {
            return 0;
        }

        private void Write208(uint address, byte data)
        {
            ime = (data & 1) != 0;
        }

        private void Write209(uint address, byte data)
        {
        }

        protected override void Dispatch()
        {
            Dma.Transfer();

            ppu.Update(Cycles);
            apu.Update(Cycles);

            if (timer[0].Enabled) { timer[0].Update(Cycles); }
            if (timer[1].Enabled) { timer[1].Update(Cycles); }
            if (timer[2].Enabled) { timer[2].Update(Cycles); }
            if (timer[3].Enabled) { timer[3].Update(Cycles); }
        }

        protected override uint Read(int size, uint address)
        {
            int cycles;
            var data = gameSystem.Read(size, address, out cycles);

            this.Cycles += cycles;

            return data;
        }

        protected override void Write(int size, uint address, uint data)
        {
            if (size == 0)
            {
                data = (data & 0xff);
                data |= (data << 8);
            }

            if (size == 1)
            {
                data = (data & 0xffff);
                data |= (data << 16);
            }

            int cycles;
            gameSystem.Write(size, address, data, out cycles);
        }

        public override void Initialize()
        {
            base.Initialize();

            apu = gameSystem.Apu;
            ppu = gameSystem.Ppu;

            Dma.Channels[0].Initialize(0x0b0);
            Dma.Channels[1].Initialize(0x0bc);
            Dma.Channels[2].Initialize(0x0c8);
            Dma.Channels[3].Initialize(0x0d4);

            timer[0].Initialize(0x100);
            timer[1].Initialize(0x104);
            timer[2].Initialize(0x108);
            timer[3].Initialize(0x10c);

            var mmio = gameSystem.mmio;
            mmio.Map(0x200, Read200, Write200);
            mmio.Map(0x201, Read201, Write201);
            mmio.Map(0x202, Read202, Write202);
            mmio.Map(0x203, Read203, Write203);
            mmio.Map(0x208, Read208, Write208);
            mmio.Map(0x209, Read209, Write209);
        }

        public override void Update()
        {
            interrupt = ((ief.w & irf.w) != 0) && ime;

            base.Update();
        }

        public void Interrupt(half interrupt)
        {
            irf.w |= interrupt;
        }

        public static class Source
        {
            public const ushort V_BLANK = 0x0001; // 0 - lcd v-blank
            public const ushort H_BLANK = 0x0002; // 1 - lcd h-blank
            public const ushort V_CHECK = 0x0004; // 2 - lcd v-counter match
            public const ushort TIMER_0 = 0x0008; // 3 - timer 0 overflow
            public const ushort TIMER_1 = 0x0010; // 4 - timer 1 overflow
            public const ushort TIMER_2 = 0x0020; // 5 - timer 2 overflow
            public const ushort TIMER_3 = 0x0040; // 6 - timer 3 overflow
            public const ushort SERIAL = 0x0080; // 7 - serial communication
            public const ushort DMA_0 = 0x0100; // 8 - dma 0
            public const ushort DMA_1 = 0x0200; // 9 - dma 1
            public const ushort DMA_2 = 0x0400; // a - dma 2
            public const ushort DMA_3 = 0x0800; // b - dma 3
            public const ushort JOYPAD = 0x1000; // c - keypad
            public const ushort CART = 0x2000; // d - game pak
            public const ushort RES0 = 0x4000; // e - not used
            public const ushort RES1 = 0x8000; // f - not used
        }
    }
}
