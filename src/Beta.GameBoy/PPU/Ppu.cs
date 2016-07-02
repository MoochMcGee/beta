using Beta.GameBoy.CPU;
using Beta.GameBoy.Messaging;
using Beta.Platform;
using Beta.Platform.Core;
using Beta.Platform.Messaging;
using Beta.Platform.Processors;
using Beta.Platform.Video;

namespace Beta.GameBoy.PPU
{
    public sealed class Ppu : Processor, IConsumer<ClockSignal>
    {
        private const int PRIORITY_CLR = 0;
        private const int PRIORITY_BKG = 1;
        private const int PRIORITY_SPR = 2;

        private const int MODE_0 = 204;
        private const int MODE_1 = (MODE_2 + MODE_3 + MODE_0) * 10;
        private const int MODE_2 = 80;
        private const int MODE_3 = 172;

        private const int SPRITE_SEQ = 0x2;
        private const int ACTIVE_SEQ = 0x3;
        private const int H_BLANK_SEQ = 0x0;
        private const int V_BLANK_SEQ = 0x1;

        private const int H_BLANK_INT = 0x08;
        private const int V_BLANK_INT = 0x10;
        private const int SPRITE_INT = 0x20;
        private const int V_CHECK_INT = 0x40;

        private static int[] colourTable = new[]
        {
            // 196, 207, 161 = c4cfa1
            // 139, 149, 109 = 8b956d
            //  77,  83,  60 = 4d533c
            //  31,  31,  31 = 1f1f1f

            0xc4cfa1,
            0x8b956d,
            0x4d533c,
            0x1f1f1f
        };

        private static int[] timingTable = new[]
        {
            MODE_0,
            MODE_1,
            MODE_2,
            MODE_3
        };

        private readonly IAddressSpace addressSpace;
        private readonly IProducer<FrameSignal> frame;
        private readonly IProducer<InterruptSignal> interrupt;
        private readonly IVideoBackend video;

        private bool bgEnabled;
        private bool lcdEnabled;
        private bool spEnabled;
        private bool wnEnabled;
        private byte scx, scy;
        private byte wnx, wny;
        private int bgChAddr = 0x1000;
        private int bgNtAddr = 0x1800;
        private int wnNtAddr = 0x1800;
        private int spRasters = 8;
        private int control;
        private int hclock;
        private int vclock;
        private int vcheck;
        private int sequence = SPRITE_SEQ;
        private int sequenceDelay = timingTable[SPRITE_SEQ];
        private int sequenceTimer;
        private int[] priority = new int[160];
        private int[] raster;
        private int[][] bgPalettes = Utility.CreateArray<int>(2, 4);
        private int[][] spPalettes = Utility.CreateArray<int>(2, 4);

        private byte[] registers = new byte[12];
        private byte[] oram = new byte[0x00a0];
        private byte[] vram = new byte[0x2000];

        public Ppu(IAddressSpace addressSpace, IProducer<FrameSignal> frame, IProducer<InterruptSignal> interrupt, IVideoBackend video)
        {
            this.frame = frame;
            this.addressSpace = addressSpace;
            this.interrupt = interrupt;
            this.video = video;

            addressSpace.Map(0x8000, 0x9fff, ReadVRam, WriteVRam);
            addressSpace.Map(0xfe00, 0xfe9f, ReadORam, WriteORam);

            addressSpace.Map(0xff40, ReadFF40, WriteFF40);
            addressSpace.Map(0xff41, ReadFF41, WriteFF41);
            addressSpace.Map(0xff42, ReadFF42, WriteFF42);
            addressSpace.Map(0xff43, ReadFF43, WriteFF43);
            addressSpace.Map(0xff44, ReadFF44, WriteFF44);
            addressSpace.Map(0xff45, ReadFF45, WriteFF45);
            addressSpace.Map(0xff46, ReadFF46, WriteFF46);
            addressSpace.Map(0xff47, ReadFF47, WriteFF47);
            addressSpace.Map(0xff48, ReadFF48, WriteFF48);
            addressSpace.Map(0xff49, ReadFF49, WriteFF49);
            addressSpace.Map(0xff4a, ReadFF4A, WriteFF4A);
            addressSpace.Map(0xff4b, ReadFF4B, WriteFF4B);

            Single = 1;
        }

