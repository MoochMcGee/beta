using Beta.Famicom.Formats;

namespace Beta.Famicom.Boards
{
    public interface IBoard
    {
        void ApplyImage(CartridgeImage image);

        void R2C02Read(ushort address, ref byte data);

        void R2C02Write(ushort address, byte data);

        void R2A03Read(ushort address, ref byte data);

        void R2A03Write(ushort address, byte data);

        bool VRAM(ushort address, out int a10);
    }
}
