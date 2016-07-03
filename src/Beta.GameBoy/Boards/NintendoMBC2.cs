namespace Beta.GameBoy.Boards
{
    public class NintendoMbc2 : Board
    {
        private int romPage = (1 << 14);

        public NintendoMbc2(byte[] rom)
            : base(rom)
        {
            Ram = new byte[0x200];
            RamMask = 0x1ff;
        }

        private byte Read_0000_3FFF(ushort address)
        {
            return Rom[address & 0x3fff];
        }

        private byte Read_4000_7FFF(ushort address)
        {
            return Rom[((address & 0x3fff) | romPage) & RomMask];
        }

        private byte Read_A000_BFFF(ushort address)
        {
            return Ram[address & 0x1ff];
        }

        private void Write_0000_1FFF(ushort address, byte data)
        {
            if ((address & 0x100) == 0)
            {
            }
        }

        private void Write_2000_3FFF(ushort address, byte data)
        {
            if ((address & 0x100) != 0)
            {
                romPage = (data & 0x1f) << 14;

                if (romPage == 0)
                {
                    romPage += (1 << 14);
                }
            }
        }

        private void Write_4000_5FFF(ushort address, byte data) { }

        private void Write_6000_7FFF(ushort address, byte data) { }

        private void Write_A000_BFFF(ushort address, byte data)
        {
            Ram[address & 0x1ff] = (byte)(data & 0x0f);
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
            if (address >= 0x2000 && address <= 0x3FFF) Write_2000_3FFF(address, data);
            if (address >= 0x4000 && address <= 0x5FFF) Write_4000_5FFF(address, data);
            if (address >= 0x6000 && address <= 0x7FFF) Write_6000_7FFF(address, data);
            if (address >= 0xA000 && address <= 0xBFFF) Write_A000_BFFF(address, data);
        }
    }
}