        private byte ReadORam(ushort address)
        {
            if (lcdEnabled && spEnabled && sequence >= SPRITE_SEQ)
                return 0xFF;

            return oram[address ^ 0xFE00];
        }

        private byte ReadVRam(ushort address)
        {
            if (lcdEnabled && bgEnabled && sequence == ACTIVE_SEQ)
                return 0xFF;

            return vram[address & 0x1FFF];
        }

        private void WriteORam(ushort address, byte data)
        {
            if (lcdEnabled && spEnabled && sequence >= SPRITE_SEQ)
                return;

            oram[address ^ 0xFE00] = data;
        }

        private void WriteVRam(ushort address, byte data)
        {
            if (lcdEnabled && bgEnabled && sequence == ACTIVE_SEQ)
                return;

            vram[address & 0x1FFF] = data;
        }

        private byte ReadFF40(ushort address)
        {
            return registers[0];
        }

        private byte ReadFF41(ushort address)
        {
            return (byte)(0x80 | control | (vclock == vcheck ? 0x04 : 0x00) | (sequence & 0x03));
        }

        private byte ReadFF42(ushort address)
        {
            return scy;
        }

        private byte ReadFF43(ushort address)
        {
            return scx;
        }

        private byte ReadFF44(ushort address)
        {
            return (byte)vclock;
        }

        private byte ReadFF45(ushort address)
        {
            return (byte)vcheck;
        }

        private byte ReadFF46(ushort address)
        {
            return registers[6];
        }

        private byte ReadFF47(ushort address)
        {
            return registers[7];
        }

        private byte ReadFF48(ushort address)
        {
            return registers[8];
        }

        private byte ReadFF49(ushort address)
        {
            return registers[9];
        }

        private byte ReadFF4A(ushort address)
        {
            return wny;
        }

        private byte ReadFF4B(ushort address)
        {
            return wnx;
        }

        private void WriteFF40(ushort address, byte data)
        {
            if ((registers[0] & 0x80) < (data & 0x80))
            {
                // lcd turning on
                hclock = 4;
                vclock = 0;

                sequence = SPRITE_SEQ;
                sequenceTimer = 4;
                sequenceDelay = timingTable[SPRITE_SEQ];
            }

            registers[0] = data;

            lcdEnabled = (data & 0x80) != 0; // Bit 7 - LCD Display Enable - (0=Off, 1=On)
            wnEnabled = (data & 0x20) != 0; // Bit 5 - Wnd Display Enable - (0=Off, 1=On)
            spRasters = (data & 0x04) != 0 ? 16 : 8;
            spEnabled = (data & 0x02) != 0; // Bit 1 - Spr Display Enable - (0=Off, 1=On)
            bgEnabled = (data & 0x01) != 0; // Bit 0 - Bkg Display Enable - (0=Off, 1=On)
            wnNtAddr = (data & 0x40) != 0 ? 0x1C00 : 0x1800; // Bit 6 - Wnd Tile Map Display Select    (0=9800-9BFF, 1=9C00-9FFF)
            bgChAddr = (data & 0x10) != 0 ? 0x0000 : 0x1000; // Bit 4 - Bkg & Wnd Tile Data Select     (0=8800-97FF, 1=8000-8FFF)
            bgNtAddr = (data & 0x08) != 0 ? 0x1C00 : 0x1800; // Bit 3 - Bkg Tile Map Display Select    (0=9800-9BFF, 1=9C00-9FFF)
        }

        private void WriteFF41(ushort address, byte data)
        {
            control = (data & 0x78);

            if (vclock == vcheck && (control & V_CHECK_INT) != 0)
            {
                Interrupt(Cpu.INT_STATUS);
            }
        }

        private void WriteFF42(ushort address, byte data)
        {
            scy = data;
        }

        private void WriteFF43(ushort address, byte data)
        {
            scx = data;
        }

        private void WriteFF44(ushort address, byte data)
        {
            hclock = 0x00;
            vclock = 0x00;

            ChangeSequence(SPRITE_SEQ);

            sequenceTimer = 0;
            sequenceDelay = timingTable[sequence];
        }

