namespace Beta.GameBoy.Memory
{
    public static class Wave
    {
        public static byte Read(State state, ushort address)
        {
            return state.wave[address & 0xf];
        }

        public static void Write(State state, ushort address, byte data)
        {
            state.wave[address & 0xf] = data;
        }
    }
}
