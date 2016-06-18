namespace Beta.Famicom.CPU
{
    public partial class R2A03
    {
        public class Duration
        {
            private static int[] lookup = new[]
            {
                0x0a, 0xfe, 0x14, 0x02, 0x28, 0x04, 0x50, 0x06,
                0xa0, 0x08, 0x3c, 0x0a, 0x0e, 0x0c, 0x1a, 0x0e,
                0x0c, 0x10, 0x18, 0x12, 0x30, 0x14, 0x60, 0x16,
                0xc0, 0x18, 0x48, 0x1a, 0x10, 0x1c, 0x20, 0x1e
            };

            private int enabled;

            public bool Halted;
            public int Counter;

            public void Clock()
            {
                if (Counter != 0 && !Halted)
                {
                    Counter = (Counter - 1) & enabled;
                }
            }

            public void SetCounter(byte value)
            {
                Counter = lookup[value >> 3] & enabled;
            }

            public void SetEnabled(bool value)
            {
                enabled = value ? 0xff : 0x00;
                Counter = Counter & enabled;
            }
        }
    }
}
