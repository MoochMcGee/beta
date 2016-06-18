namespace Beta.Famicom.Memory
{
    public sealed class MemoryFactory : IMemoryFactory
    {
        public IMemory CreateRam(int capacity)
        {
            return new Ram(capacity);
        }

        public IMemory CreateRom(byte[] image)
        {
            return new Rom(image);
        }
    }
}
