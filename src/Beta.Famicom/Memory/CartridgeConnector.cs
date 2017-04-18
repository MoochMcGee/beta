using Beta.Famicom.Boards;

namespace Beta.Famicom.Memory
{
    public static class CartridgeConnector
    {
        private static IBoard board;

        public static void InsertCartridge(IBoard board)
        {
            CartridgeConnector.board = board;
        }

        public static void R2C02Read(int address, ref byte data)
        {
            board.r2c02Read(address, ref data);
        }

        public static void R2C02Write(int address, byte data)
        {
            board.r2c02Write(address, data);
        }

        public static void R2A03Read(int address, ref byte data)
        {
            board.r2a03Read(address, ref data);
        }

        public static void R2A03Write(int address, byte data)
        {
            board.r2a03Write(address, data);
        }

        public static bool VRAM(int address, out int a10)
        {
            return board.vram(address, out a10);
        }
    }
}
