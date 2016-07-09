namespace Beta.GameBoy.Memory
{
    public sealed class MemoryMap
    {
        private readonly CartridgeConnector cart;
        private readonly MMIO mmio;
        private readonly OAM oam;
        private readonly VRAM vram;
        private readonly WRAM wram;

        public MemoryMap(CartridgeConnector cart, MMIO mmio, OAM oam, VRAM vram, WRAM wram)
        {
            this.cart = cart;
            this.mmio = mmio;
            this.oam = oam;
            this.vram = vram;
            this.wram = wram;
        }

        public byte Read(ushort address)
        {
            if (address >= 0x0000 && address <= 0x7fff) { return cart.Read(address); }
            if (address >= 0x8000 && address <= 0x9fff) { return vram.Read(address); }
            if (address >= 0xa000 && address <= 0xbfff) { return cart.Read(address); }
            if (address >= 0xc000 && address <= 0xfdff) { return wram.Read(address); }
            if (address >= 0xfe00 && address <= 0xfe9f) { return  oam.Read(address); }
            if (address >= 0xff00 && address <= 0xffff) { return mmio.Read(address); }
            return 0xff;
        }

        public void Write(ushort address, byte data)
        {
            if (address >= 0x0000 && address <= 0x7fff) { cart.Write(address, data); return; }
            if (address >= 0x8000 && address <= 0x9fff) { vram.Write(address, data); return; }
            if (address >= 0xa000 && address <= 0xbfff) { cart.Write(address, data); return; }
            if (address >= 0xc000 && address <= 0xfdff) { wram.Write(address, data); return; }
            if (address >= 0xfe00 && address <= 0xfe9f) {  oam.Write(address, data); return; }
            if (address >= 0xff00 && address <= 0xffff) { mmio.Write(address, data); return; }
        }
    }
}
