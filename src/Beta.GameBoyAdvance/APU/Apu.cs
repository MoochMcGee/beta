using Beta.GameBoyAdvance.CPU;
using Beta.Platform;
using Beta.Platform.Core;

namespace Beta.GameBoyAdvance.APU
{
    public partial class Apu : Processor
    {
        private Driver gameSystem;
        private Cpu cpu;
        private ChannelNoise noise;
        private ChannelWaveRam waveRam;
        private ChannelSquare1 square1;
        private ChannelSquare2 square2;

        private Timing courseTiming;
        private Timing sampleTiming;
        private byte[] registers = new byte[16];
        private int bias;
        private int course;
        private int psgShift;
        private int lvolume;
        private int rvolume;

        public ChannelDirectSound DirectSound1;
        public ChannelDirectSound DirectSound2;

        public Apu(Driver gameSystem)
        {
            this.gameSystem = gameSystem;
            cpu = gameSystem.Cpu;
            Single = 1;

            courseTiming.Period = 16777216 / 512;
            courseTiming.Single = 1;

            sampleTiming.Period = 16777216;
            sampleTiming.Single = 48000;

            MathHelper.Reduce(ref sampleTiming.Period, ref sampleTiming.Single);

            DirectSound1 = new ChannelDirectSound(gameSystem, sampleTiming);
            DirectSound2 = new ChannelDirectSound(gameSystem, sampleTiming);
            noise = new ChannelNoise(gameSystem, sampleTiming);
            waveRam = new ChannelWaveRam(gameSystem, sampleTiming);
            square1 = new ChannelSquare1(gameSystem, sampleTiming);
            square2 = new ChannelSquare2(gameSystem, sampleTiming);
        }

        #region Registers

        //4000080h - SOUNDCNT_L (NR50, NR51) - Channel L/R Volume/Enable (R/W)
        //  Bit   Expl.
        //  0-2   Sound 1-4 Master Volume RIGHT (0-7)
        //  3     Not used
        //  4-6   Sound 1-4 Master Volume LEFT (0-7)
        //  7     Not used
        //  8-11  Sound 1-4 Enable Flags RIGHT (each Bit 8-11, 0=Disable, 1=Enable)
        //  12-15 Sound 1-4 Enable Flags LEFT (each Bit 12-15, 0=Disable, 1=Enable)

        //4000084h - SOUNDCNT_X (NR52) - Sound on/off (R/W)
        // Bits 0-3 are automatically set when starting sound output, and are automatically cleared when a sound ends. (Ie. when the length expires, as far as length is enabled. The bits are NOT reset when an volume envelope ends.)
        //  Bit   Expl.
        //  0     Sound 1 ON flag (Read Only)
        //  1     Sound 2 ON flag (Read Only)
        //  2     Sound 3 ON flag (Read Only)
        //  3     Sound 4 ON flag (Read Only)
        //  4-6   Not used
        //  7     PSG/FIFO Master Enable (0=Disable, 1=Enable) (Read/Write)
        //  8-31  Not used

        //4000088h - SOUNDBIAS - Sound PWM Control (R/W, see below)
        // This register controls the final sound output. The default setting is 0200h, it is normally not required to change this value.
        //  Bit    Expl.
        //  0-9    Bias Level     (Default=200h, converting signed samples into unsigned)
        //  10-13  Not used
        //  14-15  Amplitude Resolution/Sampling Cycle (Default=0, see below)
        //  16-31  Not used

        private byte PeekReg(uint address)
        {
            return registers[address & 15];
        }

        private void PokeReg(uint address, byte data)
        {
            registers[address & 15] = data;
        }

        private byte Peek084(uint address)
        {
            var data = registers[4];

            data &= 0x80;

            if (square1.Enabled) data |= 0x01;
            if (square2.Enabled) data |= 0x02;
            if (waveRam.Enabled) data |= 0x04;
            if (noise.Enabled) data |= 0x08;

            return data;
        }

