using Beta.Platform;
using Beta.Famicom.Abstractions;
using Beta.Famicom.CPU;
using Beta.Famicom.Formats;
using Beta.Famicom.Memory;
using Beta.Famicom.Messaging;

namespace Beta.Famicom.Boards.Namcot
{
    // TODO: Figure out how external VRAM is really implemented.

    [BoardName("NAMCOT-163")]
    public class Namcot163 : Board
    {
        private Sound sound;
        private int[] chrPages;
        private int[] nmtPages;
        private int[] prgPages;
        private bool irqEnabled;
        private int irqCounter;
        private IMemory nmtA;
        private IMemory nmtB;
        private IMemory nmtC;
        private IMemory nmtD;

        public Namcot163(CartridgeImage image)
            : base(image)
        {
            chrPages = new int[8];
            nmtPages = new int[4];
            prgPages = new int[4];

            sound = new Sound();
        }

        private void PeekNmtA(ushort address, ref byte data)
        {
            nmtA.Peek((address & 0x3ff) | nmtPages[0], ref data);
        }

        private void PeekNmtB(ushort address, ref byte data)
        {
            nmtB.Peek((address & 0x3ff) | nmtPages[1], ref data);
        }

        private void PeekNmtC(ushort address, ref byte data)
        {
            nmtC.Peek((address & 0x3ff) | nmtPages[2], ref data);
        }

        private void PeekNmtD(ushort address, ref byte data)
        {
            nmtD.Peek((address & 0x3ff) | nmtPages[3], ref data);
        }

        private void PokeNmtA(ushort address, ref byte data)
        {
            nmtA.Poke(address & 0x3ff, ref data);
        }

        private void PokeNmtB(ushort address, ref byte data)
        {
            nmtB.Poke(address & 0x3ff, ref data);
        }

        private void PokeNmtC(ushort address, ref byte data)
        {
            nmtC.Poke(address & 0x3ff, ref data);
        }

        private void PokeNmtD(ushort address, ref byte data)
        {
            nmtD.Poke(address & 0x3ff, ref data);
        }

        private void Peek4800(ushort address, ref byte data)
        {
            data = sound.Peek4800();
        }

        private void Peek5000(ushort address, ref byte data)
        {
            Cpu.Irq(0);
            data = (byte)(irqCounter >> 0);
        }

        private void Peek5800(ushort address, ref byte data)
        {
            Cpu.Irq(0);
            data = (byte)(irqCounter >> 8);
        }

        private void Poke4800(ushort address, ref byte data)
        {
            sound.Poke4800(data);
        }

        private void Poke5000(ushort address, ref byte data)
        {
            Cpu.Irq(0);
            irqCounter = (irqCounter & ~0x00ff) | ((data & 0xff) << 0);
        }

        private void Poke5800(ushort address, ref byte data)
        {
            Cpu.Irq(0);
            irqCounter = (irqCounter & ~0x7f00) | ((data & 0x7f) << 8);
            irqEnabled = (data & 0x80) != 0;
        }

        private void Poke8000(ushort address, ref byte data)
        {
            chrPages[0] = data << 10;
        }

        private void Poke8800(ushort address, ref byte data)
        {
            chrPages[1] = data << 10;
        }

        private void Poke9000(ushort address, ref byte data)
        {
            chrPages[2] = data << 10;
        }

        private void Poke9800(ushort address, ref byte data)
        {
            chrPages[3] = data << 10;
        }

        private void PokeA000(ushort address, ref byte data)
        {
            chrPages[4] = data << 10;
        }

        private void PokeA800(ushort address, ref byte data)
        {
            chrPages[5] = data << 10;
        }

        private void PokeB000(ushort address, ref byte data)
        {
            chrPages[6] = data << 10;
        }

        private void PokeB800(ushort address, ref byte data)
        {
            chrPages[7] = data << 10;
        }

        private void PokeC000(ushort address, ref byte data)
        {
            if (data < 0xe0)
            {
                nmtA = Chr;
                nmtPages[0] = (data << 10);
            }
            else
            {
                //nmt_a = ppu.nmt[data & 1];
                nmtPages[0] = 0;
            }
        }

