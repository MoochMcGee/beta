using System;
using System.Linq;
using Beta.Platform;
using Beta.Platform.Exceptions;
using Beta.Famicom.Abstractions;
using Beta.Famicom.CPU;
using Beta.Famicom.Formats;
using Beta.Famicom.Messaging;

namespace Beta.Famicom.Boards.Konami
{
    [BoardName("KONAMI-VRC-7")]
    public class KonamiVrc7 : Board
    {
        private Irq irq;
        private Sound sound;
        private int[] chrPages;
        private int[] prgPages;
        private int mirroring;

        public KonamiVrc7(CartridgeImage image)
            : base(image)
        {
            prgPages = new int[4];
            chrPages = new int[8];

            irq = new Irq();
            sound = new Sound();
        }

        private void Poke8000(ushort address, ref byte data)
        {
            prgPages[0] = data << 13;
        }

        private void Poke8010(ushort address, ref byte data)
        {
            prgPages[1] = data << 13;
        }

        private void Poke9000(ushort address, ref byte data)
        {
            prgPages[2] = data << 13;
        }

        private void Poke9010(ushort address, ref byte data)
        {
            sound.WriteAddr(data);
        }

        private void Poke9030(ushort address, ref byte data)
        {
            sound.WriteData(data);
        }

        private void PokeA000(ushort address, ref byte data)
        {
            chrPages[0] = data << 10;
        }

        private void PokeA010(ushort address, ref byte data)
        {
            chrPages[1] = data << 10;
        }

        private void PokeB000(ushort address, ref byte data)
        {
            chrPages[2] = data << 10;
        }

        private void PokeB010(ushort address, ref byte data)
        {
            chrPages[3] = data << 10;
        }

        private void PokeC000(ushort address, ref byte data)
        {
            chrPages[4] = data << 10;
        }

        private void PokeC010(ushort address, ref byte data)
        {
            chrPages[5] = data << 10;
        }

        private void PokeD000(ushort address, ref byte data)
        {
            chrPages[6] = data << 10;
        }

        private void PokeD010(ushort address, ref byte data)
        {
            chrPages[7] = data << 10;
        }

        private void PokeE000(ushort address, ref byte data)
        {
            mirroring = (data & 0x03);
        }

        private void PokeE010(ushort address, ref byte data)
        {
            irq.Refresh = data;
        }

        private void PokeF000(ushort address, ref byte data)
        {
            irq.Mode = (data & 0x04) != 0;
            irq.Enabled = (data & 0x02) != 0;
            irq.EnabledRefresh = (data & 0x01) != 0;
            irq.Scaler = 341;

            if (irq.Enabled)
            {
                irq.Counter = irq.Refresh;
            }

            Cpu.Irq(0);
        }

        private void PokeF010(ushort address, ref byte data)
        {
            irq.Enabled = irq.EnabledRefresh;
            Cpu.Irq(0);
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
            if (!irq.Enabled)
                return;

            if (irq.Mode)
            {
                if (irq.Clock())
                {
                    Cpu.Irq(1);
                }
            }
            else
            {
                irq.Scaler -= 3;

                if (irq.Scaler <= 0)
                {
                    irq.Scaler += 341;

                    if (irq.Clock())
                    {
                        Cpu.Irq(1);
                    }
                }
            }
        }

        public override void Initialize()
        {
            base.Initialize();

            prgPages[0] = +0 << 13;
            prgPages[1] = +0 << 13;
            prgPages[2] = +0 << 13;
            prgPages[3] = -1 << 13;

            Cpu.Hook(sound);
        }

