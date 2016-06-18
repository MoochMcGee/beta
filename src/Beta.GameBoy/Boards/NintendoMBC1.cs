namespace Beta.GameBoy.Boards
{
    public class NintendoMbc1 : Board
    {
        private bool romMode;
        private uint ramPage;
        private uint romPage = (1 << 14);

        public NintendoMbc1(GameSystem gameboy, byte[] rom)
            : base(gameboy, rom)
        {
        }

        private byte Peek_0000_3FFF(uint address)
        {
            return Rom[address & 0x3fff];
        }

        private byte Peek_4000_7FFF(uint address)
        {
            if (romMode)
            {
                return Rom[((address & 0x3fff) | romPage | (ramPage << 6)) & RomMask];
            }

            return Rom[((address & 0x3fff) | romPage) & RomMask];
        }

        private void Poke_0000_1FFF(uint address, byte data)
        {
        }

        private void Poke_2000_3FFF(uint address, byte data)
        {
            romPage = (data & 0x1fu) << 14;

            if (romPage == 0)
            {
                romPage += (1 << 14);
            }
        }

        private void Poke_4000_5FFF(uint address, byte data)
        {
            ramPage = (data & 0x03u) << 13;
        }

        private void Poke_6000_7FFF(uint address, byte data)
        {
            romMode = (data & 0x01u) == 0;
        }

        protected override void DisableBios(uint address, byte data)
        {
            GameSystem.Hook(0x0000, 0x00ff, Peek_0000_3FFF, Poke_0000_1FFF);
            GameSystem.Hook(0x0200, 0x08ff, Peek_0000_3FFF, Poke_0000_1FFF);
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
