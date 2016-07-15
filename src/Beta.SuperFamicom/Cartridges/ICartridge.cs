namespace Beta.SuperFamicom.Cartridges
{
    public interface ICartridge
    {
        void Read(byte bank, ushort address, ref byte data);

        void Write(byte bank, ushort address, byte data);
    }
}
