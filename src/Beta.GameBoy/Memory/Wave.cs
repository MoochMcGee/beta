namespace Beta.GameBoy.Memory
{
    public sealed class Wave
    {
        private byte[] wave = new byte[16];

        public byte Read(ushort address)
        {
            return wave[address & 0xf];
        }

        public void Write(ushort address, byte data)
        {
            wave[address & 0xf] = data;
        }
    }
}
