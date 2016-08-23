namespace Beta.Famicom.PPU
{
    public sealed class BgUnit
    {
        private readonly R2C02MemoryMap memory;
        private readonly R2C02State state;
        private readonly int[] pixel;

        public BgUnit(R2C02MemoryMap memory, State state)
        {
            this.memory = memory;
            this.state = state.r2c02;
            this.pixel = new int[256 + 16];
        }

        public void FetchName()
        {
            memory.Read(state.fetch_address, ref state.fetch_name);
        }

        public void FetchAttr()
        {
            memory.Read(state.fetch_address, ref state.fetch_attr);

            var x = (state.scroll_address >> 0) & 2;
            var y = (state.scroll_address >> 5) & 2;
            var shift = (y << 1) | x;

            state.fetch_attr = (byte)(state.fetch_attr >> shift);
        }

        public void FetchBit0()
        {
            memory.Read(state.fetch_address, ref state.fetch_bit0);
        }

        public void FetchBit1()
        {
            memory.Read(state.fetch_address, ref state.fetch_bit1);
        }

        public void PointName()
        {
            state.fetch_address = 0x2000 | (state.scroll_address & 0xfff);
        }

        public void PointAttr()
        {
            var x = ((state.scroll_address >> 2) & 7);
            var y = ((state.scroll_address >> 4) & 0x38);

            state.fetch_address = 0x23c0 | (state.scroll_address & 0xc00) | y | x;
        }

        public void PointBit0()
        {
            var line = (state.scroll_address >> 12) & 7;

            state.fetch_address = state.bkg_address | (state.fetch_name << 4) | 0 | line;
        }

        public void PointBit1()
        {
            var line = (state.scroll_address >> 12) & 7;

            state.fetch_address = state.bkg_address | (state.fetch_name << 4) | 8 | line;
        }

        public void Synthesize()
        {
            var offset = (state.h + 9) % 336;

            for (var i = 0; i < 8; i++)
            {
                pixel[offset + i] =
                    ((state.fetch_attr << 2) & 12) |
                    ((state.fetch_bit0 >> 7) & 1) |
                    ((state.fetch_bit1 >> 6) & 2);

                state.fetch_bit0 <<= 1;
                state.fetch_bit1 <<= 1;
            }
        }

        public int GetPixel()
        {
            if (state.bkg_enabled == false)
            {
                return 0;
            }

            if (state.bkg_clipped && state.h < 8)
            {
                return 0;
            }

            return pixel[state.h + state.scroll_fine];
        }
    }
}
