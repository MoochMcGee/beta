using Beta.Famicom.Boards;
using Beta.Famicom.CPU;
using Beta.Famicom.Input;
using Beta.Famicom.Messaging;
using Beta.Famicom.PPU;
using Beta.Platform;
using Beta.Platform.Core;
using Beta.Platform.Messaging;

namespace Beta.Famicom
{
    public sealed class GameSystem
        : IGameSystem
        , IConsumer<FrameSignal>
    {
        private readonly Board board;
        private readonly R2A03 r2a03;
        private readonly R2C02 r2c02;
        private readonly byte[] vram = new byte[2048];
        private readonly byte[] wram = new byte[2048];

        public GameSystem(R2A03 r2a03, R2C02 r2c02, Board board)
        {
            this.r2a03 = r2a03;
            this.r2c02 = r2c02;
            this.board = board;

            vram.Initialize<byte>(0xff);
            wram.Initialize<byte>(0xff);
        }

        public void PeekVRam(ushort address, ref byte data)
        {
            data = vram[(address & 0x3ff) | (board.VRamA10(address) << 10)];
        }

        public void PeekWRam(ushort address, ref byte data)
        {
            data = wram[address & 0x7ff];
        }

        public void PokeVRam(ushort address, ref byte data)
        {
            vram[(address & 0x3ff) | (board.VRamA10(address) << 10)] = data;
        }

        public void PokeWRam(ushort address, ref byte data)
        {
            wram[address & 0x7ff] = data;
        }

        public void Initialize()
        {
            r2a03.Initialize();
            r2c02.Initialize();
            board.Initialize();
        }

        public void Consume(FrameSignal e)
        {
            Joypad.AutofireState = !Joypad.AutofireState;

            r2a03.Joypad1.Update();
            r2a03.Joypad2.Update();
        }
    }
}