        public override void MapToCpu(IBus bus)
        {
            base.MapToCpu(bus);

            var pin11 = 1 << int.Parse(GetPin("VRC7", 0x11).Replace("PRG A", ""));

            if (pin11 == 8)
            {
                bus.Map("1000 ---- ---- 0---", writer: Poke8000);
                bus.Map("1000 ---- ---- 1---", writer: Poke8010);
                bus.Map("1001 ---- ---- 0---", writer: Poke9000);
                bus.Map("1001 ---- ---- 1---", writer: Poke9010);
                bus.Map("1010 ---- ---- 0---", writer: PokeA000);
                bus.Map("1010 ---- ---- 1---", writer: PokeA010);
                bus.Map("1011 ---- ---- 0---", writer: PokeB000);
                bus.Map("1011 ---- ---- 1---", writer: PokeB010);
                bus.Map("1100 ---- ---- 0---", writer: PokeC000);
                bus.Map("1100 ---- ---- 1---", writer: PokeC010);
                bus.Map("1101 ---- ---- 0---", writer: PokeD000);
                bus.Map("1101 ---- ---- 1---", writer: PokeD010);
                bus.Map("1110 ---- ---- 0---", writer: PokeE000);
                bus.Map("1110 ---- ---- 1---", writer: PokeE010);
                bus.Map("1111 ---- ---- 0---", writer: PokeF000);
                bus.Map("1111 ---- ---- 1---", writer: PokeF010);
            }
            else
            {
                bus.Map("1000 ---- ---0 ----", writer: Poke8000);
                bus.Map("1000 ---- ---1 ----", writer: Poke8010);
                bus.Map("1001 ---- ---0 ----", writer: Poke9000);
                bus.Map("1001 ---- ---1 ----", writer: Poke9010);
                bus.Map("1010 ---- ---0 ----", writer: PokeA000);
                bus.Map("1010 ---- ---1 ----", writer: PokeA010);
                bus.Map("1011 ---- ---0 ----", writer: PokeB000);
                bus.Map("1011 ---- ---1 ----", writer: PokeB010);
                bus.Map("1100 ---- ---0 ----", writer: PokeC000);
                bus.Map("1100 ---- ---1 ----", writer: PokeC010);
                bus.Map("1101 ---- ---0 ----", writer: PokeD000);
                bus.Map("1101 ---- ---1 ----", writer: PokeD010);
                bus.Map("1110 ---- ---0 ----", writer: PokeE000);
                bus.Map("1110 ---- ---1 ----", writer: PokeE010);
                bus.Map("1111 ---- ---0 ----", writer: PokeF000);
                bus.Map("1111 ---- ---1 ----", writer: PokeF010);
            }

            bus.Map("1001 0000 0011 0000", writer: Poke9030); // external audio data port for lagrange point, not sure how it's wired in
        }

        public override int VRamA10(ushort address)
        {
            var x = (address >> 10) & 1;
            var y = (address >> 11) & 1;

            switch (mirroring)
            {
            case 0: return x;
            case 1: return y;
            case 2: return 0;
            case 3: return 1;
            }

            throw new CompilerPleasingException();
        }

        private class Irq
        {
            public bool Mode;
            public bool Enabled;
            public bool EnabledRefresh;
            public int Counter;
            public int Refresh;
            public int Scaler;

            public bool Clock()
            {
                if (Counter == 0xff)
                {
                    Counter = Refresh;
                    return true;
                }

                Counter++;
                return false;
            }
        }

        private class Sound : R2A03.ChannelExt
        {
            private const int CLOCK = 3579545;
            private const int CLOCK_DIV = CLOCK / 72 + 1;
            private const int RATE = 48000;
            private const int AM_DELTA = 37 * 65536 / CLOCK_DIV / 10; // ~3.7hz
            private const int PM_DELTA = 64 * 65536 / CLOCK_DIV / 10; // ~6.4hz

            private static byte[][] instruments = new[]
            {
                new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 },
                new byte[] { 0x03, 0x21, 0x05, 0x06, 0xb8, 0x82, 0x42, 0x27 },
                new byte[] { 0x13, 0x41, 0x13, 0x0d, 0xd8, 0xd6, 0x23, 0x12 },
                new byte[] { 0x31, 0x11, 0x08, 0x08, 0xfa, 0x9a, 0x22, 0x02 },
                new byte[] { 0x31, 0x61, 0x18, 0x07, 0x78, 0x64, 0x30, 0x27 },
                new byte[] { 0x22, 0x21, 0x1e, 0x06, 0xf0, 0x76, 0x08, 0x28 },
                new byte[] { 0x02, 0x01, 0x06, 0x00, 0xf0, 0xf2, 0x03, 0xf5 },
                new byte[] { 0x21, 0x61, 0x1d, 0x07, 0x82, 0x81, 0x16, 0x07 },
                new byte[] { 0x23, 0x21, 0x1a, 0x17, 0xcf, 0x72, 0x25, 0x17 },
                new byte[] { 0x15, 0x11, 0x25, 0x00, 0x4f, 0x71, 0x00, 0x11 },
                new byte[] { 0x85, 0x01, 0x12, 0x0f, 0x99, 0xa2, 0x40, 0x02 },
                new byte[] { 0x07, 0xc1, 0x69, 0x07, 0xf3, 0xf5, 0xa7, 0x12 },
                new byte[] { 0x71, 0x23, 0x0d, 0x06, 0x66, 0x75, 0x23, 0x16 },
                new byte[] { 0x01, 0x02, 0xd3, 0x05, 0xa3, 0x92, 0xf7, 0x52 },
                new byte[] { 0x61, 0x63, 0x0c, 0x00, 0x94, 0xaf, 0x34, 0x06 },
                new byte[] { 0x21, 0x62, 0x0d, 0x00, 0xb1, 0xa0, 0x54, 0x17 }
            };

