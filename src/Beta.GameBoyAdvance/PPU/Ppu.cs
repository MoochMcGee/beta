using System;
using Beta.GameBoyAdvance.CPU;
using Beta.Platform.Core;
using word = System.UInt32;

namespace Beta.GameBoyAdvance.PPU
{
    public partial class Ppu : Processor
    {
        private Driver gameSystem;
        private Cpu cpu;

        private Blend blend;
        private Bg bg0;
        private Bg bg1;
        private Bg bg2;
        private Bg bg3;
        private Sp spr;
        private Window window0 = new Window();
        private Window window1 = new Window();
        private bool displayFrameSelect;
        private bool forcedBlank;
        private bool hblankIntervalFree;
        private bool hblank, hblankIrq;
        private bool vblank, vblankIrq;
        private bool vmatch, vmatchIrq;
        private bool windowObjEnabled;
        private byte vcheck;
        private byte windowObjFlags;
        private byte windowOutFlags;
        private byte[] registers = new byte[256];
        private ushort hclock;
        private ushort vclock;
        private int bgMode;

        static Ppu()
        {
            colorLut = new int[32768];

            for (var i = 0; i < 32768; i++)
            {
                var r = (i << 3) & 0xf8;
                var g = (i >> 2) & 0xf8;
                var b = (i >> 7) & 0xf8;

                r = r | (r >> 5);
                g = g | (g >> 5);
                b = b | (b >> 5);

                var color = (r << 16) | (g << 8) | (b << 0);

                colorLut[i] = color;
            }
        }

        public Ppu(Driver gameSystem)
        {
            this.gameSystem = gameSystem;
            cpu = gameSystem.Cpu;

            Single = 4;

            bg0 = new Bg(gameSystem);
            bg1 = new Bg(gameSystem);
            bg2 = new Bg(gameSystem);
            bg3 = new Bg(gameSystem);
            spr = new Sp(gameSystem);
        }

        private void EnterHBlank()
        {
            hblank = true;

            bg2.ClockAffine();
            bg3.ClockAffine();

            if (hblankIrq)
            {
                cpu.Interrupt(Cpu.Source.H_BLANK);
            }

            if (vclock < 160)
            {
                cpu.Dma.HBlank();
                //if (cpu.dma[0].Enabled && cpu.dma[0].Type == Dma.HBlank) cpu.dma[0].Pending = true;
                //if (cpu.dma[1].Enabled && cpu.dma[1].Type == Dma.HBlank) cpu.dma[1].Pending = true;
                //if (cpu.dma[2].Enabled && cpu.dma[2].Type == Dma.HBlank) cpu.dma[2].Pending = true;
                //if (cpu.dma[3].Enabled && cpu.dma[3].Type == Dma.HBlank) cpu.dma[3].Pending = true;
            }
        }

        private void EnterVBlank()
        {
            vblank = true;

            if (vblankIrq)
            {
                cpu.Interrupt(Cpu.Source.V_BLANK);
            }

            cpu.Dma.VBlank();
            //if (cpu.dma[0].Enabled && cpu.dma[0].Type == Dma.VBlank) cpu.dma[0].Pending = true;
            //if (cpu.dma[1].Enabled && cpu.dma[1].Type == Dma.VBlank) cpu.dma[1].Pending = true;
            //if (cpu.dma[2].Enabled && cpu.dma[2].Type == Dma.VBlank) cpu.dma[2].Pending = true;
            //if (cpu.dma[3].Enabled && cpu.dma[3].Type == Dma.VBlank) cpu.dma[3].Pending = true;
        }

        private void LeaveHBlank()
        {
            hblank = false;
        }

        private void LeaveVBlank()
        {
            vblank = false;

            bg2.ResetAffine();
            bg3.ResetAffine();

            Pad.AutofireState = !Pad.AutofireState;

            gameSystem.Pad.Update();
            gameSystem.Video.Render();
        }

