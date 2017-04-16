namespace Beta.GameBoy
{
    public static class Tma
    {
        static readonly int[] lut = new[]
        {
            1 << 8, //   4,096 Hz
            1 << 2, // 262,144 Hz
            1 << 4, //  65,536 Hz
            1 << 6  //  16,384 Hz
        };

        public static int tick(TmaState e)
        {
            if (e.divZero)
            {
                e.divZero = false;
                return writeDivider(e, 0);
            }
            else
            {
                return writeDivider(e, e.divider + 1);
            }
        }

        static int writeDivider(TmaState e, int next)
        {
            int prev;

            prev = e.divider;
            e.divider = next;

            if ((e.control & 4) == 0)
            {
                return 0;
            }
            else
            {
                int mask = lut[e.control & 3];
                int edge = prev & ~next & mask;
                if (edge != 0)
                {
                    return tickCounter(e);
                }
                else
                {
                    return 0;
                }
            }
        }

        static int tickCounter(TmaState e)
        {
            if (e.counter == 0)
            {
                e.counter = e.modulus;
                return 1;
            }
            else
            {
                e.counter = (e.counter + 1) & 0xff;
                return 0;
            }
        }

        public static byte getControl(TmaState e) => (byte)(e.control);

        public static byte getCounter(TmaState e) => (byte)(e.counter);

        public static byte getDivider(TmaState e) => (byte)(e.divider >> 6);

        public static byte getModulus(TmaState e) => (byte)(e.modulus);

        public static void setControl(TmaState e, byte data) => e.control = data;

        public static void setCounter(TmaState e, byte data) => e.counter = data;

        public static void setDivider(TmaState e, byte data) => e.divZero = true;

        public static void setModulus(TmaState e, byte data) => e.modulus = data;
    }
}
