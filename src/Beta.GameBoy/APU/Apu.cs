using Beta.GameBoy.Memory;
using Beta.Platform.Messaging;

namespace Beta.GameBoy.APU
{
    public sealed class Apu : IConsumer<ClockSignal>
    {
        private readonly MemoryMap memory;
        private readonly Mixer mixer;
        private readonly ApuState apu;
        private readonly NoiState noi;
        private readonly Sq1State sq1;
        private readonly Sq2State sq2;
        private readonly WavState wav;

        public Apu(MemoryMap memory, Mixer mixer, State state)
        {
            this.memory = memory;
            this.mixer = mixer;

            this.apu = state.apu;
            this.noi = state.apu.noi;
            this.sq1 = state.apu.sq1;
            this.sq2 = state.apu.sq2;
            this.wav = state.apu.wav;
        }

        public void Consume(ClockSignal e)
        {
            for (int i = 0; i < e.Cycles; i++)
            {
                Tick();
            }
        }

        private void Tick()
        {
            apu.sample_timer -= 48000;
            if (apu.sample_timer <= 0)
            {
                apu.sample_timer += apu.sample_period;
                mixer.RenderSample();
            }

            if (apu.sequence_timer != 0 && --apu.sequence_timer == 0)
            {
                apu.sequence_timer = 4194304 / 512;

                switch (apu.sequence_step)
                {
                case 0: DurationTick(); break;
                case 1: break;
                case 2: DurationTick(); SweepManager.Tick(sq1); break;
                case 3: break;
                case 4: DurationTick(); break;
                case 5: break;
                case 6: DurationTick(); SweepManager.Tick(sq1); break;
                case 7: EnvelopeTick(); break;
                }

                apu.sequence_step = (apu.sequence_step + 1) & 7;
            }

            if (sq1.enabled && sq1.timer != 0 && --sq1.timer == 0)
            {
                sq1.timer = (2048 - sq1.period) * 4;
                sq1.duty_step = (sq1.duty_step + 1) & 7;
            }

            if (sq2.enabled && sq2.timer != 0 && --sq2.timer == 0)
            {
                sq2.timer = (2048 - sq2.period) * 4;
                sq2.duty_step = (sq2.duty_step + 1) & 7;
            }

            if (wav.enabled && wav.timer != 0 && --wav.timer == 0)
            {
                wav.timer = (2048 - wav.period) * 2;
                wav.wave_ram_shift ^= 4;

                if (wav.wave_ram_shift == 0)
                {
                    wav.wave_ram_cursor = (wav.wave_ram_cursor + 1) & 15;

                    var address = (ushort)(0xff30 | wav.wave_ram_cursor);
                    wav.wave_ram_sample = memory.Read(address);
                }

                wav.wave_ram_output = (wav.wave_ram_sample >> wav.wave_ram_shift) & 15;
            }

            if (noi.enabled && noi.timer != 0 && --noi.timer == 0)
            {
                noi.timer = noi.period;

                int feedback = (noi.lfsr ^ (noi.lfsr >> 1)) & 1;
                noi.lfsr = noi.lfsr >> 1;

                if (noi.lfsr_mode == 1)
                {
                    // the documentation says this is correct, but it sounds
                    // wrong.
                    // 
                    // noi.lfsr |= feedback << 14;
                    noi.lfsr |= feedback << 6;
                }
                else
                {
                    noi.lfsr |= feedback << 14;
                }
            }
        }

        private void DurationTick()
        {
            DurationManager.Tick(sq1.duration,  64, ref sq1.enabled);
            DurationManager.Tick(sq2.duration,  64, ref sq2.enabled);
            DurationManager.Tick(wav.duration, 256, ref wav.enabled);
            DurationManager.Tick(noi.duration,  64, ref noi.enabled);
        }

        private void EnvelopeTick()
        {
            EnvelopeManager.Tick(sq1.envelope);
            EnvelopeManager.Tick(sq2.envelope);
            EnvelopeManager.Tick(noi.envelope);
        }
    }
}
