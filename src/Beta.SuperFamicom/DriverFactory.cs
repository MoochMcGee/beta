#define LOROM
using Beta.Platform.Core;
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
        private readonly ISignalBroker broker;

        public DriverFactory(Container container, State state, ISignalBroker broker)
        {
            this.container = container;
            this.state = state;
            this.broker = broker;
        }

        public IDriver Create(byte[] binary)
        {
            var driver = container.GetInstance<Driver>();

            driver.Bus.Cpu = driver.Cpu;
            driver.Cpu.Bus = driver.Bus;
            driver.Cpu.Dma = driver.Dma;
            driver.Dma.Bus = driver.Bus;

            driver.Joypad1 = new Pad(state, 0);
            driver.Joypad2 = new Pad(state, 1);

            broker.Link<ClockSignal>(driver.Cpu);
            broker.Link<ClockSignal>(driver.Ppu);
            broker.Link<ClockSignal>(driver.Smp);
            broker.Link<FrameSignal>(driver.Joypad1);
            broker.Link<FrameSignal>(driver.Joypad2);
            broker.Link<HBlankSignal>(driver.Cpu);
            broker.Link<VBlankSignal>(driver.Cpu);

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
