namespace Beta.GameBoy.Memory
{
    public interface IMemoryMap
    {
        byte Read(ushort address);

        void Write(ushort address, byte data);
    }
}
