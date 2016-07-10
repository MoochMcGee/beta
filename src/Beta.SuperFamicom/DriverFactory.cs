using Beta.Platform.Audio;
using Beta.Platform.Core;
using Beta.Platform.Video;
using Beta.SuperFamicom.CPU;
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

        public DriverFactory(State state, IAudioBackend audio, IVideoBackend video)
        {
            this.state = state;
            this.audio = audio;
            this.video = video;
        }

        public IDriver Create(byte[] binary)
        {
            var driver = new Driver();

            driver.Bus = new BusA(driver, state, binary);
            driver.Dma = new Dma(driver.Bus);
            driver.Ppu = new Ppu(driver, video);
            driver.Smp = new Smp(driver, audio);
            driver.Cpu = new Cpu(driver.Bus);

            driver.Smp.Initialize();
            driver.Ppu.Initialize();
            driver.Cpu.Initialize();
            driver.Bus.Initialize();

            driver.Joypad1 = new Pad(0);
            driver.Joypad2 = new Pad(1);

            return driver;
        }
    }
}
