namespace Beta.GameBoy.Memory
{
    public sealed class Wram
    {
        private byte[] wram = new byte[0x2000];

        public byte Read(ushort address)
        {
            return wram[address & 0x1fff];
        }

        public void Write(ushort address, byte data)
        {
            wram[address & 0x1fff] = data;
        }
    }
}
