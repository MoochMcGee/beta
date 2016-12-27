using Beta.GameBoyAdvance.Memory;

namespace Beta.GameBoyAdvance.PPU
{
    public sealed class Bg : Layer
    {
        public static int MosaicH;
        public static int MosaicV;

        private readonly MMIO mmio;

        private ushort controlRegister;
        private ushort offsetXRegister;
        private ushort offsetYRegister;

        // Affine Registers (Bg2, Bg3)
        private ushort paRegister;
        private ushort pbRegister;
        private ushort pcRegister;
        private ushort pdRegister;
        private uint rxRegister;
        private uint ryRegister;
        private bool mosaic;

        public bool Depth;
        public bool Wrap;
        public int ChrBase;
        public int NmtBase;
        public new int Priority;
        public int Size;
        public int Rx;
        public int Ry;

        public short Scx { get { return (short)(offsetXRegister & 0x1ff); } }
        public short Scy { get { return (short)(offsetYRegister & 0x1ff); } }
        public short Dx { get { return (short)paRegister; } }
        public short Dmx { get { return (short)pbRegister; } }
        public short Dy { get { return (short)pcRegister; } }
        public short Dmy { get { return (short)pdRegister; } }

        public Bg(MMIO mmio)
        {
            this.mmio = mmio;
        }

        #region Registers

        private byte ReadControl_0(uint address)
        {
            return (byte)(controlRegister >> 0);
        }

        private byte ReadControl_1(uint address)
        {
            return (byte)(controlRegister >> 8);
        }

        private void WriteControl_0(uint address, byte data)
        {
            controlRegister &= 0xff00;
            controlRegister |= (ushort)(data & 0xcf);

            Priority = (data & 0x03);
            ChrBase = (data & 0x0c) >> 2;
            mosaic = (data & 0x40) != 0;
            Depth = (data & 0x80) != 0;
        }

        private void WriteControl_1(uint address, byte data)
        {
            controlRegister &= 0x00ff;
            controlRegister |= (ushort)(data << 8);

            NmtBase = (data & 0x1f);
            Wrap = (data & 0x20) != 0;
            Size = (data & 0xC0) >> 6;
        }

        private void WriteScrollX_0(uint address, byte data)
        {
            offsetXRegister &= 0x100;
            offsetXRegister |= data;
        }

        private void WriteScrollX_1(uint address, byte data)
        {
            offsetXRegister &= 0x0ff;
            offsetXRegister |= (ushort)((data << 8) & 0x100);
        }

        private void WriteScrollY_0(uint address, byte data)
        {
            offsetYRegister &= 0x100;
            offsetYRegister |= data;
        }

        private void WriteScrollY_1(uint address, byte data)
        {
            offsetYRegister &= 0x0ff;
            offsetYRegister |= (ushort)((data << 8) & 0x100);
        }

        // Affine Registers (Bg2, Bg3)
        private void WritePA_0(uint address, byte data)
        {
            paRegister &= 0xff00;
            paRegister |= data;
        }

        private void WritePA_1(uint address, byte data)
        {
            paRegister &= 0x00ff;
            paRegister |= (ushort)(data << 8);
        }

        private void WritePB_0(uint address, byte data)
        {
            pbRegister &= 0xff00;
            pbRegister |= data;
        }

        private void WritePB_1(uint address, byte data)
        {
            pbRegister &= 0x00ff;
            pbRegister |= (ushort)(data << 8);
        }

        private void WritePC_0(uint address, byte data)
        {
            pcRegister &= 0xff00;
            pcRegister |= data;
        }

        private void WritePC_1(uint address, byte data)
        {
            pcRegister &= 0x00ff;
            pcRegister |= (ushort)(data << 8);
        }

        private void WritePD_0(uint address, byte data)
        {
            pdRegister &= 0xff00;
            pdRegister |= data;
        }

        private void WritePD_1(uint address, byte data)
        {
            pdRegister &= 0x00ff;
            pdRegister |= (ushort)(data << 8);
        }

        private void WriteRX_0(uint address, byte data)
        {
            rxRegister &= 0xffffff00;
            rxRegister |= data;
        }

        private void WriteRX_1(uint address, byte data)
        {
            rxRegister &= 0xffff00ff;
            rxRegister |= (uint)(data << 8);
        }

        private void WriteRX_2(uint address, byte data)
        {
            rxRegister &= 0xff00ffff;
            rxRegister |= (uint)(data << 16);
        }

        private void WriteRX_3(uint address, byte data)
        {
            rxRegister &= 0x00ffffff;
            rxRegister |= (uint)(data << 24);
            Rx = (int)rxRegister;
        }

        private void WriteRY_0(uint address, byte data)
        {
            ryRegister &= 0xffffff00;
            ryRegister |= data;
        }

        private void WriteRY_1(uint address, byte data)
        {
            ryRegister &= 0xffff00ff;
            ryRegister |= (uint)(data << 8);
        }

        private void WriteRY_2(uint address, byte data)
        {
            ryRegister &= 0xff00ffff;
            ryRegister |= (uint)(data << 16);
        }

        private void WriteRY_3(uint address, byte data)
        {
            ryRegister &= 0x00ffffff;
            ryRegister |= (uint)(data << 24);
            Ry = (int)ryRegister;
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
            Rx = (int)rxRegister;
            Ry = (int)ryRegister;
        }
    }
}