            private static short[] adjustTable = new short[128];
            private static short[] db2Linear = new short[512 * 2];
            private static short[,] waveform = new short[2, 512];
            private static int[] amtable = new int[256];
            private static int[] pmtable = new int[256];
            private static int[,] ar = new int[16, 16];
            private static int[,] dr = new int[16, 16];
            private static int[,,] deltaTable = new int[512, 8, 16];
            private static int[,,] slTable = new int[2, 8, 2];
            private static int[,,,] tlTable = new int[16, 8, 64, 4];

            private Channel[] channels = new Channel[6];
            private Generator am = new Generator();
            private Generator pm = new Generator();
            private int addr;

            public Sound()
            {
                for (var i = 0; i < 6; i++)
                {
                    channels[i] = new Channel();
                }

                MakeTables();

                Reset();
            }

            private static short Linear2Db(double linear)
            {
                return (short)((linear == 0) ? 255 : Math.Min(-(int)(20.0 * Math.Log10(linear) / 0.1875), 255));
            }

            private static void MakeAdjustTable()
            {
                adjustTable[0] = 128;

                for (var i = 1; i < 128; i++)
                {
                    adjustTable[i] = (short)(128 - 1 - 128 * Math.Log(i) / Math.Log(128));
                }
            }

            private static void MakeDb2LinTable()
            {
                for (var i = 0; i < 512; i++)
                {
                    db2Linear[i] = (short)(Math.Pow(10, -i * 0.1875 / 20) * 255);

                    if (i >= 256)
                        db2Linear[i] = 0;

                    db2Linear[i + 512] = (short)(-db2Linear[i]);
                }
            }

            private static void MakeSinTable()
            {
                for (var i = 0x000; i < 0x080; i++) waveform[0, i] = Linear2Db(Math.Sin(MathHelper.Tau * i / 512));
                for (var i = 0x000; i < 0x080; i++) waveform[0, 255 - i] = waveform[0, i];
                for (var i = 0x000; i < 0x100; i++) waveform[0, 256 + i] = (short)(waveform[0, i] + 512);
                for (var i = 0x000; i < 0x100; i++) waveform[1, i] = waveform[0, i];
                for (var i = 0x100; i < 0x200; i++) waveform[1, i] = waveform[0, 0];
            }

            private static void MakeAmTable()
            {
                for (var i = 0; i < 256; i++)
                    amtable[i] = (int)(4.875 / 2 / 0.1875 * (1 + Math.Sin(MathHelper.Tau * i / 256)));
            }

            private static void MakePmTable()
            {
                for (var i = 0; i < 256; i++)
                    pmtable[i] = (int)(256 * Math.Pow(2, Math.Sin(MathHelper.Tau * i / 256) * 13.75 / 1200));
            }

            private static void MakeDeltaTable()
            {
                var lut = new[]
                {
                    0x01, 0x02, 0x04, 0x06,
                    0x08, 0x0a, 0x0c, 0x0e,
                    0x10, 0x12, 0x14, 0x14,
                    0x18, 0x18, 0x1e, 0x1e
                };

                for (var f = 0; f < 512; f++)
                {
                    for (var b = 0; b < 8; b++)
                    {
                        for (var m = 0; m < 16; m++)
                        {
                            deltaTable[f, b, m] = (((f * lut[m]) << b) >> 2) * CLOCK_DIV / RATE;
                        }
                    }
                }
            }

