namespace Beta.GameBoy.Boards
{
    public class NintendoMbc1 : Board
    {
        private bool romMode;
        private int ramPage;
        private int romPage = (1 << 14);

        public NintendoMbc1(IAddressSpace addressSpace, byte[] rom)
            : base(addressSpace, rom)
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

            return Rom[((address & 0x3fff) | romPage) & RomMask];
        }

        private void Write_0000_1FFF(ushort address, byte data)
        {
        }

        private void Write_2000_3FFF(ushort address, byte data)
        {
            romPage = (data & 0x1f) << 14;

            if (romPage == 0)
            {
                romPage += (1 << 14);
            }
        }

        private void Write_4000_5FFF(ushort address, byte data)
        {
            ramPage = (data & 0x03) << 13;
        }

        private void Write_6000_7FFF(ushort address, byte data)
        {
            romMode = (data & 0x01u) == 0;
        }

        protected override void DisableBios(ushort address, byte data)
        {
            AddressSpace.Map(0x0000, 0x00ff, Read_0000_3FFF, Write_0000_1FFF);
            AddressSpace.Map(0x0200, 0x08ff, Read_0000_3FFF, Write_0000_1FFF);
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
