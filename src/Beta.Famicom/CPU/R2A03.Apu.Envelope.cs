namespace Beta.Famicom.CPU
{
    public partial class R2A03
    {
        public class Envelope
        {
            private int[] regs = new int[2];
            private byte count;

            public bool Reset;
            public byte Level;

            private void UpdateLevel()
            {
                Level = (byte)(regs[regs[1] >> 4 & 1] & 0xf);
            }

            public void Clock()
            {
                if (Reset)
                {
                    Reset = false;
                    regs[0] = 0xF;
                }
                else
                {
                    if (count != 0)
                    {
                        count--;
                        return;
                    }

                    if (regs[0] != 0 || (regs[1] & 0x20) != 0)
                    {
                        regs[0] = (regs[0] - 1) & 0xf;
                    }
                }

                count = (byte)(regs[1] & 0xf);
                UpdateLevel();
            }

            public void Write(byte data)
            {
                regs[1] = data;
                UpdateLevel();
            }
        }
    }
}
