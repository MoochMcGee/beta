using Beta.Famicom.Boards;
using Beta.Famicom.CPU;
using Beta.Famicom.Messaging;
using Beta.Famicom.PPU;
using Beta.Platform.Core;
using Beta.Platform.Messaging;

namespace Beta.Famicom
{
    public sealed class GameSystemFactory : IGameSystemFactory
    {
        private readonly IBoardManager boardManager;
        private readonly R2A03Bus cpuBus;
        private readonly R2C02Bus ppuBus;
        private readonly IProducer<ClockSignal> clockProducer;
        private readonly IProducer<FrameSignal> frameProducer;
        private readonly IProducer<VblNmiSignal> vblNmiProducer;

        public GameSystemFactory(
            IBoardManager boardManager,
            R2A03Bus cpuBus,
            R2C02Bus ppuBus,
            IProducer<ClockSignal> clockProducer,
            IProducer<FrameSignal> frameProducer,
            IProducer<VblNmiSignal> vblNmiProducer)
        {
            this.boardManager = boardManager;
            this.cpuBus = cpuBus;
            this.ppuBus = ppuBus;
            this.clockProducer = clockProducer;
            this.frameProducer = frameProducer;
            this.vblNmiProducer = vblNmiProducer;
        }

        public IGameSystem Create(byte[] binary)
        {
            var result = new GameSystem(cpuBus, ppuBus);

            result.Cpu = new R2A03(cpuBus, result, vblNmiProducer, clockProducer);
            result.Cpu.MapTo(cpuBus);

            result.Ppu = new R2C02(ppuBus, result, vblNmiProducer, frameProducer);
            result.Ppu.MapTo(cpuBus);

            result.Board = boardManager.GetBoard(result, binary);
            result.Board.MapToCpu(cpuBus);
            result.Board.MapToPpu(ppuBus);

            clockProducer.Subscribe(result.Board);
            clockProducer.Subscribe(result.Ppu);
            frameProducer.Subscribe(result);

            return result;
        }
    }
}
