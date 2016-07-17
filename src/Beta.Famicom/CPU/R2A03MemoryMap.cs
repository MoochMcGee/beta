using Beta.Famicom.Memory;
using Beta.Famicom.PPU;

namespace Beta.Famicom.CPU
{
    public sealed class R2A03MemoryMap
    {
        private readonly CartridgeConnector cart;
        private readonly R2C02StateManager r2c02;
        private readonly R2A03StateManager r2a03;
        private readonly byte[] wram;

        public R2A03MemoryMap(CartridgeConnector cart, State state)
        {
            this.cart = cart;
            this.r2c02 = new R2C02StateManager(state);
            this.r2a03 = new R2A03StateManager(state);

            this.wram = new byte[0x800];
        }

        public void Read(ushort address, ref byte data)
        {
            cart.CpuRead(address, ref data);

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
            cart.CpuWrite(address, data);

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
