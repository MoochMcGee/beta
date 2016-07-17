namespace Beta.Famicom.Memory
{
    public interface IMemory
    {
        void Read(int address, ref byte data);

        void Write(int address, byte data);
    }
}
