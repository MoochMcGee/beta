using Beta.Famicom.Boards;
using Beta.Famicom.CPU;
using Beta.Famicom.Input;
using Beta.Famicom.Messaging;
using Beta.Famicom.PPU;
using Beta.Platform.Core;
using Beta.Platform.Messaging;
using SimpleInjector;

namespace Beta.Famicom
{
    public sealed class DriverFactory : IDriverFactory
    {
        private readonly Container container;
        private readonly IBoardFactory boardFactory;
        private readonly IJoypadFactory joypadFactory;
        private readonly ISubscriptionBroker broker;

        public DriverFactory(
            Container container,
            IBoardFactory boardFactory,
            IJoypadFactory joypadFactory,
            ISubscriptionBroker broker)
        {
            this.boardFactory = boardFactory;
            this.container = container;
            this.joypadFactory = joypadFactory;
            this.broker = broker;
        }

        public IDriver Create(byte[] binary)
        {
            var cpu = container.GetInstance<R2A03>();
            var cpuBus = container.GetInstance<R2A03Bus>();
            cpu.MapTo(cpuBus);
            cpu.Joypad1 = joypadFactory.Create(0);
            cpu.Joypad2 = joypadFactory.Create(1);

            var ppu = container.GetInstance<R2C02>();
            var ppuBus = container.GetInstance<R2C02Bus>();
            ppu.MapTo(cpuBus);

            var board = boardFactory.GetBoard(binary);
            board.Cpu = cpu;
            board.MapToCpu(cpuBus);
            board.MapToPpu(ppuBus);

            var result = new Driver(cpu, ppu, board);

            broker.Subscribe<PpuAddressSignal>(board);
            broker.Subscribe<ClockSignal>(board);
            broker.Subscribe<ClockSignal>(cpu);
            broker.Subscribe<ClockSignal>(ppu);
            broker.Subscribe<FrameSignal>(result);
            broker.Subscribe<VblSignal>(cpu);

            cpuBus.Map("000- ---- ---- ----", reader: result.PeekWRam, writer: result.PokeWRam);
            ppuBus.Map("  1- ---- ---- ----", reader: result.PeekVRam, writer: result.PokeVRam);

            result.Initialize();

            return result;
        }
    }
}
