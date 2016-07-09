using Beta.GameBoy.CPU;
using Beta.GameBoy.Memory;
using Beta.GameBoy.Messaging;
using Beta.Platform;
using Beta.Platform.Core;
using Beta.Platform.Messaging;
using Beta.Platform.Video;

namespace Beta.GameBoy.PPU
{
    public sealed class Ppu : Processor, IConsumer<ClockSignal>
    {
        private const int PRIORITY_CLR = 0;
        private const int PRIORITY_BKG = 1;
        private const int PRIORITY_SPR = 2;

        private const int SPRITE_SEQ = 0x2;
        private const int ACTIVE_SEQ = 0x3;
        private const int HBLANK_SEQ = 0x0;
        private const int VBLANK_SEQ = 0x1;

        private const int HBLANK_INT = 0x08;
        private const int VBLANK_INT = 0x10;
        private const int SPRITE_INT = 0x20;
        private const int VCHECK_INT = 0x40;

        private static readonly int[] shadeTable = new[]
        {
            ColorHelper.FromRGB(196, 207, 161),
            ColorHelper.FromRGB(139, 149, 109),
            ColorHelper.FromRGB( 77,  83,  60),
            ColorHelper.FromRGB( 31,  31,  31)
        };

        private readonly PpuRegisters regs;
        private readonly MemoryMap memory;
        private readonly IProducer<FrameSignal> frame;
        private readonly IProducer<InterruptSignal> interrupt;
        private readonly IVideoBackend video;

        private int[] priority = new int[160];
        private int[] raster;

        public Ppu(
            Registers regs,
            MemoryMap memory,
            IProducer<FrameSignal> frame,
            IProducer<InterruptSignal> interrupt,
            IVideoBackend video)
        {
            this.regs = regs.ppu;
            this.memory = memory;
            this.frame = frame;
            this.interrupt = interrupt;
            this.video = video;

            Single = 1;
        }

        private void ChangeSequence(int sequence)
        {
            if (sequence == (regs.control & 3))
            {
                return;
            }

            if ((sequence == HBLANK_SEQ && (regs.control & HBLANK_INT) != 0) ||
                (sequence == VBLANK_SEQ && (regs.control & VBLANK_INT) != 0) ||
                (sequence == SPRITE_SEQ && (regs.control & SPRITE_INT) != 0))
            {
                Interrupt(Cpu.INT_STATUS);
            }

            regs.control &= ~3;
            regs.control |= sequence;
        }

        private void UpdateSequence()
        {
            if (regs.lcd_enabled == false)
            {
                return;
            }

            if (regs.v < 144)
            {
                if (regs.h < 80) { ChangeSequence(SPRITE_SEQ); }
                else if (regs.h < 252) { ChangeSequence(ACTIVE_SEQ); }
                else if (regs.h < 456) { ChangeSequence(HBLANK_SEQ); }
            }
            else
            {
                ChangeSequence(VBLANK_SEQ);
            }
        }

        private void RenderScanline()
        {
            if (regs.lcd_enabled)
            {
                if (regs.bkg_enabled)
                {
                    RenderBkg();
                }

                if (regs.wnd_enabled)
                {
                    RenderWnd();
                }

                if (regs.obj_enabled)
                {
                    RenderObj();
                }
            }
        }

        private void RenderBkg()
        {
            if (regs.bkg_enabled == false)
            {
                return;
            }

            var xPos = (regs.scroll_x) & 0xff;
            var yPos = (regs.scroll_y + regs.v) & 0xff;
            var fine = (regs.scroll_x) & 7;

            var ntaddr = regs.bkg_name_address | ((yPos & ~7) << 2) | ((xPos & ~7) >> 3);
            var px = 0;

            for (var tx = 0; tx < 21; tx++)
            {
                var name = memory.Read((ushort)(ntaddr));
                var chaddr = (regs.bkg_char_address == 0x9000 && (name & 0x80) == 0)
                    ? 0x9000 | (name << 4) | ((yPos & 7) << 1)
                    : 0x8000 | (name << 4) | ((yPos & 7) << 1)
                    ;

                var palette = regs.bkg_palette;
                var bit0 = memory.Read((ushort)(chaddr | 0));
                var bit1 = memory.Read((ushort)(chaddr | 1));

                bit0 <<= fine;
                bit1 <<= fine;

                for (var x = fine; x < 8; x++)
                {
                    var color = ((bit0 & 0x80) >> 7) | ((bit1 & 0x80) >> 6);
                    bit0 <<= 1;
                    bit1 <<= 1;

                    if (px < 160)
                    {
                        priority[px] = (color != 0) ? PRIORITY_BKG : PRIORITY_CLR;
                        raster[px++] = GetShade(palette, color);
                    }
                }

                fine = 0;

                ntaddr = (ntaddr & 0xffe0) | ((ntaddr + 1) & 0x1f);
            }
        }

