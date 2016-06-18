namespace Beta.Famicom.Memory
{
    public interface IMemory
    {
        void Peek(int address, ref byte data);

        void Poke(int address, ref byte data);
    }
}
