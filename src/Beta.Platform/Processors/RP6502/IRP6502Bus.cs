namespace Beta.Platform.Processors.RP6502
{
    public interface IRP6502Bus
    {
        void Peek(ushort address, ref byte data);

        void Poke(ushort address, ref byte data);
    }
}
