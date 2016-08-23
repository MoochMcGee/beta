using Beta.Famicom.Formats;

namespace Beta.Famicom.Boards
{
    public interface IBoard
    {
        void ApplyImage(CartridgeImage image);

        void R2C02Read(int address, ref byte data);

        void R2C02Write(int address, byte data);

        void R2A03Read(int address, ref byte data);

        void R2A03Write(int address, byte data);

        bool VRAM(int address, out int a10);
    }
}
