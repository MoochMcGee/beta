namespace Beta.GameBoy.Memory
{
    public sealed class MemoryMap : IMemoryMap
    {
        private readonly ICartridgeConnector cart;
        private readonly MMIO mmio;
        private readonly OAM oam;
        private readonly VRAM vram;
        private readonly WRAM wram;

        public MemoryMap(ICartridgeConnector cart, MMIO mmio, OAM oam, VRAM vram, WRAM wram)
        {
            this.cart = cart;
            this.mmio = mmio;
            this.oam = oam;
            this.vram = vram;
            this.wram = wram;
        }

        public byte Read(ushort address)
        {
            var memory = Decode(address);
            if (memory != null)
            {
                return memory.Read(address);
            }
            else
            {
                return 0xff;
            }
        }

        public void Write(ushort address, byte data)
        {
            var memory = Decode(address);
            if (memory != null)
            {
                memory.Write(address, data);
            }
        }

        private IMemory Decode(ushort address)
        {
            if (address >= 0x0000 && address <= 0x7fff) { return cart; }
            if (address >= 0x8000 && address <= 0x9fff) { return vram; }
            if (address >= 0xa000 && address <= 0xbfff) { return cart; }
            if (address >= 0xc000 && address <= 0xfdff) { return wram; }
            if (address >= 0xfe00 && address <= 0xfe9f) { return  oam; }
            if (address >= 0xff00 && address <= 0xffff) { return mmio; }
            return null;
        }
    }
}
