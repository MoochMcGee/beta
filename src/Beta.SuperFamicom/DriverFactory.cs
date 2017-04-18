#define LOROM
using Beta.Platform;
using Beta.Platform.Messaging;
using Beta.SuperFamicom.Cartridges;
using Beta.SuperFamicom.Messaging;
using Beta.SuperFamicom.PAD;
using SimpleInjector;

namespace Beta.SuperFamicom
{
    public sealed class DriverFactory : IDriverFactory
    {
        private readonly Container container;
        private readonly State state;
        private readonly SignalBroker broker;

        public DriverFactory(Container container, State state, SignalBroker broker)
        {
            this.container = container;
            this.state = state;
            this.broker = broker;
        }

        public IDriver create(byte[] binary)
        {
            var driver = container.GetInstance<Driver>();

            driver.Bus.Cpu = driver.Cpu;
            driver.Cpu.Bus = driver.Bus;
            driver.Cpu.Dma = driver.Dma;
            driver.Dma.Bus = driver.Bus;

            driver.Joypad1 = new Pad(state, 0);
            driver.Joypad2 = new Pad(state, 1);

            broker.Link<ClockSignal>(driver.Cpu.Consume);
            broker.Link<ClockSignal>(driver.Ppu.Consume);
            broker.Link<ClockSignal>(driver.Smp.Consume);
            broker.Link<FrameSignal>(driver.Joypad1.Consume);
            broker.Link<FrameSignal>(driver.Joypad2.Consume);
            broker.Link<HBlankSignal>(driver.Cpu.Consume);
            broker.Link<VBlankSignal>(driver.Cpu.Consume);

#if LOROM
            var cart = new LoRomCartridge(binary);
#else
            var cart = new HiRomCartridge(binary);
#endif
            driver.Bus.Initialize(cart);
            driver.Cpu.Initialize();

            return driver;
        }
    }
}
