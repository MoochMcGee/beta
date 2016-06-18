namespace Beta.GameBoy.Boards
{
    public class NintendoMbc2 : Board
    {
        private uint romPage = (1U << 14);

        public NintendoMbc2(GameSystem gameboy, byte[] rom)
            : base(gameboy, rom)
        {
            Ram = new byte[0x200];
            RamMask = 0x1ff;
        }

        private byte Peek_0000_3FFF(uint address)
        {
            return Rom[address & 0x3fff];
        }

        private byte Peek_4000_7FFF(uint address)
        {
            return Rom[((address & 0x3fff) | romPage) & RomMask];
        }

        private byte Peek_A000_BFFF(uint address)
        {
            return Ram[address & 0x1ff];
        }

        private void Poke_0000_1FFF(uint address, byte data)
        {
            if ((address & 0x100) == 0)
            {
            }
        }

        private void Poke_2000_3FFF(uint address, byte data)
        {
            if ((address & 0x100) != 0)
            {
                romPage = (data & 0x1fu) << 14;

                if (romPage == 0)
                    romPage += (1 << 14);
            }
        }

        private void Poke_4000_5FFF(uint address, byte data)
        {
        }

        private void Poke_6000_7FFF(uint address, byte data)
        {
        }

        private void Poke_A000_BFFF(uint address, byte data)
        {
            Ram[address & 0x1ff] = (byte)(data & 0x0f);
        }

        protected override void DisableBios(uint address, byte data)
        {
            GameSystem.Hook(0x0000, 0x00ff, Peek_0000_3FFF, Poke_0000_1FFF);
            GameSystem.Hook(0x0200, 0x08ff, Peek_0000_3FFF, Poke_0000_1FFF);
        }

        protected override void HookRam()
        {
            GameSystem.Hook(0xA000, 0xbfff, Peek_A000_BFFF, Poke_A000_BFFF);
        }

        protected override void HookRom()
        {
            GameSystem.Hook(0x0000, 0x1fff, Peek_0000_3FFF, Poke_0000_1FFF);
            GameSystem.Hook(0x2000, 0x3fff, Peek_0000_3FFF, Poke_2000_3FFF);
            GameSystem.Hook(0x4000, 0x5fff, Peek_4000_7FFF, Poke_4000_5FFF);
            GameSystem.Hook(0x6000, 0x7fff, Peek_4000_7FFF, Poke_6000_7FFF);
        }
    }
}
