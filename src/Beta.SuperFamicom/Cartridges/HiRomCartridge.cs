namespace Beta.SuperFamicom.Cartridges
{
    public sealed class HiRomCartridge : ICartridge
    {
        private byte[] image;
        private int mask;

        public HiRomCartridge(byte[] image)
        {
            this.image = image;
            this.mask = image.Length - 1;
        }

        public void Read(byte bank, ushort address, ref byte data)
        {
            var index = (bank << 16) | (address & 0xffff);

            data = image[index & mask];
        }

        public void Write(byte bank, ushort address, byte data) { }
    }
}
