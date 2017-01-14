using Beta.GameBoyAdvance.APU;
using Beta.GameBoyAdvance.CPU;
using Beta.GameBoyAdvance.Memory;
using Beta.GameBoyAdvance.Messaging;
using Beta.Platform.Messaging;

namespace Beta.GameBoyAdvance
{
    public sealed class TimerController : IConsumer<ClockSignal>
    {
        private static readonly int[] shiftLut = { 0, 6, 8, 10 };

        private readonly Apu apu;
        private readonly MMIO mmio;
        private readonly IProducer<InterruptSignal> interrupt;

        private Timer[] timers;
        private int counter;

        public TimerController(Apu apu, MMIO mmio, IProducer<InterruptSignal> interrupt)
        {
            this.apu = apu;
            this.mmio = mmio;
            this.interrupt = interrupt;

            timers = new Timer[4]
            {
                new Timer(Cpu.Source.Timer0),
                new Timer(Cpu.Source.Timer1),
                new Timer(Cpu.Source.Timer2),
                new Timer(Cpu.Source.Timer3)
            };
        }

        #region Registers

        private byte ReadCounter0(uint address)
        {
            var index = GetTimerIndex(address);

            return (byte)(timers[index].counter);
        }

        private byte ReadCounter1(uint address)
        {
            var index = GetTimerIndex(address);

            return (byte)(timers[index].counter >> 8);
        }

        private byte ReadControl0(uint address)
        {
            var index = GetTimerIndex(address);

            return (byte)(timers[index].control);
        }

        private byte ReadControl1(uint address)
        {
            var index = GetTimerIndex(address);

            return (byte)(timers[index].control >> 8);
        }

        private void WriteCounter0(uint address, byte value)
        {
            var index = GetTimerIndex(address);

            timers[index].refresh &= 0xff00;
            timers[index].refresh |= value;
        }

        private void WriteCounter1(uint address, byte value)
        {
            var index = GetTimerIndex(address);

            timers[index].refresh &= 0x00ff;
            timers[index].refresh |= value << 8;
        }

        private void WriteControl0(uint address, byte value)
        {
            var index = GetTimerIndex(address);

            if ((timers[index].control & 0x80) < (value & 0x80))
            {
                timers[index].counter = timers[index].refresh;
            }

            timers[index].control &= 0xff00;
            timers[index].control |= value;
        }

        private void WriteControl1(uint address, byte value)
        {
            var index = GetTimerIndex(address);

            timers[index].control &= 0x00ff;
            timers[index].control |= value << 8;
        }

        private static uint GetTimerIndex(uint address)
        {
            return (address >> 2) & 3;
        }

        #endregion

        public void Consume(ClockSignal e)
        {
            var prev = counter;
            var next = (counter + e.Cycles) & 0xffffff;

            for (int i = 0; i < 4; i++)
            {
                if ((timers[i].control & 0x84) == 0x80)
                {
                    var shift = shiftLut[timers[i].control & 3];
                    var tprev = prev >> shift;
                    var tnext = next >> shift;

                    var delta = tnext - tprev;
                    if (delta > 0)
                    {
                        ClockTimer(i, delta);
                    }
                }
            }

            counter = next;
        }

        private void ClockTimer(int n, int times)
        {
            var timer = timers[n];

            var counter = (timer.counter + times) & 0xffff;
            if (counter <= timer.counter)
            {
                counter += timer.refresh;

                if (apu.PCM1.Timer == n) { apu.PCM1.Clock(); }
                if (apu.PCM2.Timer == n) { apu.PCM2.Clock(); }

                if ((timer.control & 0x40) != 0)
                {
                    var irq = new InterruptSignal(timer.interrupt);
                    interrupt.Produce(irq);
                }

                if (n < 3 && (timers[n + 1].control & 0x84) == 0x84)
                {
                    ClockTimer(n + 1, 1);
                }
            }

            timer.counter = counter;
        }

        public void Initialize()
        {
            mmio.Map(0x100, ReadCounter0, WriteCounter0);
            mmio.Map(0x101, ReadCounter1, WriteCounter1);
            mmio.Map(0x102, ReadControl0, WriteControl0);
            mmio.Map(0x103, ReadControl1, WriteControl1);

            mmio.Map(0x104, ReadCounter0, WriteCounter0);
            mmio.Map(0x105, ReadCounter1, WriteCounter1);
            mmio.Map(0x106, ReadControl0, WriteControl0);
            mmio.Map(0x107, ReadControl1, WriteControl1);

            mmio.Map(0x108, ReadCounter0, WriteCounter0);
            mmio.Map(0x109, ReadCounter1, WriteCounter1);
            mmio.Map(0x10a, ReadControl0, WriteControl0);
            mmio.Map(0x10b, ReadControl1, WriteControl1);

            mmio.Map(0x10c, ReadCounter0, WriteCounter0);
            mmio.Map(0x10d, ReadCounter1, WriteCounter1);
            mmio.Map(0x10e, ReadControl0, WriteControl0);
            mmio.Map(0x10f, ReadControl1, WriteControl1);
        }
    }
}
