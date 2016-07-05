using System.IO;

namespace Beta.GameBoy.Memory
{
    public sealed class BIOS : IMemory
    {
        private readonly byte[] bios;

        public BIOS()
        {
            bios = File.ReadAllBytes("drivers/gb.sys/boot.rom");
        }

        public byte Read(ushort address)
        {
            return bios[address & 0x00ff];
        }

        public void Write(ushort address, byte data)
        {
            // Read-only :-)
        }
    }
}
