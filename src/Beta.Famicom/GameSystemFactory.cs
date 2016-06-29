using Beta.Famicom.Boards;
using Beta.Famicom.CPU;
using Beta.Famicom.Input;
using Beta.Famicom.Messaging;
using Beta.Famicom.PPU;
using Beta.Platform.Core;
using Beta.Platform.Messaging;

namespace Beta.Famicom
{
    public sealed class GameSystemFactory : IGameSystemFactory
    {
        private readonly IBoardFactory boardManager;
        private readonly R2A03Bus cpuBus;
        private readonly R2C02Bus ppuBus;
        private readonly IProducer<ClockSignal> clockProducer;
        private readonly IProducer<FrameSignal> frameProducer;
        private readonly IProducer<VblNmiSignal> vblNmiProducer;
        private readonly IJoypadFactory joypadFactory;

        public GameSystemFactory(
            IBoardFactory boardManager,
            IJoypadFactory joypadFactory,
            R2A03Bus cpuBus,
            R2C02Bus ppuBus,
            IProducer<ClockSignal> clockProducer,
            IProducer<FrameSignal> frameProducer,
            IProducer<VblNmiSignal> vblNmiProducer)
        {
            this.boardManager = boardManager;
            this.joypadFactory = joypadFactory;
            this.cpuBus = cpuBus;
            this.ppuBus = ppuBus;
            this.clockProducer = clockProducer;
            this.frameProducer = frameProducer;
            this.vblNmiProducer = vblNmiProducer;
        }

        public IGameSystem Create(byte[] binary)
        {
            var result = new GameSystem(cpuBus, ppuBus);

            result.Cpu = new R2A03(cpuBus, result, clockProducer);
            result.Cpu.Joypad1 = joypadFactory.Create(0);
            result.Cpu.Joypad2 = joypadFactory.Create(1);
            result.Cpu.MapTo(cpuBus);

            result.Ppu = new R2C02(ppuBus, result, vblNmiProducer, frameProducer);
            result.Ppu.MapTo(cpuBus);

            result.Board = boardManager.GetBoard(result, binary);
            result.Board.MapToCpu(cpuBus);
            result.Board.MapToPpu(ppuBus);

            clockProducer.Subscribe(result.Board);
            clockProducer.Subscribe(result.Ppu);
            frameProducer.Subscribe(result);
            vblNmiProducer.Subscribe(result.Cpu);

            return result;
        }
    }
}
