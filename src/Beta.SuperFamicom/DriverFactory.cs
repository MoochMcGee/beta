using Beta.Platform.Core;
using Beta.Platform.Messaging;
using Beta.SuperFamicom.Messaging;
using Beta.SuperFamicom.PAD;
using SimpleInjector;

namespace Beta.SuperFamicom
{
    public sealed class DriverFactory : IDriverFactory
    {
        private readonly Container container;
        private readonly ISubscriptionBroker broker;

        public DriverFactory(Container container, ISubscriptionBroker broker)
        {
            this.container = container;
            this.broker = broker;
        }

        public IDriver Create(byte[] binary)
        {
            var driver = container.GetInstance<Driver>();

            driver.Bus.Driver = driver;
            driver.Cpu.Bus = driver.Bus;
            driver.Cpu.Dma = driver.Dma;
            driver.Dma.Bus = driver.Bus;

            driver.Joypad1 = new Pad(0);
            driver.Joypad2 = new Pad(1);

            broker.Subscribe<ClockSignal>(driver.Cpu);
            broker.Subscribe<ClockSignal>(driver.Ppu);
            broker.Subscribe<ClockSignal>(driver.Smp);
            broker.Subscribe<FrameSignal>(driver.Joypad1);
            broker.Subscribe<FrameSignal>(driver.Joypad2);
            broker.Subscribe<HBlankSignal>(driver.Cpu);
            broker.Subscribe<VBlankSignal>(driver.Cpu);

            driver.Smp.Initialize();
            driver.Bus.Initialize(binary);
            driver.Ppu.Initialize();
            driver.Cpu.Initialize();

            return driver;
        }
    }
}
