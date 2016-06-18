namespace Beta.GameBoy.APU
{
    public partial class Apu
    {
        private class Duration
        {
            public bool Enabled;
            public int Counter;
            public int Refresh;

            public bool Clock()
            {
                return (Enabled && Counter != 0 && --Counter == 0);
            }
        }
    }
}