        private void RenderScanline(int[] raster)
        {
            if (forcedBlank)
            {
                // output white, don't render anything
                for (var i = 0; i < 240; i++)
                {
                    raster[i] = colorLut[0x7fff];
                }
            }
            else
            {
                if (window0.Enabled || window1.Enabled || windowObjEnabled)
                {
                    for (var i = 0; i < 240; i++)
                    {
                        window[i] = windowOutFlags;

                        spr.Enable[i] = false;
                        spr.Raster[i] = 0;
                        spr.Priority[i] = 5;
                    }

                    if (windowObjEnabled) RenderSpriteWindow();
                    if (window1.Enabled) window1.Calculate(window, vclock);
                    if (window0.Enabled) window0.Calculate(window, vclock);
                }
                else
                {
                    for (var i = 0; i < 240; i++)
                    {
                        window[i] = 0xff;

                        spr.Enable[i] = false;
                        spr.Raster[i] = 0;
                        spr.Priority[i] = 5;
                    }
                }

                switch (bgMode)
                {
                case 0: RenderMode0(raster); break;
                case 1: RenderMode1(raster); break;
                case 2: RenderMode2(raster); break;
                case 3: RenderMode3(raster); break;
                case 4: RenderMode4(raster); break;
                case 5: RenderMode5(raster); break;
                }
            }
        }

        private void UpdateHClock()
        {
            hclock++;

            if (hclock == 240)
            {
                EnterHBlank();
            }

            if (hclock == 308)
            {
                LeaveHBlank();
                hclock = 0;
                UpdateVClock();
            }
        }

        private void UpdateVClock()
        {
            vclock++;

            if (vclock == 160)
            {
                EnterVBlank();
            }

            if (vclock == 228)
            {
                LeaveVBlank();
                vclock = 0;
            }

            UpdateVCheck();
        }

        private void UpdateVCheck()
        {
            vmatch = (vclock == vcheck);

            if (vmatch && vmatchIrq)
            {
                cpu.Interrupt(Cpu.Source.V_CHECK);
            }
        }

        #region Registers

        private byte ReadReg(word address)
        {
            return registers[address & 0xff];
        }

        private byte Read004(word address)
        {
            var data = registers[0x04];

            if (vblank) data |= 0x01;
            if (hblank) data |= 0x02;
            if (vmatch) data |= 0x04;

            return data;
        }

        private byte Read005(word address)
        {
            return vcheck;
        }

        private byte Read006(word address)
        {
            return (byte)(vclock >> 0);
        }

        private byte Read007(word address)
        {
            return (byte)(vclock >> 8);
        }

        private void Write000(word address, byte data)
        {
            registers[0x00] = data;
            bgMode = (data & 0x07);
            //CgbMode = (data & 0x08) != 0; // gbc mode is really pointless here since we have a gbc emulator in development.
            displayFrameSelect = (data & 0x10) != 0;
            hblankIntervalFree = (data & 0x20) != 0;
            spr.Mapping = (data & 0x40) != 0;
            forcedBlank = (data & 0x80) != 0;
        }

        private void Write001(word address, byte data)
        {
            registers[0x01] = data;
            bg0.MasterEnable = (data & 0x01) != 0;
            bg1.MasterEnable = (data & 0x02) != 0;
            bg2.MasterEnable = (data & 0x04) != 0;
            bg3.MasterEnable = (data & 0x08) != 0;
            spr.MasterEnable = (data & 0x10) != 0;
            window0.Enabled = (data & 0x20) != 0;
            window1.Enabled = (data & 0x40) != 0;
            windowObjEnabled = (data & 0x80) != 0;
        }

        private void Write002(word address, byte data)
        {
            registers[0x02] = data &= 0x01;
        }

        private void Write003(word address, byte data)
        {
            registers[0x03] = 0;
        }

