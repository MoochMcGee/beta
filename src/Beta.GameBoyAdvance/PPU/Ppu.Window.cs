namespace Beta.GameBoyAdvance.PPU
{
    public sealed class Window
    {
        public bool Enabled;
        public byte Flags;

        public byte X1;
        public byte X2;
        public byte Y1;
        public byte Y2;

        public void Calculate(int[] buffer, int vclock)
        {
            if (Y1 > Y2)
            { // edge case behavior
                if (vclock < Y1 && vclock >= Y2)
                {
                    return;
                }
            }
            else
            {
                if (vclock < Y1 || vclock >= Y2)
                {
                    return;
                }
            }

            for (var i = X1; i != X2; i++)
            {
                if (i < 240)
                {
                    buffer[i] = Flags;
                }
            }
        }
    }
}
