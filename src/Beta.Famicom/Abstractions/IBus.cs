using Beta.Platform.Processors.RP6502;

namespace Beta.Famicom.Abstractions
{
    public interface IBus : IRP6502Bus
    {
        void Map(string pattern, Reader reader = null, Writer writer = null);
    }
}
