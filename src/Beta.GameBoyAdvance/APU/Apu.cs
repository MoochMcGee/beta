using Beta.GameBoyAdvance.Memory;
using Beta.Platform.Audio;
using Beta.Platform.Messaging;

namespace Beta.GameBoyAdvance.APU
{
    public sealed class Apu
    {
        public const int Frequency = 16777216;
        public const int Single = 48000;

        private readonly IAudioBackend audio;
        private readonly DmaController dma;
        private readonly MMIO mmio;

        private ChannelNOI noi;
        private ChannelWAV wav;
        private ChannelSQ1 sq1;
        private ChannelSQ2 sq2;

        private int courseTimer;
        private int sampleTimer;
        private byte[] registers = new byte[16];
        private int bias;
        private int course;
        private int psgShift;
        private int lvolume;
        private int rvolume;

        public ChannelPCM PCM1;
        public ChannelPCM PCM2;

        public Apu(DmaController dma, MMIO mmio, IAudioBackend audio)
        {
            this.dma = dma;
            this.audio = audio;
            this.mmio = mmio;

            PCM1 = new ChannelPCM();
            PCM2 = new ChannelPCM();
            noi  = new ChannelNOI();
            wav  = new ChannelWAV();
            sq1  = new ChannelSQ1();
            sq2  = new ChannelSQ2();
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

        private byte ReadReg(uint address)
        {
            return registers[address & 15];
        }

        private void WriteReg(uint address, byte data)
        {
        }

        private byte Read084(uint address)
        {
            var data = registers[4];

            data &= 0x80;

            if (sq1.active) data |= 0x01;
            if (sq2.active) data |= 0x02;
            if (wav.active) data |= 0x04;
            if (noi.active) data |= 0x08;

            return data;
        }

        private void Write080(uint address, byte data)
        {
            registers[0] = data &= 0x77;

            //  0-2   Sound 1-4 Master Volume RIGHT (0-7)
            //  3     Not used
            //  4-6   Sound 1-4 Master Volume LEFT (0-7)
            //  7     Not used

            rvolume = ((data >> 0) & 7) + 1;
            lvolume = ((data >> 4) & 7) + 1;
        }

        private void Write081(uint address, byte data)
        {
            registers[1] = data &= 0xff;

            //  0-3   Sound 1-4 Enable Flags RIGHT (each Bit 0-3, 0=Disable, 1=Enable)
            //  4-7   Sound 1-4 Enable Flags LEFT (each Bit 4-7, 0=Disable, 1=Enable)

            sq1.renable = (data & 0x01) != 0;
            sq2.renable = (data & 0x02) != 0;
            wav.renable = (data & 0x04) != 0;
            noi.renable = (data & 0x08) != 0;

            sq1.lenable = (data & 0x10) != 0;
            sq2.lenable = (data & 0x20) != 0;
            wav.lenable = (data & 0x40) != 0;
            noi.lenable = (data & 0x80) != 0;
        }

        private void Write082(uint address, byte data)
        {
            //  0-1   Sound # 1-4 Volume   (0=25%, 1=50%, 2=100%, 3=Prohibited)
            //  2     DMA Sound A Volume   (0=50%, 1=100%)
            //  3     DMA Sound B Volume   (0=50%, 1=100%)
            //  4-7   Not used
            registers[2] = data &= 0x0f;

            psgShift = data & 3;
            PCM1.Shift = (~data >> 2) & 1;
            PCM2.Shift = (~data >> 3) & 1;
        }

        private void Write083(uint address, byte data)
        {
            registers[3] = data &= 0x77;

            PCM1.renable = (data & 0x01) != 0; //  0     DMA Sound A Enable RIGHT (0=Disable, 1=Enable)
            PCM2.renable = (data & 0x10) != 0; //  4     DMA Sound B Enable RIGHT (0=Disable, 1=Enable)

            PCM1.lenable = (data & 0x02) != 0; //  1     DMA Sound A Enable LEFT  (0=Disable, 1=Enable)
            PCM2.lenable = (data & 0x20) != 0; //  5     DMA Sound B Enable LEFT  (0=Disable, 1=Enable)

            PCM1.Timer = (data >> 2) & 1; //  2     DMA Sound A Timer Select (0=Timer 0, 1=Timer 1)
            PCM2.Timer = (data >> 6) & 1; //  6     DMA Sound B Timer Select (0=Timer 0, 1=Timer 1)

            if ((data & 0x08) != 0) PCM1.Clear(); //  3     DMA Sound A Reset FIFO   (1=Reset)
            if ((data & 0x80) != 0) PCM2.Clear(); //  7     DMA Sound B Reset FIFO   (1=Reset)
        }

        private void Write084(uint address, byte data)
        {
            registers[4] = data;

            PCM1.enabled = (data & 0x80) != 0;
            PCM2.enabled = (data & 0x80) != 0;
        }

        private void Write088(uint address, byte data)
        {
            registers[8] = data;

            bias = (bias & ~0x0ff) | ((data << 0) & 0x0ff);
        }

        private void Write089(uint address, byte data)
        {
            registers[9] = data;

            bias = (bias & ~0x300) | ((data << 8) & 0x300);
        }

        #endregion

        private void Sample()
        {
            var dsa = PCM1.Level >> PCM1.Shift;
            var dsb = PCM2.Level >> PCM2.Shift;
            var sq1 = this.sq1.Render(Frequency);
            var sq2 = this.sq2.Render(Frequency);
            var wav = this.wav.Render(Frequency);
            var noi = this.noi.Render(Frequency);

            var lsample = 0; // 0x200 - bias;
            var rsample = 0; // 0x200 - bias;

            // if (psg_enable)
            {
                var lsequence = 0;
                if (this.sq1.lenable) lsequence += sq1;
                if (this.sq2.lenable) lsequence += sq2;
                if (this.wav.lenable) lsequence += wav;
                if (this.noi.lenable) lsequence += noi;

                var rsequence = 0;
                if (this.sq1.renable) rsequence += sq1;
                if (this.sq2.renable) rsequence += sq2;
                if (this.wav.renable) rsequence += wav;
                if (this.noi.renable) rsequence += noi;

                if (psgShift < 3)
                {
                    lsample += (lsequence * lvolume) >> (2 - psgShift);
                    rsample += (rsequence * rvolume) >> (2 - psgShift);
                }
            }

            if (PCM1.lenable) lsample += dsa; // -80h ~ 7Fh
            if (PCM2.lenable) lsample += dsb; // -80h ~ 7Fh

            if (PCM1.renable) rsample += dsa; // -80h ~ 7Fh
            if (PCM2.renable) rsample += dsb; // -80h ~ 7Fh

            lsample = (lsample > 511) ? 511 : (lsample < -512) ? -512 : lsample;
            rsample = (rsample > 511) ? 511 : (rsample < -512) ? -512 : rsample;
            lsample = (lsample << 6) | ((lsample >> 3) & 0x3f);
            rsample = (rsample << 6) | ((rsample >> 3) & 0x3f);

            audio.Render(lsample);
            audio.Render(rsample);
        }

        public void Initialize()
        {
            mmio.Map(0x060, sq1.ReadRegister1, sq1.WriteRegister1);
            mmio.Map(0x061, sq1.ReadRegister2, sq1.WriteRegister2);
            mmio.Map(0x062, sq1.ReadRegister3, sq1.WriteRegister3);
            mmio.Map(0x063, sq1.ReadRegister4, sq1.WriteRegister4);
            mmio.Map(0x064, sq1.ReadRegister5, sq1.WriteRegister5);
            mmio.Map(0x065, sq1.ReadRegister6, sq1.WriteRegister6);
            mmio.Map(0x066, sq1.ReadRegister7, sq1.WriteRegister7);
            mmio.Map(0x067, sq1.ReadRegister8, sq1.WriteRegister8);

            mmio.Map(0x068, sq2.ReadRegister1, sq2.WriteRegister1);
            mmio.Map(0x069, sq2.ReadRegister2, sq2.WriteRegister2);
            mmio.Map(0x06a, sq2.ReadRegister3, sq2.WriteRegister3);
            mmio.Map(0x06b, sq2.ReadRegister4, sq2.WriteRegister4);
            mmio.Map(0x06c, sq2.ReadRegister5, sq2.WriteRegister5);
            mmio.Map(0x06d, sq2.ReadRegister6, sq2.WriteRegister6);
            mmio.Map(0x06e, sq2.ReadRegister7, sq2.WriteRegister7);
            mmio.Map(0x06f, sq2.ReadRegister8, sq2.WriteRegister8);

            mmio.Map(0x070, wav.ReadRegister1, wav.WriteRegister1);
            mmio.Map(0x071, wav.ReadRegister2, wav.WriteRegister2);
            mmio.Map(0x072, wav.ReadRegister3, wav.WriteRegister3);
            mmio.Map(0x073, wav.ReadRegister4, wav.WriteRegister4);
            mmio.Map(0x074, wav.ReadRegister5, wav.WriteRegister5);
            mmio.Map(0x075, wav.ReadRegister6, wav.WriteRegister6);
            mmio.Map(0x076, wav.ReadRegister7, wav.WriteRegister7);
            mmio.Map(0x077, wav.ReadRegister8, wav.WriteRegister8);

            mmio.Map(0x078, noi.ReadRegister1, noi.WriteRegister1);
            mmio.Map(0x079, noi.ReadRegister2, noi.WriteRegister2);
            mmio.Map(0x07a, noi.ReadRegister3, noi.WriteRegister3);
            mmio.Map(0x07b, noi.ReadRegister4, noi.WriteRegister4);
            mmio.Map(0x07c, noi.ReadRegister5, noi.WriteRegister5);
            mmio.Map(0x07d, noi.ReadRegister6, noi.WriteRegister6);
            mmio.Map(0x07e, noi.ReadRegister7, noi.WriteRegister7);
            mmio.Map(0x07f, noi.ReadRegister8, noi.WriteRegister8);

            mmio.Map(0x080, ReadReg, Write080);
            mmio.Map(0x081, ReadReg, Write081);
            mmio.Map(0x082, ReadReg, Write082);
            mmio.Map(0x083, ReadReg, Write083);
            mmio.Map(0x084, Read084, Write084);
            mmio.Map(0x085, ReadReg, WriteReg);
            mmio.Map(0x086, ReadReg, WriteReg);
            mmio.Map(0x087, ReadReg, WriteReg);
            mmio.Map(0x088, ReadReg, Write088);
            mmio.Map(0x089, ReadReg, Write089);
            mmio.Map(0x08a, ReadReg, WriteReg);
            mmio.Map(0x08b, ReadReg, WriteReg);
            mmio.Map(0x08c, ReadReg, WriteReg);
            mmio.Map(0x08d, ReadReg, WriteReg);
            mmio.Map(0x08e, ReadReg, WriteReg);
            mmio.Map(0x08f, ReadReg, WriteReg);

            mmio.Map(0x090, 0x09f, wav.Read, wav.Write);

            mmio.Map(0x0a0, PCM1.WriteFifo);
            mmio.Map(0x0a1, PCM1.WriteFifo);
            mmio.Map(0x0a2, PCM1.WriteFifo);
            mmio.Map(0x0a3, PCM1.WriteFifo);

            mmio.Map(0x0a4, PCM2.WriteFifo);
            mmio.Map(0x0a5, PCM2.WriteFifo);
            mmio.Map(0x0a6, PCM2.WriteFifo);
            mmio.Map(0x0a7, PCM2.WriteFifo);

            PCM1.Initialize(dma.Channels[1]);
            PCM2.Initialize(dma.Channels[2]);
        }

        public void Update(int cycles)
        {
            courseTimer += cycles * 512;

            while (courseTimer >= Frequency)
            {
                courseTimer -= Frequency;

                switch (course)
                {
                case 0:
                case 4:
                    sq1.ClockDuration();
                    sq2.ClockDuration();
                    wav.ClockDuration();
                    noi.ClockDuration();
                    break;

                case 2:
                case 6:
                    sq1.ClockDuration();
                    sq2.ClockDuration();
                    wav.ClockDuration();
                    noi.ClockDuration();

                    sq1.ClockSweep();
                    break;

                case 7:
                    sq1.ClockEnvelope();
                    sq2.ClockEnvelope();
                    noi.ClockEnvelope();
                    break;
                }

                course = (course + 1) & 7;
            }

            sampleTimer += cycles * Single;

            while (sampleTimer >= Frequency)
            {
                sampleTimer -= Frequency;
                Sample();
            }
        }

        public void Consume(ClockSignal e)
        {
            Update(e.Cycles);
        }
    }
}
