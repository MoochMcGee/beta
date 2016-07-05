namespace Beta.GameBoyAdvance.Memory
{
    public interface IMemory
    {
        uint Read(int size, uint address);

        void Write(int size, uint address, uint data);
    }
}
