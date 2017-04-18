namespace Beta.Famicom.Memory
{
    public static class MemoryFactory
    {
        public static IMemory CreateRam(int capacity)
        {
            return new Ram(capacity);
        }

        public static IMemory CreateRom(byte[] image)
        {
            return new Rom(image);
        }
    }
}
