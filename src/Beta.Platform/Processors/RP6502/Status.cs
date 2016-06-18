namespace Beta.Platform.Processors.RP6502
{
    public struct Status
    {
        public int N;
        public int V;
        public int D;
        public int I;
        public int Z;
        public int C;

        public byte Pack()
        {
            return (byte)(
                (N << 7) |
                (V << 6) |
                (D << 3) |
                (I << 2) |
                (Z << 1) |
                (C << 0) | 0x30);
        }

        public void Unpack(byte value)
        {
            N = (value >> 7) & 1;
            V = (value >> 6) & 1;
            D = (value >> 3) & 1;
            I = (value >> 2) & 1;
            Z = (value >> 1) & 1;
            C = (value >> 0) & 1;
        }
    }
}
