using System.Threading;
using Beta.GameBoy.APU;
using Beta.GameBoy.Boards;
using Beta.GameBoy.CPU;
using Beta.GameBoy.PPU;
using Beta.Platform.Audio;
using Beta.Platform.Core;
using Beta.Platform.Video;

namespace Beta.GameBoy
{
    public partial class GameSystem : IGameSystem
    {
        private Board board;
        public Pad pad;
        private Tma tma;
        public Cpu cpu;
        private Ppu ppu;
        private Apu apu;

        public IAudioBackend Audio { get; set; }

        public IVideoBackend Video { get; set; }

        public GameSystem()
        {
            cpu = new Cpu(this);
            ppu = new Ppu(this);
            apu = new Apu(this);
            pad = new Pad(this);
            tma = new Tma(this);

            Hook(0x0000, 0xffff,
                delegate { return 0; },
                delegate { });
        }

        public void Emulate()
        {
            Initialize();

            while (true)
            {
                try
                {
                    cpu.Update();
                }
                catch (ThreadAbortException)
                {
                    break;
                }
            }
        }

        public void Initialize()
        {
            board.Initialize();

            InitializeMemory();

            cpu.Initialize();
            ppu.Initialize();
            apu.Initialize();
            pad.Initialize();
            tma.Initialize();
        }

        public void LoadGame(byte[] binary)
        {
            board = BoardManager.GetBoard(this, binary);
        }
    }
}
