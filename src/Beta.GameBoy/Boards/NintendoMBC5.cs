namespace Beta.GameBoy.Boards
{
    public class NintendoMbc5 : Board
    {
        private bool ramEnabled;
        private uint ramPage;
        private uint romPage;

        public NintendoMbc5(GameSystem gameboy, byte[] rom)
            : base(gameboy, rom)
        {
        }

        private byte Peek_0000_3FFF(uint address)
        {
            return Rom[address & 0x3FFF];
        }

        private byte Peek_4000_7FFF(uint address)
        {
            return Rom[((address & 0x3FFF) | romPage) & RomMask];
        }

        private byte Peek_A000_BFFF(uint address)
        {
            if (!ramEnabled)
                return 0;

            return Ram[((address & 0x1FFF) | ramPage) & RamMask];
        }

        private void Poke_0000_1FFF(uint address, byte data)
        {
            ramEnabled = (data == 0x0AU);
        }

        private void Poke_2000_2FFF(uint address, byte data)
        {
            romPage = (data & 0xFFU) << 14;
        }

        private void Poke_3000_3FFF(uint address, byte data)
        {
            romPage |= (data & 0x01U) << 22;
        }

        private void Poke_4000_5FFF(uint address, byte data)
        {
            ramPage = (data & 0x0FU) << 13;
        }

        private void Poke_6000_7FFF(uint address, byte data)
        {
        }

        private void Poke_A000_BFFF(uint address, byte data)
        {
            if (!ramEnabled)
                return;

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

        protected override void DisableBios(uint address, byte data)
        {
            GameSystem.Hook(0x0000, 0x00FF, Peek_0000_3FFF, Poke_0000_1FFF);
            GameSystem.Hook(0x0200, 0x08FF, Peek_0000_3FFF, Poke_0000_1FFF);
        }

        protected override void HookRam()
        {
            GameSystem.Hook(0xA000, 0xBFFF, Peek_A000_BFFF, Poke_A000_BFFF);
        }

        protected override void HookRom()
        {
            GameSystem.Hook(0x0000, 0x1FFF, Peek_0000_3FFF, Poke_0000_1FFF);
            GameSystem.Hook(0x2000, 0x2FFF, Peek_0000_3FFF, Poke_2000_2FFF);
            GameSystem.Hook(0x3000, 0x3FFF, Peek_0000_3FFF, Poke_3000_3FFF);
            GameSystem.Hook(0x4000, 0x5FFF, Peek_4000_7FFF, Poke_4000_5FFF);
            GameSystem.Hook(0x6000, 0x7FFF, Peek_4000_7FFF, Poke_6000_7FFF);
        }
    }
}
