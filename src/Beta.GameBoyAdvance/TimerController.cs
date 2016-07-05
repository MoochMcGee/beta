using Beta.GameBoyAdvance.CPU;
using Beta.GameBoyAdvance.Memory;
using Beta.GameBoyAdvance.Messaging;
using Beta.Platform.Messaging;

namespace Beta.GameBoyAdvance
{
    public sealed class TimerController : IConsumer<ClockSignal>
    {
        private Timer[] timers;

        public TimerController(Driver driver, MMIO mmio, IProducer<InterruptSignal> interrupt)
        {
            timers = new[]
            {
                new Timer(driver, mmio, interrupt, Cpu.Source.TIMER_0, 0),
                new Timer(driver, mmio, interrupt, Cpu.Source.TIMER_1, 1),
                new Timer(driver, mmio, interrupt, Cpu.Source.TIMER_2, 2),
                new Timer(driver, mmio, interrupt, Cpu.Source.TIMER_3, 3)
            };

            timers[0].NextTimer = timers[1];
            timers[1].NextTimer = timers[2];
            timers[2].NextTimer = timers[3];
            timers[3].NextTimer = null;
        }

        public void Consume(ClockSignal e)
        {
            if (timers[0].Enabled) { timers[0].Update(e.Cycles); }
            if (timers[1].Enabled) { timers[1].Update(e.Cycles); }
            if (timers[2].Enabled) { timers[2].Update(e.Cycles); }
            if (timers[3].Enabled) { timers[3].Update(e.Cycles); }
        }

        public void Initialize()
        {
            timers[0].Initialize(0x100);
            timers[1].Initialize(0x104);
            timers[2].Initialize(0x108);
            timers[3].Initialize(0x10c);
        }
    }
}
