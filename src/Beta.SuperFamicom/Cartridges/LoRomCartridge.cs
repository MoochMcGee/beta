namespace Beta.SuperFamicom.Cartridges
{
    public sealed class LoRomCartridge : ICartridge
    {
        private readonly byte[] image;
        private readonly int mask;

        public LoRomCartridge(byte[] image)
        {
            this.image = image;
            this.mask = image.Length - 1;
        }

        public void Read(byte bank, ushort address, ref byte data)
        {
            var index = (bank << 15) | (address & 0x7fff);

            data = image[index & mask];
        }

        public void Write(byte bank, ushort address, byte data) { }
    }
}