        private void WriteFF45(ushort address, byte data)
        {
            vcheck = data;

            if (vclock == vcheck && (control & V_CHECK_INT) != 0)
            {
                Interrupt(Cpu.INT_STATUS);
            }
        }

        private void WriteFF46(ushort address, byte data)
        {
            registers[6] = data;

            var addr = (ushort)(data << 8);

            for (var i = 0; i < 160; i++)
            {
                oram[i] = addressSpace.Read(addr++);
            }
        }

        private void WriteFF47(ushort address, byte data)
        {
            registers[7] = data;

            bgPalettes[0][0] = colourTable[data >> 0 & 0x03];
            bgPalettes[0][1] = colourTable[data >> 2 & 0x03];
            bgPalettes[0][2] = colourTable[data >> 4 & 0x03];
            bgPalettes[0][3] = colourTable[data >> 6 & 0x03];
        }

        private void WriteFF48(ushort address, byte data)
        {
            registers[8] = data;

            spPalettes[0][0] = colourTable[data >> 0 & 0x03];
            spPalettes[0][1] = colourTable[data >> 2 & 0x03];
            spPalettes[0][2] = colourTable[data >> 4 & 0x03];
            spPalettes[0][3] = colourTable[data >> 6 & 0x03];
        }

        private void WriteFF49(ushort address, byte data)
        {
            registers[9] = data;

            spPalettes[1][0] = colourTable[data >> 0 & 0x03];
            spPalettes[1][1] = colourTable[data >> 2 & 0x03];
            spPalettes[1][2] = colourTable[data >> 4 & 0x03];
            spPalettes[1][3] = colourTable[data >> 6 & 0x03];
        }

        private void WriteFF4A(ushort address, byte data)
        {
            wny = data;
        }

        private void WriteFF4B(ushort address, byte data)
        {
            wnx = data;
        }

        private void Interrupt(byte flag)
        {
            interrupt.Produce(new InterruptSignal(flag));
        }

        private void ChangeSequence(int sequence)
        {
            if (this.sequence == sequence)
                return;

            switch (sequence)
            {
            case H_BLANK_SEQ:
                if ((control & H_BLANK_INT) != 0)
                {
                    Interrupt(Cpu.INT_STATUS);
                }
                break;

            case V_BLANK_SEQ:
                if ((control & V_BLANK_INT) != 0)
                {
                    Interrupt(Cpu.INT_STATUS);
                }
                break;

            case SPRITE_SEQ:
                if ((control & SPRITE_INT) != 0)
                {
                    Interrupt(Cpu.INT_STATUS);
                }
                break;
            }

            this.sequence = sequence;
        }

        private void ClockSequencer()
        {
            sequenceTimer += Single;

            if (sequenceTimer == sequenceDelay)
            {
                // the sections labelled below represent the END of that section
                // example, when the case label for "Sprite" is hit, the controller is technically at the beginning of "Active"
                switch (sequence)
                {
                case H_BLANK_SEQ:
                    ChangeSequence(vclock == 144
                        ? V_BLANK_SEQ
                        : SPRITE_SEQ);
                    break;

                case V_BLANK_SEQ:
                    ChangeSequence(SPRITE_SEQ);
                    break;

                case SPRITE_SEQ:
                    ChangeSequence(ACTIVE_SEQ);
                    break;

                case ACTIVE_SEQ:
                    ChangeSequence(H_BLANK_SEQ);
                    break;
                }

                sequenceTimer = 0;
                sequenceDelay = timingTable[sequence];
            }
        }

        private void RenderScanline()
        {
            RenderBkg();

            if (spEnabled)
                RenderSpr();
        }

