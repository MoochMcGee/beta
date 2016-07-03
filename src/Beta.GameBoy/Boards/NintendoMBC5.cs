namespace Beta.GameBoy.Boards
{
    public class NintendoMbc5 : Board
    {
        private bool ramEnabled;
        private int ramPage;
        private int romPage;

        public NintendoMbc5(byte[] rom)
            : base(rom)
        {
        }

        private byte Read_0000_3FFF(ushort address)
        {
            return Rom[address & 0x3FFF];
        }

        private byte Read_4000_7FFF(ushort address)
        {
            return Rom[((address & 0x3FFF) | romPage) & RomMask];
        }

        private byte Read_A000_BFFF(ushort address)
        {
            if (!ramEnabled)
            {
                return 0;
            }

            return Ram[((address & 0x1FFF) | ramPage) & RamMask];
        }

        private void Write_0000_1FFF(ushort address, byte data)
        {
            ramEnabled = (data == 0x0A);
        }

        private void Write_2000_2FFF(ushort address, byte data)
        {
            romPage = (data & 0xFF) << 14;
        }

        private void Write_3000_3FFF(ushort address, byte data)
        {
            romPage |= (data & 0x01) << 22;
        }

        private void Write_4000_5FFF(ushort address, byte data)
        {
            ramPage = (data & 0x0F) << 13;
        }

        private void Write_6000_7FFF(ushort address, byte data)
        {
        }

        private void Write_A000_BFFF(ushort address, byte data)
        {
            if (!ramEnabled)
            {
                return;
            }

            Ram[((address & 0x1FFF) | ramPage) & RamMask] = data;
        }

        protected override void SetRamSize(byte value)
        {
            switch (value)
            {
            case 0: Ram = null; RamMask = 0; break;
            case 1: Ram = new byte[0x02000]; RamMask = 0x01FFF; break;
            case 2: Ram = new byte[0x08000]; RamMask = 0x07FFF; break;
            case 3: Ram = new byte[0x20000]; RamMask = 0x1FFFF; break;
            }
        }

        public override byte Read(ushort address)
        {
            if (address >= 0x0000 && address <= 0x3FFF) return Read_0000_3FFF(address);
            if (address >= 0x4000 && address <= 0x7FFF) return Read_4000_7FFF(address);
            if (address >= 0xA000 && address <= 0xBFFF) return Read_A000_BFFF(address);
            return 0xff;
        }

        public override void Write(ushort address, byte data)
        {
            if (address >= 0x0000 && address <= 0x1FFF) Write_0000_1FFF(address, data);
            if (address >= 0x2000 && address <= 0x2FFF) Write_2000_2FFF(address, data);
            if (address >= 0x3000 && address <= 0x3FFF) Write_3000_3FFF(address, data);
            if (address >= 0x4000 && address <= 0x5FFF) Write_4000_5FFF(address, data);
            if (address >= 0x6000 && address <= 0x7FFF) Write_6000_7FFF(address, data);
            if (address >= 0xA000 && address <= 0xBFFF) Write_A000_BFFF(address, data);
        }
    }
}
