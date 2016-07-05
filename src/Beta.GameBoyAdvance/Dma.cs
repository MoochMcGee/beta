using Beta.GameBoyAdvance.Memory;
using Beta.GameBoyAdvance.Messaging;
using Beta.Platform;
using Beta.Platform.Messaging;

namespace Beta.GameBoyAdvance
{
    public sealed class Dma
    {
        public const int IMMEDIATE = 0x0000;
        public const int V_BLANK = 0x1000;
        public const int H_BLANK = 0x2000;
        public const int SPECIAL = 0x3000;

        private static uint[] stepLut = new[]
        {
            1U, ~0U, 0U, 1U
        };

        private readonly IProducer<InterruptSignal> interrupt;
        private readonly MMIO mmio;

        private Driver driver;
        private ushort interruptType;

        private Register16 controlRegister;
        private Register16 lengthRegister;
        private Register32 sourceRegister;
        private Register32 targetRegister;
        private ushort length;
        private uint target;
        private uint targetStep;
        private uint source;
        private uint sourceStep;

        private bool Reload;

        public bool Pending;

        public bool Enabled { get { return (controlRegister.w & 0x8000u) != 0; } }
        public uint Type { get { return (controlRegister.w & 0x3000u); } }

        public Dma(Driver driver, MMIO mmio, IProducer<InterruptSignal> interrupt, ushort interruptType)
        {
            this.driver = driver;
            this.mmio = mmio;
            this.interrupt = interrupt;
            this.interruptType = interruptType;
        }

        private void Transfer(int size, ushort count)
        {
            targetStep <<= size;
            sourceStep <<= size;

            do
            {
                int cycles;

                var data = driver.Read(size, source, out cycles);
                driver.Cpu.Cycles += cycles;

                driver.Write(size, target, data, out cycles);
                driver.Cpu.Cycles += cycles;

                target += targetStep;
                source += sourceStep;
            }
            while (--count != 0);
        }

        #region Registers

        private byte ReadControl_0(uint address)
        {
            return controlRegister.l;
        }

        private byte ReadControl_1(uint address)
        {
            return controlRegister.h;
        }

        private void WriteControl_0(uint address, byte data)
        {
            controlRegister.l = data;
        }

        private void WriteControl_1(uint address, byte data)
        {
            if (controlRegister.h < (data & 0x80))
            {
                length = lengthRegister.w;
                target = targetRegister.ud0;
                source = sourceRegister.ud0;

                if ((data & 0x30) == 0x00) Pending = true;
            }

            controlRegister.h = data;
        }

        private void WriteCounter_0(uint address, byte data)
        {
            lengthRegister.l = data;
        }

        private void WriteCounter_1(uint address, byte data)
        {
            lengthRegister.h = data;
        }

        private void WriteDstAddr_0(uint address, byte data)
        {
            targetRegister.ub0 = data;
        }

        private void WriteDstAddr_1(uint address, byte data)
        {
            targetRegister.ub1 = data;
        }

        private void WriteDstAddr_2(uint address, byte data)
        {
            targetRegister.ub2 = data;
        }

        private void WriteDstAddr_3(uint address, byte data)
        {
            targetRegister.ub3 = data;
        }

        private void WriteSrcAddr_0(uint address, byte data)
        {
            sourceRegister.ub0 = data;
        }

        private void WriteSrcAddr_1(uint address, byte data)
        {
            sourceRegister.ub1 = data;
        }

        private void WriteSrcAddr_2(uint address, byte data)
        {
            sourceRegister.ub2 = data;
        }

        private void WriteSrcAddr_3(uint address, byte data)
        {
            sourceRegister.ub3 = data;
        }

        #endregion

        public void Initialize(uint address)
        {
            mmio.Map(address + 0x0u, WriteSrcAddr_0); // $40000B0 - 32 - DMA0SAD
            mmio.Map(address + 0x1u, WriteSrcAddr_1);
            mmio.Map(address + 0x2u, WriteSrcAddr_2);
            mmio.Map(address + 0x3u, WriteSrcAddr_3);
            mmio.Map(address + 0x4u, WriteDstAddr_0); // $40000B4 - 32 - DMA0DAD
            mmio.Map(address + 0x5u, WriteDstAddr_1);
            mmio.Map(address + 0x6u, WriteDstAddr_2);
            mmio.Map(address + 0x7u, WriteDstAddr_3);
            mmio.Map(address + 0x8u, WriteCounter_0); // $40000B8 - 16 - DMA0CNT_L
            mmio.Map(address + 0x9u, WriteCounter_1);
            mmio.Map(address + 0xau, ReadControl_0, WriteControl_0); // $40000BA - 16 - DMA0CNT_H
            mmio.Map(address + 0xbu, ReadControl_1, WriteControl_1);
        }

        public void Transfer()
        {
            targetStep = stepLut[(controlRegister.w >> 5) & 3u];
            sourceStep = stepLut[(controlRegister.w >> 7) & 3u];

            var count = (lengthRegister.w);
            var width = (controlRegister.w & 0x0400) != 0 ? 2 : 1;

            if (Type == SPECIAL)
            {
                targetStep = 0;
                count = 4;
                width = 2;
            }

            Transfer(width, count);

            if ((controlRegister.w & 0x0060) == 0x0060) { target = targetRegister.ud0; }
            if ((controlRegister.w & 0x0200) == 0x0000) { controlRegister.w &= 0x7fff; }
            if ((controlRegister.w & 0x0200) == 0x0200) { length = controlRegister.w; }

            if ((controlRegister.w & 0x4000) != 0)
            {
                interrupt.Produce(new InterruptSignal(interruptType));
            }
        }
    }
}
