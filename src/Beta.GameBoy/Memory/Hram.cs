namespace Beta.GameBoy.Memory
{
    public static class HRAM
    {
        public static byte Read(State state, ushort address)
        {
            return state.hram[address & 0x007f];
        }

        public static void Write(State state, ushort address, byte data)
        {
            state.hram[address & 0x007f] = data;
        }
    }
}
