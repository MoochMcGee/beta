using Beta.GameBoy.Boards;

namespace Beta.GameBoy.Memory
{
    public static class CartridgeConnector
    {
        public static void InsertCartridge(State state, Board cart)
        {
            state.cart = cart;
        }

        public static byte Read(State state, ushort address)
        {
            return (state.boot_rom_enabled && address <= 0x00ff)
                ? BIOS.Read(state, address)
                : state.cart.Read(address)
                ;
        }

        public static void Write(State state, ushort address, byte data)
        {
            state.cart.Write(address, data);
        }
    }
}
