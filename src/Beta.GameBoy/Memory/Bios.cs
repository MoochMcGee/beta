namespace Beta.GameBoy.Memory
{
    public static class BIOS
    {
        public static byte Read(State state, ushort address)
        {
            return state.bios[address & 0x00ff];
        }

        public static void Write(State state, ushort address, byte data)
        {
            // Read-only :-)
        }
    }
}
