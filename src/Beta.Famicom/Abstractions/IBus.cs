using Beta.Platform.Processors.RP6502;

namespace Beta.Famicom.Abstractions
{
    public interface IBus : IRP6502Bus
    {
        IBusDecoder Decode(string pattern);
    }

    public interface IBusDecoder
    {
        IBusDecoder Peek(Access access);

        IBusDecoder Poke(Access access);
    }
}
