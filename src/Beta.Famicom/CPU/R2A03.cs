﻿using Beta.Famicom.APU;
using Beta.Famicom.PPU;
using Beta.Platform.Audio;
using Beta.Platform.Processors.RP6502;
using Beta.Platform.Video;

namespace Beta.Famicom.CPU
{
    public static class R2A03
    {
        public static void irq(R2A03State e, int signal)
        {
            Interrupt.irq(e.r6502.ints, signal);
        }

        public static void nmi(R2A03State e, int signal)
        {
            Interrupt.nmi(e.r6502.ints, signal);
        }

        public static void tick(State e, IAudioBackend audio, IVideoBackend video)
        {
            R6502.update(e.r2a03.r6502);

            if (e.r2a03.dma_trigger)
            {
                e.r2a03.dma_trigger = false;

                var address = (ushort)(e.r2a03.dma_segment << 8);
                var data = default(byte);

                for (var i = 0; i < 256; i++)
                {
                    R6502.read(e.r2a03.r6502, address);
                    R6502.write(e.r2a03.r6502, 0x2004, data);

                    address++;
                }
            }

            tickR2A03(e.r2a03, audio);

            R2C02.tick(e.r2c02, video);
            R2C02.tick(e.r2c02, video);
            R2C02.tick(e.r2c02, video);

            Interrupt.nmi(
                e.r2a03.r6502.ints,
                e.r2c02.vbl_enabled & e.r2c02.vbl_flag);
        }

        public static void tickR2A03(R2A03State e, IAudioBackend audio)
        {
            if (e.sequence_mode == 0)
            {
                switch (e.sequence_time)
                {
                case     0: /*             */ /*             */ sequencerInterrupt(e); break;
                case  7457: quadFrameTick(e); /*             */ break;
                case 14913: quadFrameTick(e); halfFrameTick(e); break;
                case 22371: quadFrameTick(e); /*             */ break;
                case 29828: /*             */ /*             */ sequencerInterrupt(e); break;
                case 29829: quadFrameTick(e); halfFrameTick(e); sequencerInterrupt(e); break;
                }

                e.sequence_time++;
                if (e.sequence_time == 29830)
                {
                    e.sequence_time = 0;
                }
            }
            else
            {
                switch (e.sequence_time)
                {
                case  7457: quadFrameTick(e); /*             */ break;
                case 14913: quadFrameTick(e); halfFrameTick(e); break;
                case 22371: quadFrameTick(e); /*             */ break;
                case 29829: /*             */ /*             */ break;
                case 37281: quadFrameTick(e); halfFrameTick(e); break;
                }

                e.sequence_time++;
                if (e.sequence_time == 37282)
                {
                    e.sequence_time = 0;
                }
            }

            Sq1.tick(e.sq1);
            Sq2.tick(e.sq2);
            Tri.tick(e.tri);
            Noi.tick(e.noi);
            Dmc.tick(e.dmc);

            Mixer.tick(e, audio);
        }

        public static void sequencerInterrupt(R2A03State e)
        {
            if (e.sequence_irq_enabled)
            {
                e.sequence_irq_pending = true;
                irq(e, 1);
            }
        }

        public static void halfFrameTick(R2A03State e)
        {
            Duration.tick(e.sq1.duration);
            Duration.tick(e.sq2.duration);
            Duration.tick(e.tri.duration);
            Duration.tick(e.noi.duration);

            var sq1 = e.sq1;
            sq1.period = Sweep.tick(sq1.sweep, sq1.period, ~sq1.period);

            var sq2 = e.sq2;
            sq2.period = Sweep.tick(sq2.sweep, sq2.period, -sq2.period);
        }

        public static void quadFrameTick(R2A03State e)
        {
            Envelope.tick(e.sq1.envelope);
            Envelope.tick(e.sq2.envelope);
            Envelope.tick(e.noi.envelope);

            var tri = e.tri;
            if (tri.linear_counter_reload)
            {
                tri.linear_counter = tri.linear_counter_latch;
            }
            else
            {
                if (tri.linear_counter != 0)
                {
                    tri.linear_counter--;
                }
            }

            if (tri.linear_counter_control == false)
            {
                tri.linear_counter_reload = false;
            }
        }
    }
}
