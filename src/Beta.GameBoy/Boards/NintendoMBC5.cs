namespace Beta.GameBoy.Boards
{
    public class NintendoMbc5 : Board
    {
        private bool ramEnabled;
        private int ramPage;
        private int romPage;

        public NintendoMbc5(IAddressSpace addressSpace, byte[] rom)
            : base(addressSpace, rom)
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

        protected override void DisableBios(ushort address, byte data)
        {
            AddressSpace.Map(0x0000, 0x00FF, Read_0000_3FFF, Write_0000_1FFF);
            AddressSpace.Map(0x0200, 0x08FF, Read_0000_3FFF, Write_0000_1FFF);
        }

        protected override void HookRam()
        {
            AddressSpace.Map(0xA000, 0xBFFF, Read_A000_BFFF, Write_A000_BFFF);
        }

        protected override void HookRom()
        {
            AddressSpace.Map(0x0000, 0x1FFF, Read_0000_3FFF, Write_0000_1FFF);
            AddressSpace.Map(0x2000, 0x2FFF, Read_0000_3FFF, Write_2000_2FFF);
            AddressSpace.Map(0x3000, 0x3FFF, Read_0000_3FFF, Write_3000_3FFF);
            AddressSpace.Map(0x4000, 0x5FFF, Read_4000_7FFF, Write_4000_5FFF);
            AddressSpace.Map(0x6000, 0x7FFF, Read_4000_7FFF, Write_6000_7FFF);
        }
    }
}
