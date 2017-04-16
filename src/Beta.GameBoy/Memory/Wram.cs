namespace Beta.GameBoy.Memory
{
    public static class WRAM
    {
        public static byte Read(State state, ushort address)
        {
            return state.wram[address & 0x1fff];
        }

        public static void Write(State state, ushort address, byte data)
        {
            state.wram[address & 0x1fff] = data;
        }
    }
}
