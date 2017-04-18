namespace Beta.Famicom.Memory
{
    public static class MemoryFactory
    {
        public static IMemory createRam(int capacity)
        {
            return new Ram(capacity);
        }

        public static IMemory createRom(byte[] image)
        {
            return new Rom(image);
        }
    }
}
