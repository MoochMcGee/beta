namespace Beta.GameBoy
{
    public interface IAddressSpace
    {
        void Map(ushort start, Reader reader = null, Writer writer = null);

        void Map(ushort start, ushort end, Reader reader = null, Writer writer = null);

        byte Read(ushort address);

        void Write(ushort address, byte data);
    }
}
