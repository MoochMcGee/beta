namespace Beta.Platform.Processors.RP65816
{
    public interface IBus
    {
        void InternalOperation();

        byte Read(byte bank, ushort address);

        void Write(byte bank, ushort address, byte data);
    }
}
