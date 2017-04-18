using Beta.Famicom.Formats;

namespace Beta.Famicom.Boards
{
    public interface IBoard
    {
        void applyImage(CartridgeImage image);

        void r2c02Read(int address, ref byte data);

        void r2c02Write(int address, byte data);

        void r2a03Read(int address, ref byte data);

        void r2a03Write(int address, byte data);

        bool vram(int address, out int a10);
    }
}
