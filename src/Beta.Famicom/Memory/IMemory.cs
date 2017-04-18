namespace Beta.Famicom.Memory
{
    public interface IMemory
    {
        void read(int address, ref byte data);

        void write(int address, byte data);
    }
}