        private void PokeC800(ushort address, ref byte data)
        {
            if (data < 0xe0)
            {
                nmtB = Chr;
                nmtPages[1] = (data << 10);
            }
            else
            {
                //nmt_b = ppu.nmt[data & 1];
                nmtPages[1] = 0;
            }
        }

        private void PokeD000(ushort address, ref byte data)
        {
            if (data < 0xe0)
            {
                nmtC = Chr;
                nmtPages[2] = (data << 10);
            }
            else
            {
                //nmt_c = ppu.nmt[data & 1];
                nmtPages[2] = 0;
            }
        }

        private void PokeD800(ushort address, ref byte data)
        {
            if (data < 0xe0)
            {
                nmtD = Chr;
                nmtPages[3] = (data << 10);
            }
            else
            {
                //nmt_d = ppu.nmt[data & 1];
                nmtPages[3] = 0;
            }
        }

        private void PokeE000(ushort address, ref byte data)
        {
            prgPages[0] = (data & 0x3f) << 13;
        }

        private void PokeE800(ushort address, ref byte data)
        {
            prgPages[1] = (data & 0x3f) << 13;
        }

        private void PokeF000(ushort address, ref byte data)
        {
            prgPages[2] = (data & 0x3f) << 13;
        }

        private void PokeF800(ushort address, ref byte data)
        {
            sound.PokeF800(data);
        }

        protected override int DecodeChr(ushort address)
        {
            return (address & 0x3ff) | chrPages[(address >> 10) & 7];
        }

        protected override int DecodePrg(ushort address)
        {
            return (address & 0x1fff) | prgPages[(address >> 13) & 3];
        }

        public override void Consume(ClockSignal e)
        {
            if (irqEnabled)
            {
                if (irqCounter == 0x7fff)
                {
                    Cpu.Irq(1);
                }
                else
                {
                    irqCounter++;
                }
            }
        }

        public override void Initialize()
        {
            base.Initialize();

            nmtA = Chr;
            nmtB = Chr;
            nmtC = Chr;
            nmtD = Chr;

            prgPages[0] = +0 << 13;
            prgPages[1] = +0 << 13;
            prgPages[2] = +0 << 13;
            prgPages[3] = -1 << 13;

            Cpu.Hook(sound);

            sound.Initialize();
        }

        public override void MapToCpu(IBus bus)
        {
            base.MapToCpu(bus);

            bus.Decode("0100 1--- ---- ----").Peek(Peek4800).Poke(Poke4800);
            bus.Decode("0101 0--- ---- ----").Peek(Peek5000).Poke(Poke5000);
            bus.Decode("0101 1--- ---- ----").Peek(Peek5800).Poke(Poke5800);
            // $6000
            // $6800
            // $7000
            // $7800
            bus.Decode("1000 0--- ---- ----").Poke(Poke8000);
            bus.Decode("1000 1--- ---- ----").Poke(Poke8800);
            bus.Decode("1001 0--- ---- ----").Poke(Poke9000);
            bus.Decode("1001 1--- ---- ----").Poke(Poke9800);
            bus.Decode("1010 0--- ---- ----").Poke(PokeA000);
            bus.Decode("1010 1--- ---- ----").Poke(PokeA800);
            bus.Decode("1011 0--- ---- ----").Poke(PokeB000);
            bus.Decode("1011 1--- ---- ----").Poke(PokeB800);
            bus.Decode("1100 0--- ---- ----").Poke(PokeC000);
            bus.Decode("1100 1--- ---- ----").Poke(PokeC800);
            bus.Decode("1101 0--- ---- ----").Poke(PokeD000);
            bus.Decode("1101 1--- ---- ----").Poke(PokeD800);
            bus.Decode("1110 0--- ---- ----").Poke(PokeE000);
            bus.Decode("1110 1--- ---- ----").Poke(PokeE800); //  $E800
            bus.Decode("1111 0--- ---- ----").Poke(PokeF000);
            bus.Decode("1111 1--- ---- ----").Poke(PokeF800);
        }

