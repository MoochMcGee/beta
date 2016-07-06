namespace Beta.Famicom.Abstractions
{
    public interface IBus
    {
        void Map(string pattern, Reader reader = null, Writer writer = null);

        void Read(ushort address, ref byte data);

        void Write(ushort address, ref byte data);
    }
}