        private void Poke080(uint address, byte data)
        {
            registers[0] = data;

            //  0-2   Sound 1-4 Master Volume RIGHT (0-7)
            //  3     Not used
            //  4-6   Sound 1-4 Master Volume LEFT (0-7)
            //  7     Not used

            rvolume = ((data >> 0) & 7) + 1;
            lvolume = ((data >> 4) & 7) + 1;
        }

        private void Poke081(uint address, byte data)
        {
            registers[1] = data;

            //  0-3   Sound 1-4 Enable Flags RIGHT (each Bit 0-3, 0=Disable, 1=Enable)
            //  4-7   Sound 1-4 Enable Flags LEFT (each Bit 4-7, 0=Disable, 1=Enable)

            square1.renable = (data & 0x01) != 0;
            square2.renable = (data & 0x02) != 0;
            waveRam.renable = (data & 0x04) != 0;
            noise.renable = (data & 0x08) != 0;

            square1.lenable = (data & 0x10) != 0;
            square2.lenable = (data & 0x20) != 0;
            waveRam.lenable = (data & 0x40) != 0;
            noise.lenable = (data & 0x80) != 0;
        }

        private void Poke082(uint address, byte data)
        {
            //  0-1   Sound # 1-4 Volume   (0=25%, 1=50%, 2=100%, 3=Prohibited)
            //  2     DMA Sound A Volume   (0=50%, 1=100%)
            //  3     DMA Sound B Volume   (0=50%, 1=100%)
            //  4-7   Not used
            registers[2] = data;

            psgShift = data & 3;
            DirectSound1.Shift = (~data >> 2) & 1;
            DirectSound2.Shift = (~data >> 3) & 1;
        }

        private void Poke083(uint address, byte data)
        {
            registers[3] = data;

            DirectSound1.renable = (data & 0x01) != 0; //  0     DMA Sound A Enable RIGHT (0=Disable, 1=Enable)
            DirectSound2.renable = (data & 0x10) != 0; //  4     DMA Sound B Enable RIGHT (0=Disable, 1=Enable)

            DirectSound1.lenable = (data & 0x02) != 0; //  1     DMA Sound A Enable LEFT  (0=Disable, 1=Enable)
            DirectSound2.lenable = (data & 0x20) != 0; //  5     DMA Sound B Enable LEFT  (0=Disable, 1=Enable)

            DirectSound1.Timer = (data >> 2) & 1; //  2     DMA Sound A Timer Select (0=Timer 0, 1=Timer 1)
            DirectSound2.Timer = (data >> 6) & 1; //  6     DMA Sound B Timer Select (0=Timer 0, 1=Timer 1)

            if ((data & 0x08) != 0) DirectSound1.Clear(); //  3     DMA Sound A Reset FIFO   (1=Reset)
            if ((data & 0x80) != 0) DirectSound2.Clear(); //  7     DMA Sound B Reset FIFO   (1=Reset)
        }

        private void Poke084(uint address, byte data)
        {
            registers[4] = data;

            DirectSound1.Enabled = (data & 0x80) != 0;
            DirectSound2.Enabled = (data & 0x80) != 0;
        }

        private void Poke088(uint address, byte data)
        {
            registers[8] = data;

            bias = (bias & ~0x0ff) | ((data << 0) & 0x0ff);
        }

        private void Poke089(uint address, byte data)
        {
            registers[9] = data;

            bias = (bias & ~0x300) | ((data << 8) & 0x300);
        }

        #endregion

