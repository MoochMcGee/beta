using Beta.Famicom.Boards;

namespace Beta.Famicom.Memory
{
    public sealed class CartridgeConnector
    {
        private IBoard board;

        public void InsertCartridge(IBoard board)
        {
            this.board = board;
        }

        public void R2C02Read(int address, ref byte data)
        {
            board.R2C02Read(address, ref data);
        }

        public void R2C02Write(int address, byte data)
        {
            board.R2C02Write(address, data);
        }

        public void R2A03Read(int address, ref byte data)
        {
            board.R2A03Read(address, ref data);
        }

        public void R2A03Write(int address, byte data)
        {
            board.R2A03Write(address, data);
        }

        public bool VRAM(int address, out int a10)
        {
            return board.VRAM(address, out a10);
        }
    }
}
