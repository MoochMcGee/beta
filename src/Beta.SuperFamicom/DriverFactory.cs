using Beta.Platform.Audio;
using Beta.Platform.Core;
using Beta.Platform.Messaging;
using Beta.Platform.Video;
using Beta.SuperFamicom.CPU;
using Beta.SuperFamicom.Messaging;
using Beta.SuperFamicom.PAD;
using Beta.SuperFamicom.PPU;
using Beta.SuperFamicom.SMP;

namespace Beta.SuperFamicom
{
    public sealed class DriverFactory : IDriverFactory
    {
        private readonly State state;
        private readonly IAudioBackend audio;
        private readonly IVideoBackend video;
        private readonly IProducer<HBlankSignal> hblank;
        private readonly IProducer<VBlankSignal> vblank;
        private readonly ISubscriptionBroker broker;

        public DriverFactory(
            State state,
            IAudioBackend audio,
            IVideoBackend video,
            IProducer<HBlankSignal> hblank,
            IProducer<VBlankSignal> vblank,
            ISubscriptionBroker broker)
        {
            this.state = state;
            this.audio = audio;
            this.video = video;
            this.hblank = hblank;
            this.vblank = vblank;
            this.broker = broker;
        }

        public IDriver Create(byte[] binary)
        {
            var driver = new Driver();

            driver.Bus = new BusA(driver, state, binary);
            driver.Dma = new Dma(state, driver.Bus);
            driver.Ppu = new Ppu(driver, video, hblank, vblank);
            driver.Smp = new Smp(driver, audio);
            driver.Cpu = new Cpu(state, driver.Bus, driver.Dma);

            driver.Smp.Initialize();
            driver.Ppu.Initialize();
            driver.Cpu.Initialize();
            driver.Bus.Initialize();

            driver.Joypad1 = new Pad(0);
            driver.Joypad2 = new Pad(1);

            broker.Subscribe<HBlankSignal>(driver.Cpu);
            broker.Subscribe<VBlankSignal>(driver.Cpu);

            return driver;
        }
    }
}
