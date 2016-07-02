namespace Beta.GameBoy.Boards
{
    public class NintendoMbc2 : Board
    {
        private int romPage = (1 << 14);

        public NintendoMbc2(IAddressSpace addressSpace, byte[] rom)
            : base(addressSpace, rom)
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

        private void Write_4000_5FFF(ushort address, byte data)
        {
        }

        private void Write_6000_7FFF(ushort address, byte data)
        {
        }

        private void Write_A000_BFFF(ushort address, byte data)
        {
            Ram[address & 0x1ff] = (byte)(data & 0x0f);
        }

        protected override void DisableBios(ushort address, byte data)
        {
            AddressSpace.Map(0x0000, 0x00ff, Read_0000_3FFF, Write_0000_1FFF);
            AddressSpace.Map(0x0200, 0x08ff, Read_0000_3FFF, Write_0000_1FFF);
        }

        protected override void HookRam()
        {
            AddressSpace.Map(0xA000, 0xbfff, Read_A000_BFFF, Write_A000_BFFF);
        }

        protected override void HookRom()
        {
            AddressSpace.Map(0x0000, 0x1fff, Read_0000_3FFF, Write_0000_1FFF);
            AddressSpace.Map(0x2000, 0x3fff, Read_0000_3FFF, Write_2000_3FFF);
            AddressSpace.Map(0x4000, 0x5fff, Read_4000_7FFF, Write_4000_5FFF);
            AddressSpace.Map(0x6000, 0x7fff, Read_4000_7FFF, Write_6000_7FFF);
        }
    }
}
