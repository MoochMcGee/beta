using Beta.Platform;

namespace Beta.SuperFamicom.Memory
{
    public sealed class WRAM
    {
        private byte[] wram = new byte[0x20000];

        public int address;

        public WRAM()
        {
            wram.Initialize<byte>(0x55);
        }

        public void Read(byte bank, ushort address, ref byte data)
        {
            data = wram[((bank << 16) & 0x10000) | address];
        }

        public void Write(byte bank, ushort address, byte data)
        {
            wram[((bank << 16) & 0x10000) | address] = data;
        }

        public byte Read()
        {
            var data = wram[address];
            address = (address + 1) & 0x1ffff;

            return data;
        }

        public void Write(byte data)
        {
            wram[address] = data;
            address = (address + 1) & 0x1ffff;
        }
    }
}