        private void Sample()
        {
            var dsa = DirectSound1.Level >> DirectSound1.Shift;
            var dsb = DirectSound2.Level >> DirectSound2.Shift;
            var sq1 = square1.Render(sampleTiming.Period);
            var sq2 = square2.Render(sampleTiming.Period);
            var wav = waveRam.Render(sampleTiming.Period);
            var noi = noise.Render(sampleTiming.Period);

            var lsample = 0; // 0x200 - bias;
            var rsample = 0; // 0x200 - bias;

            // if (psg_enable)
            {
                var lsequence = 0;
                if (square1.lenable) lsequence += sq1;
                if (square2.lenable) lsequence += sq2;
                if (waveRam.lenable) lsequence += wav;
                if (noise.lenable) lsequence += noi;

                var rsequence = 0;
                if (square1.renable) rsequence += sq1;
                if (square2.renable) rsequence += sq2;
                if (waveRam.renable) rsequence += wav;
                if (noise.renable) rsequence += noi;

                if (psgShift < 3)
                {
                    lsample += (lsequence * lvolume) >> (2 - psgShift);
                    rsample += (rsequence * rvolume) >> (2 - psgShift);
                }
            }

            if (DirectSound1.lenable) lsample += dsa; // -80h ~ 7Fh
            if (DirectSound2.lenable) lsample += dsb; // -80h ~ 7Fh

            if (DirectSound1.renable) rsample += dsa; // -80h ~ 7Fh
            if (DirectSound2.renable) rsample += dsb; // -80h ~ 7Fh

            lsample = (lsample > 511) ? 511 : (lsample < -512) ? -512 : lsample;
            rsample = (rsample > 511) ? 511 : (rsample < -512) ? -512 : rsample;
            lsample = (lsample << 6) | ((lsample >> 3) & 0x3f);
            rsample = (rsample << 6) | ((rsample >> 3) & 0x3f);

            gameSystem.Audio.Render(lsample);
            gameSystem.Audio.Render(rsample);
        }

        public void Initialize()
        {
            square1.Initialize(0x060);
            square2.Initialize(0x068);
            waveRam.Initialize(0x070);
            noise.Initialize(0x078);

            gameSystem.mmio.Map(0x080, PeekReg, Poke080);
            gameSystem.mmio.Map(0x081, PeekReg, Poke081);
            gameSystem.mmio.Map(0x082, PeekReg, Poke082);
            gameSystem.mmio.Map(0x083, PeekReg, Poke083);
            gameSystem.mmio.Map(0x084, Peek084, Poke084);
            gameSystem.mmio.Map(0x085, PeekReg, PokeReg);
            gameSystem.mmio.Map(0x086, PeekReg, PokeReg);
            gameSystem.mmio.Map(0x087, PeekReg, PokeReg);
            gameSystem.mmio.Map(0x088, PeekReg, Poke088);
            gameSystem.mmio.Map(0x089, PeekReg, Poke089);
            gameSystem.mmio.Map(0x08a, PeekReg, PokeReg);
            gameSystem.mmio.Map(0x08b, PeekReg, PokeReg);
            gameSystem.mmio.Map(0x08c, PeekReg, PokeReg);
            gameSystem.mmio.Map(0x08d, PeekReg, PokeReg);
            gameSystem.mmio.Map(0x08e, PeekReg, PokeReg);
            gameSystem.mmio.Map(0x08f, PeekReg, PokeReg);

            gameSystem.mmio.Map(0x090, 0x09f, waveRam.Peek, waveRam.Poke);

            DirectSound1.Initialize(cpu.Dma.Channels[1], 0x0a0);
            DirectSound2.Initialize(cpu.Dma.Channels[2], 0x0a4);
        }

        public override void Update(int cycles)
        {
            courseTiming.Cycles += cycles * courseTiming.Single;

            while (courseTiming.Cycles >= courseTiming.Period)
            {
                courseTiming.Cycles -= courseTiming.Period;

                switch (course)
                {
                case 0:
                case 4:
                    square1.ClockDuration();
                    square2.ClockDuration();
                    waveRam.ClockDuration();
                    noise.ClockDuration();
                    break;

                case 2:
                case 6:
                    square1.ClockDuration();
                    square2.ClockDuration();
                    waveRam.ClockDuration();
                    noise.ClockDuration();

                    square1.ClockSweep();
                    break;

                case 7:
                    square1.ClockEnvelope();
                    square2.ClockEnvelope();
                    noise.ClockEnvelope();
                    break;
                }

                course = (course + 1) & 7;
            }

            sampleTiming.Cycles += cycles * sampleTiming.Single;

            while (sampleTiming.Cycles >= sampleTiming.Period)
            {
                sampleTiming.Cycles -= sampleTiming.Period;
                Sample();
            }
        }
    }
}