            private static void MakeRateTable()
            {
                for (var i = 0; i < 16; i++)
                {
                    for (var j = 0; j < 16; j++)
                    {
                        var rm = i + (j >> 2);
                        var rl = j & 3;

                        if (rm > 15)
                            rm = 15;

                        switch (i)
                        {
                        case 0:
                            ar[i, j] = 0;
                            dr[i, j] = 0;
                            break;

                        case 15:
                            ar[i, j] = 0;
                            dr[i, j] = ((1 * (rl + 4) << (rm - 1)) * CLOCK_DIV) / RATE;
                            break;

                        default:
                            ar[i, j] = ((3 * (rl + 4) << (rm + 1)) * CLOCK_DIV) / RATE;
                            dr[i, j] = ((1 * (rl + 4) << (rm - 1)) * CLOCK_DIV) / RATE;
                            break;
                        }
                    }
                }
            }

            private static void MakeSlTable()
            {
                for (var f = 0; f < 2; f++)
                {
                    for (var b = 0; b < 8; b++)
                    {
                        slTable[f, b, 0] = (b >> 1);
                        slTable[f, b, 1] = (b << 1) + f;
                    }
                }
            }

            private static void MakeTlTable()
            {
                var lut = new[]
                {
                        0, 18000, 24000, 27750,
                    30000, 32250, 33750, 35250,
                    36000, 37500, 38250, 39000,
                    39750, 40500, 41250, 42000
                };

                for (var f = 0; f < 16; f++)
                {
                    for (var b = 0; b < 8; b++)
                    {
                        for (var t = 0; t < 64; t++)
                        {
                            tlTable[f, b, t, 0] = (t * 2);

                            for (var k = 1; k < 4; k++)
                            {
                                var tmp = (lut[f] - 6000 * (b ^ 7)) / 1000;

                                if (tmp > 0)
                                {
                                    tlTable[f, b, t, k] = (t * 2) + ((tmp >> (k ^ 3)) * 8 / 3);
                                }
                                else
                                {
                                    tlTable[f, b, t, k] = (t * 2);
                                }
                            }
                        }
                    }
                }
            }

            private static void MakeTables()
            {
                MakeAmTable();
                MakePmTable();
                MakeTlTable();
                MakeSlTable();
                MakeSinTable();
                MakeRateTable();
                MakeDeltaTable();
                MakeAdjustTable();
                MakeDb2LinTable();
            }

            public void WriteAddr(byte data)
            {
                addr = data;
            }

            public void WriteData(byte data)
            {
                if (addr < 8)
                    instruments[0][addr] = data;

                switch (addr)
                {
                case 0x00: foreach (var channel in channels.Where(o => o.Patch == 0)) { channel.Update(0); } break;
                case 0x01: foreach (var channel in channels.Where(o => o.Patch == 0)) { channel.Update(1); } break;
                case 0x02: foreach (var channel in channels.Where(o => o.Patch == 0)) { channel.UpdateTl(0); } break;
                case 0x03: break;
                case 0x04: foreach (var channel in channels.Where(o => o.Patch == 0)) { channel.UpdateEg(0); } break;
                case 0x05: foreach (var channel in channels.Where(o => o.Patch == 0)) { channel.UpdateEg(1); } break;
                case 0x06: foreach (var channel in channels.Where(o => o.Patch == 0)) { channel.UpdateEg(0); } break;
                case 0x07: foreach (var channel in channels.Where(o => o.Patch == 0)) { channel.UpdateEg(1); } break;

                case 0x10:
                case 0x11:
                case 0x12:
                case 0x13:
                case 0x14:
                case 0x15: channels[addr & 7].WriteReg0(data); break;
                case 0x20:
                case 0x21:
                case 0x22:
                case 0x23:
                case 0x24:
                case 0x25: channels[addr & 7].WriteReg1(data); break;
                case 0x30:
                case 0x31:
                case 0x32:
                case 0x33:
                case 0x34:
                case 0x35: channels[addr & 7].WriteReg2(data); break;
                }
            }

            public override short Render()
            {
                am.Count = (am.Count + AM_DELTA) & 0xffff;
                pm.Count = (pm.Count + PM_DELTA) & 0xffff;
                am.Level = amtable[am.Count >> 8];
                pm.Level = pmtable[pm.Count >> 8];

                var output = 0;

                foreach (var channel in channels)
                {
                    channel.CalculatePg(pm.Level);
                    channel.CalculateEg(am.Level);

                    if (channel.Slots[1].EgState != State.Finish)
                    {
                        output += channel.GetSample();
                    }
                }

                return (short)(output << 4);
            }

