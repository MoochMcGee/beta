using System.IO;

namespace Beta.GameBoy.Memory
{
    public sealed class Bios
    {
        private byte[] bios;

        public Bios()
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
