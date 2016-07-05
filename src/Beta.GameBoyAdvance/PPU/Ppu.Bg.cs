using Beta.GameBoyAdvance.Memory;
using Beta.Platform;

namespace Beta.GameBoyAdvance.PPU
{
    public partial class Ppu
    {
        private class Bg : Layer
        {
            public static int MosaicH;
            public static int MosaicV;

            private readonly MMIO mmio;

            private Register16 controlRegister;
            private Register16 offsetXRegister;
            private Register16 offsetYRegister;

            // Affine Registers (Bg2, Bg3)
            private Register16 paRegister;

            private Register16 pbRegister;
            private Register16 pcRegister;
            private Register16 pdRegister;
            private Register32 rxRegister;
            private Register32 ryRegister;
            private bool mosaic;

            public bool Depth;
            public bool Wrap;
            public int ChrBase;
            public int NmtBase;
            public new int Priority;
            public int Size;
            public int Rx;
            public int Ry;

            public short Scx { get { return (short)(offsetXRegister.w & 0x1ff); } }
            public short Scy { get { return (short)(offsetYRegister.w & 0x1ff); } }
            public short Dx { get { return (short)paRegister.w; } }
            public short Dmx { get { return (short)pbRegister.w; } }
            public short Dy { get { return (short)pcRegister.w; } }
            public short Dmy { get { return (short)pdRegister.w; } }

            public Bg(MMIO mmio)
            {
                this.mmio = mmio;
            }

            #region Registers

            private byte ReadControl_0(uint address)
            {
                return controlRegister.l;
            }

            private byte ReadControl_1(uint address)
            {
                return controlRegister.h;
            }

            private void WriteControl_0(uint address, byte data)
            {
                controlRegister.l = data &= 0xCF;

                Priority = (data & 0x03);
                ChrBase = (data & 0x0C) >> 2;
                mosaic = (data & 0x40) != 0;
                Depth = (data & 0x80) != 0;
            }

            private void WriteControl_1(uint address, byte data)
            {
                controlRegister.h = data &= 0xFF;

                NmtBase = (data & 0x1F);
                Wrap = (data & 0x20) != 0;
                Size = (data & 0xC0) >> 6;
            }

            private void WriteScrollX_0(uint address, byte data)
            {
                offsetXRegister.l = data;
            }

            private void WriteScrollX_1(uint address, byte data)
            {
                offsetXRegister.h = data &= 0x01;
            }

            private void WriteScrollY_0(uint address, byte data)
            {
                offsetYRegister.l = data;
            }

            private void WriteScrollY_1(uint address, byte data)
            {
                offsetYRegister.h = data &= 0x01;
            }

            // Affine Registers (Bg2, Bg3)
            private void WritePA_0(uint address, byte data)
            {
                paRegister.l = data;
            }

            private void WritePA_1(uint address, byte data)
            {
                paRegister.h = data;
            }

            private void WritePB_0(uint address, byte data)
            {
                pbRegister.l = data;
            }

            private void WritePB_1(uint address, byte data)
            {
                pbRegister.h = data;
            }

            private void WritePC_0(uint address, byte data)
            {
                pcRegister.l = data;
            }

            private void WritePC_1(uint address, byte data)
            {
                pcRegister.h = data;
            }

            private void WritePD_0(uint address, byte data)
            {
                pdRegister.l = data;
            }

            private void WritePD_1(uint address, byte data)
            {
                pdRegister.h = data;
            }

            private void WriteRX_0(uint address, byte data)
            {
                rxRegister.ub0 = data;
            }

            private void WriteRX_1(uint address, byte data)
            {
                rxRegister.ub1 = data;
            }

            private void WriteRX_2(uint address, byte data)
            {
                rxRegister.ub2 = data;
            }

            private void WriteRX_3(uint address, byte data)
            {
                rxRegister.ub3 = data; Rx = (int)rxRegister.ud0;
            }

            private void WriteRY_0(uint address, byte data)
            {
                ryRegister.ub0 = data;
            }

            private void WriteRY_1(uint address, byte data)
            {
                ryRegister.ub1 = data;
            }

            private void WriteRY_2(uint address, byte data)
            {
                ryRegister.ub2 = data;
            }

            private void WriteRY_3(uint address, byte data)
            {
                ryRegister.ub3 = data; Ry = (int)ryRegister.ud0;
            }

            #endregion

            public void Initialize(uint index)
            {
                Index = (int)index;

                mmio.Map(0x008 + (index * 2), ReadControl_0, WriteControl_0);
                mmio.Map(0x009 + (index * 2), ReadControl_1, WriteControl_1);
                mmio.Map(0x010 + (index * 4), /*          */ WriteScrollX_0);
                mmio.Map(0x011 + (index * 4), /*          */ WriteScrollX_1);
                mmio.Map(0x012 + (index * 4), /*          */ WriteScrollY_0);
                mmio.Map(0x013 + (index * 4), /*          */ WriteScrollY_1);

                if (index >= 2)
                {
                    mmio.Map(0x020 + ((index - 2) * 16), WritePA_0);
                    mmio.Map(0x021 + ((index - 2) * 16), WritePA_1);
                    mmio.Map(0x022 + ((index - 2) * 16), WritePB_0);
                    mmio.Map(0x023 + ((index - 2) * 16), WritePB_1);
                    mmio.Map(0x024 + ((index - 2) * 16), WritePC_0);
                    mmio.Map(0x025 + ((index - 2) * 16), WritePC_1);
                    mmio.Map(0x026 + ((index - 2) * 16), WritePD_0);
                    mmio.Map(0x027 + ((index - 2) * 16), WritePD_1);
                    mmio.Map(0x028 + ((index - 2) * 16), WriteRX_0);
                    mmio.Map(0x029 + ((index - 2) * 16), WriteRX_1);
                    mmio.Map(0x02A + ((index - 2) * 16), WriteRX_2);
                    mmio.Map(0x02B + ((index - 2) * 16), WriteRX_3);
                    mmio.Map(0x02C + ((index - 2) * 16), WriteRY_0);
                    mmio.Map(0x02D + ((index - 2) * 16), WriteRY_1);
                    mmio.Map(0x02E + ((index - 2) * 16), WriteRY_2);
                    mmio.Map(0x02F + ((index - 2) * 16), WriteRY_3);
                }
            }

            public void ClockAffine()
            {
                Rx += Dmx;
                Ry += Dmy;
            }

            public void ResetAffine()
            {
                Rx = (int)rxRegister.ud0;
                Ry = (int)ryRegister.ud0;
            }
        }
    }
}
