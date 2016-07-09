using System;
using Beta.GameBoy.Memory;
using Beta.Platform.Audio;
using Beta.Platform.Messaging;

namespace Beta.GameBoy.APU
{
    public sealed class Apu : IConsumer<ClockSignal>
    {
        private static int[][] square_lut = new[]
        {
            new[] { 0, 0, 0, 0, 0, 0, 0, 1 },
            new[] { 1, 0, 0, 0, 0, 0, 0, 1 },
            new[] { 1, 0, 0, 0, 0, 1, 1, 1 },
            new[] { 0, 1, 1, 1, 1, 1, 1, 0 }
        };

        private readonly MemoryMap memory;
        private readonly ApuRegisters regs;
        private readonly NoiRegisters noi;
        private readonly Sq1Registers sq1;
        private readonly Sq2Registers sq2;
        private readonly WavRegisters wav;
        private readonly IAudioBackend audio;

        private int sample_timer;
        private int sample_period = 1048576;

        public Apu(MemoryMap memory, Registers regs, IAudioBackend audio)
        {
            this.memory = memory;
            this.regs = regs.apu;
            this.noi = regs.noi;
            this.sq1 = regs.sq1;
            this.sq2 = regs.sq2;
            this.wav = regs.wav;
            this.audio = audio;
        }

        public void Consume(ClockSignal e)
        {
            for (int i = 0; i < e.Cycles / 4; i++)
            {
                Tick(e);
            }
        }

        private void Tick(ClockSignal e)
        {
            if (regs.sequence_timer != 0 && --regs.sequence_timer == 0)
            {
                regs.sequence_timer = 4194304 / 2048;

                switch (regs.sequence_step)
                {
                case 0: DurationTick(); break;
                case 1: break;
                case 2: DurationTick(); SweepTick(); break;
                case 3: break;
                case 4: DurationTick(); break;
                case 5: break;
                case 6: DurationTick(); SweepTick(); break;
                case 7: EnvelopeTick(); break;
                }

                regs.sequence_step = (regs.sequence_step + 1) & 7;
            }

            if (sq1.enabled && sq1.timer != 0 && --sq1.timer == 0)
            {
                sq1.timer = 2048 - sq1.period;
                sq1.duty_step = (sq1.duty_step + 1) & 7;
            }

            if (sq2.enabled && sq2.timer != 0 && --sq2.timer == 0)
            {
                sq2.timer = 2048 - sq2.period;
                sq2.duty_step = (sq2.duty_step + 1) & 7;
            }

            if (wav.enabled && wav.timer != 0 && --wav.timer == 0)
            {
                wav.timer = (2048 - wav.period) / 2;

                if (wav.wave_ram_shift == 0)
                {
                    wav.wave_ram_shift = 4;
                    wav.wave_ram_cursor = (wav.wave_ram_cursor + 1) & 15;

                    var address = (ushort)(0xff30 | wav.wave_ram_cursor);
                    wav.wave_ram_sample = memory.Read(address);
                }
                else
                {
                    wav.wave_ram_shift = 0;
                }
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

            sample_timer -= 48000;
            if (sample_timer <= 0)
            {
                sample_timer += sample_period;
                RenderSample();
            }
        }

        private void DurationTick()
        {
            if (sq1.duration != 0 && --sq1.duration == 0)
            {
                if (sq1.duration_loop)
                {
                    sq1.duration = 64 - sq1.duration_latch;
                }
                else
                {
                    sq1.enabled = false;
                }
            }

            if (sq2.duration != 0 && --sq2.duration == 0)
            {
                if (sq2.duration_loop)
                {
                    sq2.duration = 64 - sq2.duration_latch;
                }
                else
                {
                    sq2.enabled = false;
                }
            }

            if (wav.duration != 0 && --wav.duration == 0)
            {
                if (wav.duration_loop)
                {
                    wav.duration = 256 - wav.duration_latch;
                }
                else
                {
                    wav.enabled = false;
                }
            }

            if (noi.duration != 0 && --noi.duration == 0)
            {
                if (noi.duration_loop)
                {
                    noi.duration = 64 - noi.duration_latch;
                }
                else
                {
                    noi.enabled = false;
                }
            }
        }

        private void EnvelopeTick()
        {
            if (sq1.volume_period != 0 && sq1.volume_timer != 0 && --sq1.volume_timer == 0)
            {
                sq1.volume_timer = sq1.volume_period;
                sq1.volume = sq1.volume_direction == 0
                    ? Math.Max(sq1.volume - 1, 0)
                    : Math.Min(sq1.volume + 1, 15)
                    ;
            }

            if (sq2.volume_period != 0 && sq2.volume_timer != 0 && --sq2.volume_timer == 0)
            {
                sq2.volume_timer = sq2.volume_period;
                sq2.volume = sq2.volume_direction == 0
                    ? Math.Max(sq2.volume - 1, 0)
                    : Math.Min(sq2.volume + 1, 15)
                    ;
            }

            if (noi.volume_period != 0 && noi.volume_timer != 0 && --noi.volume_timer == 0)
            {
                noi.volume_timer = noi.volume_period;
                noi.volume = noi.volume_direction == 0
                    ? Math.Max(noi.volume - 1, 0)
                    : Math.Min(noi.volume + 1, 15)
                    ;
            }
        }

        private void SweepTick()
        {
            if (sq1.sweep_enabled)
            {
                if (sq1.sweep_timer != 0 && --sq1.sweep_timer == 0)
                {
                    sq1.sweep_timer = sq1.sweep_period;

                    int period = sq1.sweep_direction == 0
                        ? sq1.period + (sq1.period >> sq1.sweep_shift)
                        : sq1.period - (sq1.period >> sq1.sweep_shift)
                        ;

                    if (period < 0)
                    {
                        period = 0;
                    }

                    if (period > 0x7ff)
                    {
                        period = 0x7ff;
                        sq1.enabled = false;
                    }

                    sq1.period = period;
                }
            }
        }

        private void RenderSample()
        {
            int sq1_out = sq1.enabled
                ? square_lut[sq1.duty_form][sq1.duty_step] * sq1.volume
                : 0
                ;

            int sq2_out = sq2.enabled
                ? square_lut[sq2.duty_form][sq2.duty_step] * sq2.volume
                : 0
                ;

            int noi_out = noi.enabled
                ? (~noi.lfsr & 1) * noi.volume
                : 0
                ;

            int wav_out = wav.enabled
                ? ((wav.wave_ram_sample >> wav.wave_ram_shift) & 0xf) >> wav.volume_shift
                : 0
                ;

            for (int i = 0; i < 2; i++)
            {
                int sample = 0;

                if ((regs.speaker_select[i] & 8) != 0) sample += noi_out;
                if ((regs.speaker_select[i] & 4) != 0) sample += wav_out;
                if ((regs.speaker_select[i] & 2) != 0) sample += sq2_out;
                if ((regs.speaker_select[i] & 1) != 0) sample += sq1_out;

                // apply volume correction

                sample = (sample * 32767 * regs.speaker_volume[i]) / 16 / 4 / 8;
                audio.Render(sample);
            }
        }
    }
}
