namespace Beta.Famicom.PPU
{
    public static class BgUnit
    {
        public static void PointName(R2C02State e)
        {
            e.fetch_address = 0x2000 | (e.scroll_address & 0xfff);
        }

        public static void PointAttr(R2C02State e)
        {
            var x = ((e.scroll_address >> 2) & 7);
            var y = ((e.scroll_address >> 4) & 0x38);

            e.fetch_address = 0x23c0 | (e.scroll_address & 0xc00) | y | x;
        }

        public static void PointBit0(R2C02State e)
        {
            var line = (e.scroll_address >> 12) & 7;

            e.fetch_address = e.bkg_address | (e.fetch_name << 4) | 0 | line;
        }

        public static void PointBit1(R2C02State e)
        {
            var line = (e.scroll_address >> 12) & 7;

            e.fetch_address = e.bkg_address | (e.fetch_name << 4) | 8 | line;
        }

        public static void FetchName(R2C02State e)
        {
            R2C02MemoryMap.Read(e.fetch_address, ref e.fetch_name);
        }

        public static void FetchAttr(R2C02State e)
        {
            R2C02MemoryMap.Read(e.fetch_address, ref e.fetch_attr);

            var x = (e.scroll_address >> 0) & 2;
            var y = (e.scroll_address >> 5) & 2;
            var shift = (y << 1) | x;

            e.fetch_attr = (byte)(e.fetch_attr >> shift);
        }

        public static void FetchBit0(R2C02State e)
        {
            R2C02MemoryMap.Read(e.fetch_address, ref e.fetch_bit0);
        }

        public static void FetchBit1(R2C02State e)
        {
            R2C02MemoryMap.Read(e.fetch_address, ref e.fetch_bit1);
        }

        public static void Synthesize(R2C02State e)
        {
            var offset = (e.h + 9) % 336;

            for (var i = 0; i < 8; i++)
            {
                e.bgPixel[offset + i] =
                    ((e.fetch_attr << 2) & 12) |
                    ((e.fetch_bit0 >> 7) & 1) |
                    ((e.fetch_bit1 >> 6) & 2);

                e.fetch_bit0 <<= 1;
                e.fetch_bit1 <<= 1;
            }
        }

        public static int GetPixel(R2C02State e)
        {
            if (e.bkg_enabled == false)
            {
                return 0;
            }

            if (e.bkg_clipped && e.h < 8)
            {
                return 0;
            }

            return e.bgPixel[e.h + e.scroll_fine];
        }
    }
}
