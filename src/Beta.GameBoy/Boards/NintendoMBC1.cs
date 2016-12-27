namespace Beta.GameBoy.Boards
{
    public class NintendoMbc1 : Board
    {
        private bool romMode;
        private int ramPage;
        private int romPage = 1 << 14;

        public NintendoMbc1(byte[] rom)
            : base(rom)
        {
        }

        private byte Read_0000_3FFF(ushort address)
        {
            return Rom[address & 0x3fff];
        }

        private byte Read_4000_7FFF(ushort address)
        {
            if (romMode)
            {
                return Rom[((address & 0x3fff) | romPage | (ramPage << 6)) & RomMask];
            }
            else
            {
                return Rom[((address & 0x3fff) | romPage) & RomMask];
            }
        }

        private byte Read_A000_BFFF(ushort address)
        {
            return Ram[((address & 0x1fff) | ramPage) & RamMask];
        }

        private void Write_0000_1FFF(ushort address, byte data)
        {
        }

        private void Write_2000_3FFF(ushort address, byte data)
        {
            romPage = (data & 0x1f) << 14;

            if (romPage == 0)
            {
                romPage += 1 << 14;
            }
        }

        private void Write_4000_5FFF(ushort address, byte data)
        {
            ramPage = (data & 0x03) << 13;
        }

        private void Write_6000_7FFF(ushort address, byte data)
        {
            romMode = (data & 0x01) == 0;
        }

        private void Write_A000_BFFF(ushort address, byte data)
        {
            Ram[((address & 0x1fff) | ramPage) & RamMask] = data;
        }

        public override byte Read(ushort address)
        {
            if (address >= 0x0000 && address <= 0x3fff) return Read_0000_3FFF(address);
            if (address >= 0x4000 && address <= 0x7fff) return Read_4000_7FFF(address);
            if (address >= 0xa000 && address <= 0xbfff) return Read_A000_BFFF(address);
            return 0xff;
        }

        public override void Write(ushort address, byte data)
        {
            if (address >= 0x0000 && address <= 0x1fff) Write_0000_1FFF(address, data);
            if (address >= 0x2000 && address <= 0x3fff) Write_2000_3FFF(address, data);
            if (address >= 0x4000 && address <= 0x5fff) Write_4000_5FFF(address, data);
            if (address >= 0x6000 && address <= 0x7fff) Write_6000_7FFF(address, data);
            if (address >= 0xa000 && address <= 0xbfff) Write_A000_BFFF(address, data);
        }
    }
}
