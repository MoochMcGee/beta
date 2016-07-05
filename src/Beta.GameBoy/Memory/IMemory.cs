namespace Beta.GameBoy.Memory
{
    public interface IMemory
    {
        byte Read(ushort address);

        void Write(ushort address, byte data);
    }
}