        private void Write004(word address, byte data)
        {
            registers[0x04] = data &= 0x38;
            vblankIrq = (data & 0x08) != 0;
            hblankIrq = (data & 0x10) != 0;
            vmatchIrq = (data & 0x20) != 0;
        }

        private void Write005(word address, byte data)
        {
            vcheck = data;
        }

        //           006-04B
        private void Write04C(word address, byte data)
        {
            Bg.MosaicH = (data >> 0) & 15;
            Bg.MosaicV = (data >> 4) & 15;
        }

        private void Write04D(word address, byte data)
        {
            Sp.MosaicH = (data >> 0) & 15;
            Sp.MosaicV = (data >> 4) & 15;
        }

        //           04E-04F
        private void Write050(word address, byte data)
        {
            registers[0x50] = data;
            blend.Target1 = (data & 0x3f);
            blend.Type = (data & 0xc0) >> 6;
        }

        private void Write051(word address, byte data)
        {
            registers[0x51] = data;
            blend.Target2 = (data & 0x3f);
        }

        private void Write052(word address, byte data)
        {
            registers[0x52] = data;
            blend.Eva = Math.Min(data & 31, 16);
        }

        private void Write053(word address, byte data)
        {
            registers[0x53] = data;
            blend.Evb = Math.Min(data & 31, 16);
        }

        private void Write054(word address, byte data)
        {
            registers[0x54] = data;
            blend.Evy = Math.Min(data & 31, 16);
        }

        //           055-05F

        #endregion

        public void Initialize()
        {
            bg0.Initialize(0u);
            bg1.Initialize(1u);
            bg2.Initialize(2u);
            bg3.Initialize(3u);

            var mmio = gameSystem.mmio;

            mmio.Map(0x000, ReadReg, Write000);
            mmio.Map(0x001, ReadReg, Write001);
            mmio.Map(0x002, ReadReg, Write002);
            mmio.Map(0x003, ReadReg, Write003);
            mmio.Map(0x004, Read004, Write004);
            mmio.Map(0x005, Read005, Write005);
            // vertical counter
            mmio.Map(0x006, Read006);
            mmio.Map(0x007, Read007);
            // window feature
            mmio.Map(0x040, (a, data) => window0.X2 = data);
            mmio.Map(0x041, (a, data) => window0.X1 = data);
            mmio.Map(0x042, (a, data) => window1.X2 = data);
            mmio.Map(0x043, (a, data) => window1.X1 = data);
            mmio.Map(0x044, (a, data) => window0.Y2 = data);
            mmio.Map(0x045, (a, data) => window0.Y1 = data);
            mmio.Map(0x046, (a, data) => window1.Y2 = data);
            mmio.Map(0x047, (a, data) => window1.Y1 = data);
            mmio.Map(0x048, a => window0.Flags, (a, data) => window0.Flags = data);
            mmio.Map(0x049, a => window1.Flags, (a, data) => window1.Flags = data);
            mmio.Map(0x04a, a => windowOutFlags, (a, data) => windowOutFlags = data);
            mmio.Map(0x04b, a => windowObjFlags, (a, data) => windowObjFlags = data);
            mmio.Map(0x04c, ReadReg, Write04C);
            mmio.Map(0x04d, ReadReg, Write04D);
            // 04e - 04f
            mmio.Map(0x050, ReadReg, Write050);
            mmio.Map(0x051, ReadReg, Write051);
            mmio.Map(0x052, ReadReg, Write052);
            mmio.Map(0x053, ReadReg, Write053);
            mmio.Map(0x054, ReadReg, Write054);
            // 054 - 05f
        }

        public override void Update()
        {
            UpdateHClock();

            if (hclock == 240 && vclock < 160)
            {
                RenderScanline(gameSystem.Video.GetRaster(vclock));
            }
        }

        private struct Blend
        {
            public int Eva;
            public int Evb;
            public int Evy;
            public int Target1;
            public int Target2;
            public int Type;
        }
    }
}
