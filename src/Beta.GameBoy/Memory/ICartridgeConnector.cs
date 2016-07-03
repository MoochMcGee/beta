using Beta.GameBoy.Boards;

namespace Beta.GameBoy.Memory
{
    public interface ICartridgeConnector : IMemory
    {
        void InsertCartridge(byte[] cartridgeImage);
    }
}
