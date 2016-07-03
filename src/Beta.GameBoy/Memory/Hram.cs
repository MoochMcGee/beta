namespace Beta.GameBoy.Memory
{
    public sealed class HRAM : IMemory
    {
        private readonly byte[] hram = new byte[0x007f];

        public byte Read(ushort address)
        {
            return hram[address & 0x007f];
        }

        public void Write(ushort address, byte data)
        {
            hram[address & 0x007f] = data;
        }
    }
}