        public override void MapToPpu(IBus bus)
        {
            base.MapToPpu(bus);

            bus.Decode("001- 00-- ---- ----").Peek(PeekNmtA).Poke(PokeNmtA);
            bus.Decode("001- 01-- ---- ----").Peek(PeekNmtB).Poke(PokeNmtB);
            bus.Decode("001- 10-- ---- ----").Peek(PeekNmtC).Poke(PokeNmtC);
            bus.Decode("001- 11-- ---- ----").Peek(PeekNmtD).Poke(PokeNmtD);
        }

        private class Sound : R2A03.ChannelExt
        {
            private Channel[] channels = new Channel[8];
            private Timing timing;
            private bool step;
            private byte[] wave = new byte[256];
            private byte[] wram = new byte[128];
            private int addr;
            private int curr;

            public Sound()
            {
                timing.Cycles =
                timing.Single = R2A03.PHASE * 15;
                timing.Period = R2A03.DELAY;

                for (var i = 0; i < 8; i++)
                {
                    channels[i] = new Channel(i);
                }
            }

            public override void Initialize()
            {
                base.Initialize();

                PokeF800(0x80); // address: 0, auto-increment on

                for (var i = 0x00; i < 0x80; i++)
                {
                    Poke4800(0x00); // clear wram, and initialize sound channels
                }

                PokeF800(0x00); // address: 0, auto-increment off
            }

            public byte Peek4800()
            {
                var data = wram[addr];

                if (step)
                {
                    addr = (addr + 1) & 0x7f;
                }

                return data;
            }

            public void Poke4800(byte data)
            {
                wave[(addr << 1) | 0] = (byte)(data & 15);
                wave[(addr << 1) | 1] = (byte)(data >> 4);

                wram[addr] = data;

                if (addr > 0x3f)
                {
                    var c = channels[(addr >> 3) & 7];

                    switch (addr & 0x07)
                    {
                    case 0: c.Rd.ub0 = data; break;
                    case 1: c.Rp.ub0 = data; break;
                    case 2: c.Rd.ub1 = data; break;
                    case 3: c.Rp.ub1 = data; break;
                    case 4: c.Rd.ub2 = (byte)(data & 0x03); c.Count = 16777216 - ((data & ~3) << 16); break;
                    case 5: c.Rp.ub2 = data; break;
                    case 6: c.Index = data; break;
                    case 7: c.Level = (byte)(data & 0x0f); break;
                    }
                }

                if (step)
                {
                    addr = (addr + 1) & 0x7f;
                }
            }

            public void PokeF800(byte data)
            {
                addr = (data & 0x7f);
                step = (data & 0x80) != 0;
            }

            public override short Render()
            {
                var enable = (wram[0x7f] & 0x70) >> 4;
                var output = 0;

                for (timing.Cycles -= timing.Period; timing.Cycles < 0; timing.Cycles += timing.Single)
                {
                    channels[7 - curr].Update(wram, wave);

                    if (++curr > enable)
                        curr = 0;
                }

                for (var i = 7 - enable; i <= 7; i++)
                {
                    output += channels[i].Render();
                }

                return (short)(((output * 32767) / 225) / (enable + 1));
            }

            private class Channel
            {
                private byte start;
                private byte value;

                public Register32 Rd;
                public Register32 Rp;
                public byte Index;
                public byte Level;
                public int Count;

                public Channel(int ordinal)
                {
                    start = (byte)(0x40 | (ordinal << 3));
                }

                public byte Render()
                {
                    return (byte)(value * Level);
                }

                public void Update(byte[] wram, byte[] wave)
                {
                    // -- Clock phase counter --
                    Rp.sd0 += Rd.sd0;
                    Rp.sd0 %= Count;

                    // -- Get next sample --
                    value = wave[(Rp.ub2 + Index) & 0xff];

                    // -- Write phase back into ram (This is detectable by games via $4800) --
                    wram[start | 1] = Rp.ub0;
                    wram[start | 3] = Rp.ub1;
                    wram[start | 5] = Rp.ub2;
                }
            }
        }
    }
}
