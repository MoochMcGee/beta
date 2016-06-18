namespace Beta.Famicom.Memory
{
    public interface IMemoryFactory
    {
        IMemory CreateRam(int capacity);

        IMemory CreateRom(byte[] image);
    }
}