            private void Reset()
            {
                am.Count = 0;
                pm.Count = 0;

                foreach (var channel in channels)
                {
                    channel.Slots[0] = new Slot();
                    channel.Slots[1] = new Slot();
                }

                for (addr = 0; addr < 0x40; addr++)
                {
                    WriteData(0);
                }

                addr = 0;
            }

            private class Channel
            {
                private bool sustain;
                private byte[] instrument;
                private int block;
                private int feedback;
                private int frequency;
                private int key;
                private int sound;

                public Slot[] Slots = new Slot[2];
                public int Patch;

                public Channel()
                {
                    Slots[0] = new Slot();
                    Slots[1] = new Slot();

                    instrument = instruments[0];
                }

                public void CalculateEg(int lfo)
                {
                    for (var i = 0; i < 2; i++)
                    {
                        var slot = Slots[i];

                        int egout;

                        switch (slot.EgState)
                        {
                        case State.A:
                            egout = adjustTable[slot.Eg.Count >> 15];
                            slot.Eg.Count += slot.Eg.Delta;

                            if ((slot.Eg.Count & (1 << 22)) != 0 || ((instrument[4 + i] >> 4) == 15))
                            {
                                egout = 0;
                                slot.Eg.Count = 0;
                                slot.EgState = State.D;
                                UpdateEg(i);
                            }
                            break;

                        case State.D:
                            egout = slot.Eg.Count >> 15;
                            slot.Eg.Count += slot.Eg.Delta;

                            var sl = (instrument[6 + i] >> 4) << 18;

                            if (slot.Eg.Count >= sl)
                            {
                                slot.Eg.Count = sl;
                                slot.EgState = (instrument[0 + i] & 0x20) != 0 ? State.H : State.S;
                                UpdateEg(i);
                            }
                            break;

                        case State.H:
                            egout = slot.Eg.Count >> 15;

                            if ((instrument[0 + i] & 0x20) == 0)
                            {
                                slot.EgState = State.S;
                                UpdateEg(i);
                            }
                            break;

                        case State.S:
                        case State.R:
                            egout = slot.Eg.Count >> 15;
                            slot.Eg.Count += slot.Eg.Delta;

                            if (egout >= (1 << 7))
                            {
                                slot.EgState = State.Finish;
                                egout = (1 << 7) - 1;
                            }
                            break;

                        case State.Settle:
                            egout = slot.Eg.Count >> 15;
                            slot.Eg.Count += slot.Eg.Delta;

                            if (egout >= (1 << 7))
                            {
                                slot.EgState = State.A;
                                egout = (1 << 7) - 1;
                                UpdateEg(i);
                            }
                            break;

                        default: egout = 0x7F; break;
                        }

                        if ((instrument[0 + i] & 0x80) != 0)
                            egout = ((egout + slot.Tl) * 2) + lfo;
                        else
                            egout = ((egout + slot.Tl) * 2);

                        if (egout > 0xFF)
                            egout = 0xFF;

                        slot.Eg.Level = egout;
                    }
                }

                public void CalculatePg(int lfo)
                {
                    for (var i = 0; i < 2; i++)
                    {
                        var slot = Slots[i];

                        if ((instrument[0 + i] & 0x40) != 0)
                            slot.Pg.Count += (slot.Pg.Delta * lfo) >> 8;
                        else
                            slot.Pg.Count += (slot.Pg.Delta);

                        slot.Pg.Count &= 0x3FFFF;
                        slot.Pg.Level = slot.Pg.Count >> 9;
                    }
                }

