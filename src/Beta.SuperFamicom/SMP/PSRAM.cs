namespace Beta.SuperFamicom.SMP
{
    public sealed class PSRAM
    {
        private readonly byte[] psram;

        public PSRAM()
        {
            psram = new byte[65536];
        }

        public byte Read(ushort address)
        {
            return psram[address];
        }

        public void Write(ushort address, byte data)
        {
            psram[address] = data;
        }
    }
}
