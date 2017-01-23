using System;
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
        private readonly ApuState state = new ApuState();

        private byte[] registers = new byte[16];
        private int course_timer;
        private int sample_timer;
        private int bias;
        private int course;
        private int psg_shift;
        private int[] volume = new int[2];

        public readonly ChannelSQ1 sound_1;
        public readonly ChannelSQ2 sound_2;
        public readonly ChannelWAV sound_3;
        public readonly ChannelNOI sound_4;
        public readonly ChannelPCM sound_a;
        public readonly ChannelPCM sound_b;

        public Apu(DmaController dma, MMIO mmio, IAudioBackend audio)
        {
            this.audio = audio;

            sound_1 = new ChannelSQ1(state);
            sound_2 = new ChannelSQ2(state);
            sound_3 = new ChannelWAV(state);
            sound_4 = new ChannelNOI(state);
            sound_a = new ChannelPCM(dma.Channels[1]);
            sound_b = new ChannelPCM(dma.Channels[2]);

            mmio.Map(0x060, sound_1.ReadReg, sound_1.Write060);
            mmio.Map(0x061, sound_1.ReadReg, sound_1.Write061);
            mmio.Map(0x062, sound_1.ReadReg, sound_1.Write062);
            mmio.Map(0x063, sound_1.ReadReg, sound_1.Write063);
            mmio.Map(0x064, sound_1.ReadReg, sound_1.Write064);
            mmio.Map(0x065, sound_1.ReadReg, sound_1.Write065);
            mmio.Map(0x066, sound_1.ReadReg, sound_1.Write066);
            mmio.Map(0x067, sound_1.ReadReg, sound_1.Write067);

            mmio.Map(0x068, sound_2.ReadReg, sound_2.Write068);
            mmio.Map(0x069, sound_2.ReadReg, sound_2.Write069);
            mmio.Map(0x06a, sound_2.ReadReg, sound_2.Write06A);
            mmio.Map(0x06b, sound_2.ReadReg, sound_2.Write06B);
            mmio.Map(0x06c, sound_2.ReadReg, sound_2.Write06C);
            mmio.Map(0x06d, sound_2.ReadReg, sound_2.Write06D);
            mmio.Map(0x06e, sound_2.ReadReg, sound_2.Write06E);
            mmio.Map(0x06f, sound_2.ReadReg, sound_2.Write06F);

            mmio.Map(0x070, sound_3.ReadReg, sound_3.Write070);
            mmio.Map(0x071, sound_3.ReadReg, sound_3.Write071);
            mmio.Map(0x072, sound_3.ReadReg, sound_3.Write072);
            mmio.Map(0x073, sound_3.ReadReg, sound_3.Write073);
            mmio.Map(0x074, sound_3.ReadReg, sound_3.Write074);
            mmio.Map(0x075, sound_3.ReadReg, sound_3.Write075);
            mmio.Map(0x076, sound_3.ReadReg, sound_3.Write076);
            mmio.Map(0x077, sound_3.ReadReg, sound_3.Write077);

            mmio.Map(0x078, sound_4.ReadReg, sound_4.Write078);
            mmio.Map(0x079, sound_4.ReadReg, sound_4.Write079);
            mmio.Map(0x07a, sound_4.ReadReg, sound_4.Write07A);
            mmio.Map(0x07b, sound_4.ReadReg, sound_4.Write07B);
            mmio.Map(0x07c, sound_4.ReadReg, sound_4.Write07C);
            mmio.Map(0x07d, sound_4.ReadReg, sound_4.Write07D);
            mmio.Map(0x07e, sound_4.ReadReg, sound_4.Write07E);
            mmio.Map(0x07f, sound_4.ReadReg, sound_4.Write07F);

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

            mmio.Map(0x090, 0x09f, sound_3.Read, sound_3.Write);

            mmio.Map(0x0a0, sound_a.Write);
            mmio.Map(0x0a1, sound_a.Write);
            mmio.Map(0x0a2, sound_a.Write);
            mmio.Map(0x0a3, sound_a.Write);

            mmio.Map(0x0a4, sound_b.Write);
            mmio.Map(0x0a5, sound_b.Write);
            mmio.Map(0x0a6, sound_b.Write);
            mmio.Map(0x0a7, sound_b.Write);
        }

        #region Registers

        private byte ReadReg(uint address) => registers[address & 15];

        private void WriteReg(uint address, byte data) { }

        private byte Read084(uint address)
        {
            var data = registers[4];

            data &= 0x80;

            if (state.sound_1.active) data |= 0x01;
            if (state.sound_2.active) data |= 0x02;
            if (state.sound_3.active) data |= 0x04;
            if (state.sound_4.active) data |= 0x08;

            return data;
        }

        private void Write080(uint address, byte data)
        {
            registers[0] = data &= 0x77;

            volume[1] = ((data >> 0) & 7) + 1;
            volume[0] = ((data >> 4) & 7) + 1;
        }

        private void Write081(uint address, byte data)
        {
            registers[1] = data;

            state.sound_1.output[1] = (data & 0x01) != 0;
            state.sound_2.output[1] = (data & 0x02) != 0;
            state.sound_3.output[1] = (data & 0x04) != 0;
            state.sound_4.output[1] = (data & 0x08) != 0;

            state.sound_1.output[0] = (data & 0x10) != 0;
            state.sound_2.output[0] = (data & 0x20) != 0;
            state.sound_3.output[0] = (data & 0x40) != 0;
            state.sound_4.output[0] = (data & 0x80) != 0;
        }

        private void Write082(uint address, byte data)
        {
            registers[2] = data &= 0x0f;

            psg_shift = data & 3;
            sound_a.shift = (~data >> 2) & 1;
            sound_b.shift = (~data >> 3) & 1;
        }

        private void Write083(uint address, byte data)
        {
            registers[3] = data &= 0x77;

            sound_a.output[1] = (data & 0x01) != 0;
            sound_b.output[1] = (data & 0x10) != 0;

            sound_a.output[0] = (data & 0x02) != 0;
            sound_b.output[0] = (data & 0x20) != 0;

            sound_a.timer = (data >> 2) & 1;
            sound_b.timer = (data >> 6) & 1;

            if ((data & 0x08) != 0) sound_a.Reset();
            if ((data & 0x80) != 0) sound_b.Reset();
        }

        private void Write084(uint address, byte data)
        {
            registers[4] = data;
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

        public void Consume(ClockSignal e)
        {
            course_timer += e.Cycles * 512;

            while (course_timer >= Frequency)
            {
                course_timer -= Frequency;

                switch (course)
                {
                case 0:
                case 4:
                    sound_1.ClockDuration();
                    sound_2.ClockDuration();
                    sound_3.ClockDuration();
                    sound_4.ClockDuration();
                    break;

                case 2:
                case 6:
                    sound_1.ClockDuration();
                    sound_2.ClockDuration();
                    sound_3.ClockDuration();
                    sound_4.ClockDuration();

                    sound_1.ClockSweep();
                    break;

                case 7:
                    sound_1.ClockEnvelope();
                    sound_2.ClockEnvelope();
                    sound_4.ClockEnvelope();
                    break;
                }

                course = (course + 1) & 7;
            }

            sample_timer += e.Cycles * Single;

            while (sample_timer >= Frequency)
            {
                sample_timer -= Frequency;
                Sample();
            }
        }

        private void Sample()
        {
            var sq1 = sound_1.Render(Frequency);
            var sq2 = sound_2.Render(Frequency);
            var wav = sound_3.Render(Frequency);
            var noi = sound_4.Render(Frequency);

            for (int i = 0; i < 2; i++)
            {
                var sample = // 0x200 - bias;
                    GetPSGOutput(i, sq1, sq2, wav, noi) +
                    GetSoundAOutput(i) +
                    GetSoundBOutput(i);

                audio.Render(Clamp(sample) * 64);
            }
        }

        private int GetPSGOutput(int speaker, int sq1, int sq2, int wav, int noi)
        {
            int sample = 0;
            if (state.sound_1.output[speaker]) sample += sq1;
            if (state.sound_2.output[speaker]) sample += sq2;
            if (state.sound_3.output[speaker]) sample += wav;
            if (state.sound_4.output[speaker]) sample += noi;

            switch (psg_shift)
            {
            case 0: return (sample * volume[speaker]) >> 2;
            case 1: return (sample * volume[speaker]) >> 1;
            case 2: return (sample * volume[speaker]) >> 0;
            case 3: return 0;
            }

            throw new ArgumentOutOfRangeException();
        }

        private int GetSoundAOutput(int speaker)
        {
            return sound_a.output[speaker]
                ? sound_a.level >> sound_a.shift
                : 0;
        }

        private int GetSoundBOutput(int speaker)
        {
            return sound_b.output[speaker]
                ? sound_b.level >> sound_b.shift
                : 0;
        }

        private static int Clamp(int n)
        {
            const int min = ~511;
            const int max = 511;

            if (n < min) return min;
            if (n > max) return max;
            return n;
        }
    }
}
