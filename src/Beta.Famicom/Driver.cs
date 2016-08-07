using Beta.Famicom.Boards;
using Beta.Famicom.CPU;
using Beta.Famicom.PPU;
using Beta.Platform;
using Beta.Platform.Core;

namespace Beta.Famicom
{
    public sealed class Driver : IDriver
    {
        private readonly Board board;
        private readonly R2A03 r2a03;
        private readonly R2C02 r2c02;
        private readonly byte[] vram = new byte[2048];

        public Driver(R2A03 r2a03, R2C02 r2c02, Board board)
        {
            this.r2a03 = r2a03;
            this.r2c02 = r2c02;
            this.board = board;

            vram.Initialize<byte>(0xff);
        }

        public void PeekVRam(ushort address, ref byte data)
        {
            int a10;

            if (board.VRAM(address, out a10))
            {
                data = vram[(address & 0x3ff) | (a10 << 10)];
            }
        }

        public void PokeVRam(ushort address, byte data)
        {
            int a10;

            if (board.VRAM(address, out a10))
            {
                vram[(address & 0x3ff) | (a10 << 10)] = data;
            }
        }

        public void Initialize()
        {
            // r2a03.Initialize();
            r2c02.Initialize();
            board.Initialize();
        }

        public void Main()
        {
            r2a03.ResetHard();

            while (true)
            {
                r2a03.Update();
            }
        }
    }
}
