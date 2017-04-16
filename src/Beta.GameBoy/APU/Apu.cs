using Beta.GameBoy.Memory;
using Beta.Platform.Audio;

namespace Beta.GameBoy.APU
{
    public static class Apu
    {
        public static void tick(ApuState apu, MemoryMap memory, IAudioBackend audio)
        {
            apu.sample_timer -= 48000;
            if (apu.sample_timer <= 0)
            {
                apu.sample_timer += apu.sample_period;
                Mixer.RenderSample(apu, audio);
            }

            if (apu.sequence_timer != 0 && --apu.sequence_timer == 0)
            {
                apu.sequence_timer = 4194304 / 512;

                switch (apu.sequence_step)
                {
                case 0: durationTick(apu); break;
                case 1: break;
                case 2: durationTick(apu); Sweep.Tick(apu.sq1); break;
                case 3: break;
                case 4: durationTick(apu); break;
                case 5: break;
                case 6: durationTick(apu); Sweep.Tick(apu.sq1); break;
                case 7: envelopeTick(apu); break;
                }

                apu.sequence_step = (apu.sequence_step + 1) & 7;
            }

            tickSq1(apu.sq1);
            tickSq2(apu.sq2);
            tickWav(apu.wav, memory);
            tickNoi(apu.noi);
        }

        static void tickSq1(Sq1State e)
        {
            if (e.enabled && e.timer != 0 && --e.timer == 0)
            {
                e.timer = (2048 - e.period) * 4;
                e.duty_step = (e.duty_step + 1) & 7;
            }
        }

        static void tickSq2(Sq2State e)
        {
            if (e.enabled && e.timer != 0 && --e.timer == 0)
            {
                e.timer = (2048 - e.period) * 4;
                e.duty_step = (e.duty_step + 1) & 7;
            }
        }

        static void tickWav(WavState e, MemoryMap memory)
        {
            if (e.enabled && e.timer != 0 && --e.timer == 0)
            {
                e.timer = (2048 - e.period) * 2;
                e.wave_ram_shift ^= 4;

                if (e.wave_ram_shift == 0)
                {
                    e.wave_ram_cursor = (e.wave_ram_cursor + 1) & 15;

                    var address = (ushort)(0xff30 | e.wave_ram_cursor);
                    e.wave_ram_sample = memory.Read(address);
                }

                e.wave_ram_output = (e.wave_ram_sample >> e.wave_ram_shift) & 15;
            }
        }

        static void tickNoi(NoiState e)
        {
            if (e.enabled && e.timer != 0 && --e.timer == 0)
            {
                e.timer = e.period;

                var tap0 = (e.lfsr >> 0) & 1;
                var tap1 = (e.lfsr >> 1) & 1;
                var next = (tap0 ^ tap1);

                e.lfsr = e.lfsr >> 1;
                e.lfsr = e.lfsr_mode == 1
                    ? (e.lfsr & ~0x4040) | (next * 0x4040)
                    : (e.lfsr & ~0x4000) | (next * 0x4000);
            }
        }

        static void durationTick(ApuState e)
        {
            if (Duration.Tick(e.sq1.duration)) e.sq1.enabled = false;
            if (Duration.Tick(e.sq2.duration)) e.sq2.enabled = false;
            if (Duration.Tick(e.wav.duration)) e.wav.enabled = false;
            if (Duration.Tick(e.noi.duration)) e.noi.enabled = false;
        }

        static void envelopeTick(ApuState e)
        {
            Envelope.Tick(e.sq1.envelope);
            Envelope.Tick(e.sq2.envelope);
            Envelope.Tick(e.noi.envelope);
        }
    }
}
