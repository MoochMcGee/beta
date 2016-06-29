namespace Beta.Platform.Processors.RP6502
{
    public interface IRP6502Bus
    {
        void Read(ushort address, ref byte data);

        void Write(ushort address, ref byte data);
    }
}
