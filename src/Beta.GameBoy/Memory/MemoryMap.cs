namespace Beta.GameBoy.Memory
{
    public sealed class MemoryMap
    {
        private readonly State state;

        public MemoryMap(State state)
        {
            this.state = state;
        }

        public byte Read(ushort address)
        {
            if (address >= 0x0000 && address <= 0x7fff) { return CartridgeConnector.Read(state, address); }
            if (address >= 0x8000 && address <= 0x9fff) { return VRAM.Read(state, address); }
            if (address >= 0xa000 && address <= 0xbfff) { return CartridgeConnector.Read(state, address); }
            if (address >= 0xc000 && address <= 0xfdff) { return WRAM.Read(state, address); }
            if (address >= 0xfe00 && address <= 0xfe9f) { return  OAM.Read(state, address); }
            if (address >= 0xff00 && address <= 0xffff) { return MMIO.Read(state, address); }
            return 0xff;
        }

        public void Write(ushort address, byte data)
        {
            if (address >= 0x0000 && address <= 0x7fff) { CartridgeConnector.Write(state, address, data); return; }
            if (address >= 0x8000 && address <= 0x9fff) { VRAM.Write(state, address, data); return; }
            if (address >= 0xa000 && address <= 0xbfff) { CartridgeConnector.Write(state, address, data); return; }
            if (address >= 0xc000 && address <= 0xfdff) { WRAM.Write(state, address, data); return; }
            if (address >= 0xfe00 && address <= 0xfe9f) {  OAM.Write(state, address, data); return; }
            if (address >= 0xff00 && address <= 0xffff) { MMIO.Write(state, address, data); return; }
        }
    }
}