        private void RenderBkg()
        {
            var xPos = (scx) & 0xff;
            var yPos = (scy + vclock) & 0xff;
            var fine = scx & 7;
            var enabled = bgEnabled;

            var ntaddr = bgNtAddr | ((yPos & ~7) << 2) | ((xPos & ~7) >> 3);
            var px = 0;

            for (var tx = 0; tx < 21; tx++)
            {
                if (wnEnabled && tx == (wnx / 8) && vclock >= wny)
                {
                    xPos = wnx;
                    yPos = (vclock - wny) & 0xff;
                    fine = 0;

                    ntaddr = wnNtAddr | ((yPos & ~7) << 2);
                    enabled = wnEnabled;
                }

                if (!enabled)
                {
                    px++;
                    continue;
                }

                var name = vram[ntaddr];
                var chaddr = (name << 4) | ((yPos & 7) << 1);

                if ((name & 0x80) == 0)
                    chaddr |= bgChAddr;

                var palette = bgPalettes[0];
                var bit0 = vram[chaddr | 0];
                var bit1 = vram[chaddr | 1];

                for (var x = 0; x < 8; x++)
                {
                    var color = ((bit0 & 0x80) >> 7) | ((bit1 & 0x80) >> 6);
                    bit0 <<= 1;
                    bit1 <<= 1;

                    if (fine != 0)
                    {
                        fine--; // stall lcd, causing pixels to drop out
                        continue;
                    }

                    if (px < 160)
                    {
                        priority[px] = (color != 0) ? PRIORITY_BKG : PRIORITY_CLR;
                        raster[px++] = palette[color];
                    }
                }

                ntaddr = (ntaddr & 0xffe0) | ((ntaddr + 1) & 0x1f);
            }
        }

        private void RenderSpr()
        {
            var count = 0;

            for (var i = 0; i < 160 && count < 10; i += 4)
            {
                var yPos = oram[0x00 + i] - 16;
                var xPos = oram[0x01 + i] - 8;
                var name = oram[0x02 + i];
                var attr = oram[0x03 + i];

                var line = (vclock - yPos) & 0xFFFF;

                if (line < spRasters)
                {
                    count++;

                    if ((attr & 0x40) != 0)
                        line ^= 0x0F;

                    if (spRasters == 16)
                    {
                        name &= 0xFE;

                        if (line >= 8)
                            name |= 0x01;
                    }

                    var addr = (name << 4) | (line << 1 & 0x000E);
                    var bit0 = vram[addr | 0];
                    var bit1 = vram[addr | 1];

                    if ((attr & 0x20) != 0)
                    {
                        bit0 = Utility.ReverseLookup[bit0];
                        bit1 = Utility.ReverseLookup[bit1];
                    }

                    var palette = spPalettes[(attr >> 4) & 1];

                    for (var x = 0; x < 8 && xPos < 160; x++, xPos++, bit0 <<= 1, bit1 <<= 1)
                    {
                        if (xPos < 0 || priority[xPos] == PRIORITY_SPR)
                            continue;

                        var color = (bit0 >> 7 & 0x1) | (bit1 >> 6 & 0x2);

                        if (color != 0)
                        {
                            if ((attr & 0x80) != 0)
                            {
                                if (priority[xPos] == PRIORITY_CLR)
                                {
                                    priority[xPos] = PRIORITY_SPR;
                                    raster[xPos] = palette[color];
                                }
                            }
                            else
                            {
                                priority[xPos] = PRIORITY_SPR;
                                raster[xPos] = palette[color];
                            }
                        }
                    }
                }
            }
        }

        public void Consume(ClockSignal e)
        {
            Update(e.Cycles);
        }

        public override void Update()
        {
            hclock++;

            if (hclock == 252 && vclock < 144)
            {
                raster = video.GetRaster(vclock);

                if (lcdEnabled)
                {
                    RenderScanline();
                }
                else
                {
                    for (var i = 0; i < 160; i++)
                    {
                        raster[i] = colourTable[0];
                    }
                }
            }

            if (lcdEnabled)
            {
                ClockSequencer();
            }

            if (hclock == 456)
            {
                hclock = 0;
                vclock++;

                if (vclock == 154)
                {
                    vclock = 0;

                    frame.Produce(new FrameSignal());

                    video.Render();
                }

                if (lcdEnabled)
                {
                    if (vclock == 144)
                    {
                        Interrupt(Cpu.INT_VBLANK);
                    }

                    if (vclock == vcheck && (control & V_CHECK_INT) != 0)
                    {
                        Interrupt(Cpu.INT_STATUS);
                    }
                }
            }
        }
    }
}
