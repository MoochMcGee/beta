using Beta.Famicom.APU;
using Beta.Platform.Audio;
using Beta.Platform.Processors.RP6502;

namespace Beta.Famicom.CPU
{
    public static class R2A03
    {
        private static void ReadInternal(int address, ref byte data)
        {
        }

        private static void WriteInternal(int address, byte data)
        {
        }

        public static void Update(R2A03State e)
        {
            R6502.Update(e.r6502);

            if (e.dma_trigger)
            {
                e.dma_trigger = false;

                var address = e.dma_segment << 8;
                var data = default(byte);

                for (var i = 0; i < 256; i++)
                {
                    ReadInternal(address, ref data);
                    WriteInternal(0x2004, data);

                    address++;
                }
            }
        }

        public static void IRQ(R2A03State e, int signal)
        {
            Interrupts.IRQ(e.r6502.ints, signal);
        }

        public static void NMI(R2A03State e, int signal)
        {
            Interrupts.NMI(e.r6502.ints, signal);
        }

        public static void Tick(R2A03State e, IAudioBackend audio)
        {
            if (e.sequence_mode == 0)
            {
                switch (e.sequence_time)
                {
                case     0: /*             */ /*             */ SequencerInterrupt(e); break;
                case  7457: QuadFrameTick(e); /*             */ break;
                case 14913: QuadFrameTick(e); HalfFrameTick(e); break;
                case 22371: QuadFrameTick(e); /*             */ break;
                case 29828: /*             */ /*             */ SequencerInterrupt(e); break;
                case 29829: QuadFrameTick(e); HalfFrameTick(e); SequencerInterrupt(e); break;
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
                case  7457: QuadFrameTick(e); /*             */ break;
                case 14913: QuadFrameTick(e); HalfFrameTick(e); break;
                case 22371: QuadFrameTick(e); /*             */ break;
                case 29829: /*             */ /*             */ break;
                case 37281: QuadFrameTick(e); HalfFrameTick(e); break;
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

            Mixer.Tick(e, audio);
        }

        public static void SequencerInterrupt(R2A03State e)
        {
            if (e.sequence_irq_enabled)
            {
                e.sequence_irq_pending = true;
                IRQ(e, 1);
            }
        }

        public static void HalfFrameTick(R2A03State e)
        {
            Duration.tick(e.sq1.duration);
            Duration.tick(e.sq2.duration);
            Duration.tick(e.tri.duration);
            Duration.tick(e.noi.duration);

            var sq1 = e.sq1;
            sq1.period = Sweep.Tick(sq1.sweep, sq1.period, ~sq1.period);

            var sq2 = e.sq2;
            sq2.period = Sweep.Tick(sq2.sweep, sq2.period, -sq2.period);
        }

        public static void QuadFrameTick(R2A03State e)
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
