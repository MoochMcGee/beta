using Beta.Famicom.Boards;
using Beta.Famicom.CPU;
using Beta.Famicom.Input;
using Beta.Famicom.Memory;
using Beta.Famicom.Messaging;
using Beta.Famicom.PPU;
using Beta.Platform;
using Beta.Platform.Messaging;
using SimpleInjector;

namespace Beta.Famicom
{
    public sealed class DriverFactory : IDriverFactory
    {
        private readonly Container container;
        private readonly CartridgeConnector cartridge;
        private readonly InputConnector input;
        private readonly BoardFactory boardFactory;
        private readonly IJoypadFactory joypadFactory;
        private readonly SignalBroker broker;

        public DriverFactory(
            Container container,
            CartridgeConnector cartridge,
            InputConnector input,
            BoardFactory boardFactory,
            IJoypadFactory joypadFactory,
            SignalBroker broker)
        {
            this.container = container;
            this.cartridge = cartridge;
            this.input = input;
            this.boardFactory = boardFactory;
            this.joypadFactory = joypadFactory;
            this.broker = broker;
        }

        public IDriver Create(byte[] binary)
        {
            input.ConnectJoypad1(joypadFactory.Create(0));
            input.ConnectJoypad2(joypadFactory.Create(1));

            var mixer = container.GetInstance<Mixer>();
            var r2a03 = container.GetInstance<R2A03>();
            var r2c02 = container.GetInstance<R2C02>();
            var board = boardFactory.GetBoard(binary);

            cartridge.InsertCartridge(board);

            broker.Link<HalfFrameSignal>(r2a03.Consume);
            broker.Link<QuadFrameSignal>(r2a03.Consume);
            broker.Link<ClockSignal>(r2a03.Consume);
            broker.Link<ClockSignal>(r2c02.Consume);
            broker.Link<ClockSignal>(mixer.Consume);
            broker.Link<FrameSignal>(input.Consume);
            broker.Link<IrqSignal>(r2a03.Consume);
            broker.Link<VblSignal>(r2a03.Consume);

            broker.Link<ClockSignal>(container.GetInstance<Sq1>().Consume);
            broker.Link<ClockSignal>(container.GetInstance<Sq2>().Consume);
            broker.Link<ClockSignal>(container.GetInstance<Tri>().Consume);
            broker.Link<ClockSignal>(container.GetInstance<Noi>().Consume);

            return container.GetInstance<Driver>();
        }
    }
}