                public int GetSample()
                {
                    var slot = Slots[0];

                    if (slot.Eg.Level >= 0xFF)
                    {
                        slot.Output[0] = 0;
                    }
                    else if ((instrument[3] & 7) != 0)
                    {
                        var fm = (feedback << 2) >> (7 - (instrument[3] & 7));
                        slot.Output[0] = db2Linear[waveform[(instrument[3] >> 3) & 1, (slot.Pg.Level + fm) & 0x1FF] + slot.Eg.Level];
                    }
                    else
                    {
                        slot.Output[0] = db2Linear[waveform[(instrument[3] >> 3) & 1, (slot.Pg.Level)] + slot.Eg.Level];
                    }

                    feedback = (slot.Output[1] + slot.Output[0]) / 2;
                    slot.Output[1] = slot.Output[0];

                    slot = Slots[1];

                    if ((slot.Eg.Level & 0x7fffffff) >= 0xff)
                    {
                        slot.Output[0] = 0;
                    }
                    else
                    {
                        slot.Output[0] = db2Linear[waveform[(instrument[3] >> 4) & 1, (slot.Pg.Level + (feedback << 3)) & 0x1FF] + slot.Eg.Level];
                    }

                    return slot.Output[1] = (slot.Output[1] + slot.Output[0]) / 2;
                }

                private void UpdatePg(int i)
                {
                    Slots[i].Pg.Delta = deltaTable[frequency, block, instrument[0 + i] & 15];
                }

                public void UpdateTl(int i)
                {
                    Slots[i].Tl = tlTable[frequency >> 5, block, i == 0 ? (instrument[2] & 63) : sound, (instrument[2 + i] >> 6) & 3];
                }

                private void UpdateSl(int i)
                {
                    Slots[i].Sl = slTable[frequency >> 8, block, (instrument[0 + i] >> 4) & 1];
                }

                public void UpdateEg(int i)
                {
                    var slot = Slots[i];

                    switch (slot.EgState)
                    {
                    case State.A: slot.Eg.Delta = ar[instrument[4 + i] >> 4, slot.Sl]; break;
                    case State.D: slot.Eg.Delta = dr[instrument[4 + i] & 15, slot.Sl]; break;
                    case State.H: slot.Eg.Delta = 0; break;
                    case State.S: slot.Eg.Delta = dr[instrument[6 + i] & 15, slot.Sl]; break;
                    case State.R:
                        if (sustain)
                            slot.Eg.Delta = dr[5, slot.Sl];
                        else if ((instrument[0 + i] & 0x20) != 0)
                            slot.Eg.Delta = dr[instrument[6 + i] & 15, slot.Sl];
                        else
                            slot.Eg.Delta = dr[7, slot.Sl];
                        break;

                    case State.Settle: slot.Eg.Delta = dr[15, 0]; break;
                    default: slot.Eg.Delta = 0; break;
                    }
                }

                private void Update()
                {
                    Update(0);
                    Update(1);
                }

                public void Update(int s)
                {
                    UpdatePg(s);
                    UpdateTl(s);
                    UpdateSl(s);
                    UpdateEg(s); // EG should be updated last
                }

                public void WriteReg0(byte data)
                {
                    frequency = (frequency & 0x100) | (data << 0 & 0x0FF);

                    Update();
                }

                public void WriteReg1(byte data)
                {
                    frequency = (frequency & 0x0FF) | (data << 8 & 0x100);
                    block = (data & 0x0E) >> 1;
                    sustain = (data & 0x20) != 0;

                    if (key != (data & 0x10))
                    {
                        key = (data & 0x10);

                        if (key != 0)
                        {
                            for (var i = 0; i < 2; i++)
                            {
                                Slots[i].EgState = State.A;
                                Slots[i].Eg.Count = 0;
                                Slots[i].Pg.Count = 0;

                                UpdateEg(i);
                            }
                        }
                        else
                        {
                            if (Slots[1].EgState == State.A)
                                Slots[1].Eg.Count = adjustTable[Slots[1].Eg.Count >> 15] << 15;

                            Slots[1].EgState = State.R;

                            UpdateEg(1);
                        }
                    }

                    Update();
                }

                public void WriteReg2(byte data)
                {
                    Patch = (data >> 4) & 0x0F;
                    sound = (data << 2) & 0x3C;

                    instrument = instruments[Patch];

                    Update();
                }
            }

            private class Generator
            {
                public int Count;
                public int Delta;
                public int Level;
            }

            private class Slot
            {
                public Generator Eg = new Generator { Count = 1 << 22 };
                public Generator Pg = new Generator();
                public State EgState; // Current state
                public int Sl;
                public int Tl;
                public int[] Output = new int[2]; // Output value of slot
            }

            private enum State
            {
                Finish,
                A,
                D,
                H,
                S,
                R,
                Settle
            }
        }
    }
}
