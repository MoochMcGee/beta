using System.Threading;
using Beta.Famicom.Abstractions;
using Beta.Famicom.Boards;
using Beta.Famicom.CPU;
using Beta.Famicom.PPU;
using Beta.Platform;
using Beta.Platform.Audio;
using Beta.Platform.Core;
using Beta.Platform.Video;

namespace Beta.Famicom
{
    public sealed class GameSystem : IGameSystem
    {
        private readonly IBoardManager boardManager;

        private IBus cpuBus;
        private IBus ppuBus;
        private byte[] vram = new byte[2048];
        private byte[] wram = new byte[2048];

        public IBoard Board;
        public R2A03 Cpu;
        public R2C02 Ppu;

        public IAudioBackend Audio { get; set; }

        public IVideoBackend Video { get; set; }

        public GameSystem(IBoardManager boardManager)
        {
            this.boardManager = boardManager;

            cpuBus = new Bus(1 << 16);
            ppuBus = new Bus(1 << 14);

            Cpu = new R2A03(cpuBus, this);
            Ppu = new R2C02(ppuBus, this);

            vram.Initialize<byte>(0xff);
            wram.Initialize<byte>(0xff);

            cpuBus.Decode("---- ---- ---- ----").Peek(Peek____).Poke(Poke____);
            cpuBus.Decode("000- ---- ---- ----").Peek(PeekWRam).Poke(PokeWRam);
            ppuBus.Decode("  -- ---- ---- ----").Peek(Peek____).Poke(Poke____);
            ppuBus.Decode("  1- ---- ---- ----").Peek(PeekVRam).Poke(PokeVRam);

            Cpu.MapTo(cpuBus);
            Ppu.MapTo(cpuBus);
        }

        private static void Peek____(ushort address, ref byte data)
        {
        }

        private static void Poke____(ushort address, ref byte data)
        {
        }

        private void PeekVRam(ushort address, ref byte data)
        {
            data = vram[(address & 0x3ff) | (Board.VRamA10(address) << 10)];
        }

        private void PeekWRam(ushort address, ref byte data)
        {
            data = wram[address & 0x7ff];
        }

        private void PokeVRam(ushort address, ref byte data)
        {
            vram[(address & 0x3ff) | (Board.VRamA10(address) << 10)] = data;
        }

        private void PokeWRam(ushort address, ref byte data)
        {
            wram[address & 0x7ff] = data;
        }

        public void Emulate()
        {
            Initialize();

            Cpu.ResetHard();
            Board.ResetHard();

            try
            {
                while (true)
                {
                    Cpu.Update();
                }
            }
            catch (ThreadAbortException) { }
        }

        public void Initialize()
        {
            Cpu.Initialize();
            Ppu.Initialize();

            Board.Initialize();
        }

        public void LoadGame(byte[] binary)
        {
            Board = boardManager.GetBoard(this, binary);
            Board.MapToCpu(cpuBus);
            Board.MapToPpu(ppuBus);
        }
    }
}
