using Beta.GameBoy.APU;
using Beta.GameBoy.Memory;
using Beta.GameBoy.Messaging;
using Beta.Platform.Audio;
using Beta.Platform.Messaging;
using Beta.Platform.Processors.LR35902;

namespace Beta.GameBoy.CPU
{
    public class Cpu : Core
    {
        public const byte INT_VBLANK = (1 << 0);
        public const byte INT_STATUS = (1 << 1);
        public const byte INT_ELAPSE = (1 << 2);
        public const byte INT_SERIAL = (1 << 3);
        public const byte INT_JOYPAD = (1 << 4);

        private readonly IAudioBackend audio;
        private readonly IProducer<ClockSignal> clock;
        private readonly MemoryMap memory;
        private readonly CpuState cpu;
        private readonly State state;

        public Cpu(State state, MemoryMap memory, IAudioBackend audio, IProducer<ClockSignal> clock)
        {
            this.state = state;
            this.cpu = state.cpu;
            this.memory = memory;
            this.audio = audio;
            this.clock = clock;
        }

        protected override void Dispatch()
        {
            if (interrupt.ff2 == 1)
            {
                interrupt.ff2 = 0;
                interrupt.ff1 = 1;
            }

            Apu.tick(state.apu, memory, audio);
            Apu.tick(state.apu, memory, audio);
            Apu.tick(state.apu, memory, audio);
            Apu.tick(state.apu, memory, audio);

            int tma = Tma.tick(state.tma);
            if (tma == 1) Interrupt(INT_ELAPSE);

            clock.Produce(new ClockSignal(4));
        }

        protected override byte Read(ushort address)
        {
            Dispatch();

            return memory.Read(address);
        }

        protected override void Write(ushort address, byte data)
        {
            Dispatch();

            memory.Write(address, data);
        }

        public override void Update()
        {
            base.Update();

            var flags = (cpu.irf & cpu.ief) & -interrupt.ff1;
            if (flags != 0)
            {
                interrupt.ff1 = 0;

                if ((flags & 0x01) != 0) { cpu.irf ^= 0x01; Rst(0x40); return; }
                if ((flags & 0x02) != 0) { cpu.irf ^= 0x02; Rst(0x48); return; }
                if ((flags & 0x04) != 0) { cpu.irf ^= 0x04; Rst(0x50); return; }
                if ((flags & 0x08) != 0) { cpu.irf ^= 0x08; Rst(0x58); return; }
                if ((flags & 0x10) != 0) { cpu.irf ^= 0x10; Rst(0x60); return; }
            }
        }

        public void Consume(InterruptSignal e)
        {
            Interrupt((byte)e.Flag);
        }

        private void Interrupt(byte flag)
        {
            cpu.irf |= flag;

            if ((cpu.ief & flag) != 0)
            {
                halt = false;

                if (flag == INT_JOYPAD)
                {
                    stop = false;
                }
            }
        }
    }
}
