namespace Beta.Platform.Processors.LR35902
{
    public struct Status
    {
        public int Z;
        public int N;
        public int H;
        public int C;

        public void Load(byte value)
        {
            Z = (value >> 7) & 1;
            N = (value >> 6) & 1;
            H = (value >> 5) & 1;
            C = (value >> 4) & 1;
        }

        public byte Save()
        {
            return (byte)(
                (Z << 7) |
                (N << 6) |
                (H << 5) |
                (C << 4));
        }
    }
}