        private void RenderWnd()
        {
            if (regs.wnd_enabled == false || regs.v < regs.window_y)
            {
                return;
            }

            var x = (regs.window_x - 7);
            var y = (regs.v - regs.window_y) & 0xff;

            var name_address = regs.wnd_name_address | ((y & ~7) << 2);

            var tx = (168 - x) / 8;

            for (var i = 0; i < tx; i++)
            {
                var name = memory.Read((ushort)(name_address));
                var char_address = (regs.bkg_char_address == 0x9000 && name < 0x80)
                    ? 0x9000 | (name << 4) | ((y & 7) << 1)
                    : 0x8000 | (name << 4) | ((y & 7) << 1)
                    ;

                var palette = regs.bkg_palette;
                var bit0 = memory.Read((ushort)(char_address | 0));
                var bit1 = memory.Read((ushort)(char_address | 1));

                for (var j = 0; j < 8; j++)
                {
                    var color = ((bit0 & 0x80) >> 7) | ((bit1 & 0x80) >> 6);
                    bit0 <<= 1;
                    bit1 <<= 1;

                    if (x >= 0 && x < 160)
                    {
                        priority[x] = PRIORITY_BKG;
                        raster[x] = GetShade(palette, color);
                    }

                    x++;
                }

                name_address = (name_address & 0xffe0) | ((name_address + 1) & 0x1f);
            }
        }

        private void RenderObj()
        {
            var count = 0;

            for (var i = 0; i < 160 && count < 10; i += 4)
            {
                var yPos = memory.Read((ushort)(0xfe00 + i + 0)) - 16;
                var xPos = memory.Read((ushort)(0xfe00 + i + 1)) - 8;
                var name = memory.Read((ushort)(0xfe00 + i + 2));
                var attr = memory.Read((ushort)(0xfe00 + i + 3));

                var line = (regs.v - yPos) & 0xFFFF;
                if (line < regs.obj_rasters)
                {
                    count++;

                    if ((attr & 0x40) != 0)
                        line ^= 0x0F;

                    if (regs.obj_rasters == 16)
                    {
                        name &= 0xFE;

                        if (line >= 8)
                            name |= 0x01;
                    }

                    var addr = 0x8000 | (name << 4) | ((line << 1) & 0x000E);
                    var bit0 = memory.Read((ushort)(addr | 0));
                    var bit1 = memory.Read((ushort)(addr | 1));

                    if ((attr & 0x20) != 0)
                    {
                        bit0 = Utility.ReverseLookup[bit0];
                        bit1 = Utility.ReverseLookup[bit1];
                    }

                    var palette = regs.obj_palette[(attr >> 4) & 1];

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
                                    raster[xPos] = GetShade(palette, color);
                                }
                            }
                            else
                            {
                                priority[xPos] = PRIORITY_SPR;
                                raster[xPos] = GetShade(palette, color);
                            }
                        }
                    }
                }
            }
        }

        private int GetShade(int palette, int color)
        {
            return shadeTable[(palette >> (color * 2)) & 3];
        }

        public void Consume(ClockSignal e)
        {
            Update(e.Cycles);
        }

        public override void Update()
        {
            if (regs.dma_triggered)
            {
                regs.dma_triggered = false;

                var dst_address = (ushort)(0xfe00);
                var src_address = (ushort)(regs.dma_segment << 8);

                for (var i = 0; i < 160; i++)
                {
                    var data = memory.Read(src_address);
                    memory.Write(dst_address, data);

                    dst_address++;
                    src_address++;
                }
            }

            regs.h++;

            if (regs.h == 252 && regs.v < 144)
            {
                raster = video.GetRaster(regs.v);

                if (regs.lcd_enabled)
                {
                    RenderScanline();
                }
                else
                {
                    for (var i = 0; i < 160; i++)
                    {
                        raster[i] = shadeTable[0];
                    }
                }
            }

            if (regs.lcd_enabled)
            {
                UpdateSequence();
            }

            if (regs.h == 456)
            {
                regs.h = 0;
                regs.v++;

                if (regs.v == 154)
                {
                    regs.v = 0;

                    frame.Produce(new FrameSignal());

                    video.Render();
                }

                if (regs.lcd_enabled)
                {
                    if (regs.v == 144)
                    {
                        Interrupt(Cpu.INT_VBLANK);
                    }

                    VCompare();
                }
            }
        }

        private void VCompare()
        {
            if (regs.v == regs.v_check)
            {
                regs.control |= 4;

                if ((regs.control & VCHECK_INT) != 0)
                {
                    Interrupt(Cpu.INT_STATUS);
                }
            }
            else
            {
                regs.control &= ~4;
            }
        }

        private void Interrupt(byte flag)
        {
            interrupt.Produce(new InterruptSignal(flag));
        }
    }
}
