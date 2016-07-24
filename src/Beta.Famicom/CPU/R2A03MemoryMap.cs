using Beta.Famicom.PPU;

namespace Beta.Famicom.CPU
{
    public sealed class R2A03MemoryMap
    {
        private readonly R2A03Registers r2a03;
        private readonly R2C02Registers r2c02;
        private readonly byte[] wram;

        public R2A03MemoryMap(R2A03Registers r2a03, R2C02Registers r2c02)
        {
            this.r2a03 = r2a03;
            this.r2c02 = r2c02;

            this.wram = new byte[0x800];
        }

        public void Read(ushort address, ref byte data)
        {
            if (address <= 0x1fff)
            {
                data = wram[address & 0x7ff];
            }
            else if (address <= 0x3fff)
            {
                r2c02.Read(address, ref data);
            }
            else if (address <= 0x4017)
            {
                r2a03.Read(address, ref data);
            }
        }

        public void Write(ushort address, byte data)
        {
            if (address <= 0x1fff)
            {
                wram[address & 0x7ff] = data;
            }
            else if (address <= 0x3fff)
            {
                r2c02.Write(address, data);
            }
            else if (address <= 0x4017)
            {
                r2a03.Write(address, data);
            }
        }
    }
}
