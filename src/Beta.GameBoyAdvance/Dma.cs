using System;
using Beta.GameBoyAdvance.Memory;
using Beta.GameBoyAdvance.Messaging;
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

        private readonly MemoryMap memory;
        private readonly MMIO mmio;
        private readonly IProducer<InterruptSignal> interrupt;

        private ushort interruptType;

        private ushort controlRegister;
        private ushort lengthRegister;
        private uint sourceRegister;
        private uint targetRegister;
        private ushort length;
        private uint target;
        private uint targetStep;
        private uint source;
        private uint sourceStep;

        private bool Reload;

        public bool Pending;

        public bool Enabled { get { return (controlRegister & 0x8000u) != 0; } }
        public uint Type { get { return (controlRegister & 0x3000u); } }

        public Dma(MemoryMap memory, MMIO mmio, IProducer<InterruptSignal> interrupt, ushort interruptType)
        {
            this.memory = memory;
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
                var data = memory.Read(size, source);
                memory.Write(size, target, data);

                target += targetStep;
                source += sourceStep;
            }
            while (--count != 0);
        }

        #region Registers

        private byte ReadControl_0(uint address)
        {
            return (byte)((controlRegister >> 0) & 0xe0);
        }

        private byte ReadControl_1(uint address)
        {
            switch (address)
            {
            case 0xba+1: return (byte)((controlRegister >> 8) & 0xf7);
            case 0xc6+1: return (byte)((controlRegister >> 8) & 0xf7);
            case 0xd2+1: return (byte)((controlRegister >> 8) & 0xf7);
            case 0xde+1: return (byte)((controlRegister >> 8) & 0xff);
            }

            throw new ArgumentOutOfRangeException();
        }

        private void WriteControl_0(uint address, byte data)
        {
            controlRegister &= 0xff00;
            controlRegister |= data;
        }

        private void WriteControl_1(uint address, byte data)
        {
            int prev = (controlRegister >> 8) & 0x80;
            int next = (data >> 0) & 0x80;

            if (prev < next)
            {
                length = lengthRegister;
                target = targetRegister;
                source = sourceRegister;

                if ((data & 0x30) == 0x00) Pending = true;
            }

            controlRegister &= 0x00ff;
            controlRegister |= (ushort)(data << 8);
        }

        private void WriteCounter_0(uint address, byte data)
        {
            lengthRegister &= 0xff00;
            lengthRegister |= data;
        }

        private void WriteCounter_1(uint address, byte data)
        {
            lengthRegister &= 0x00ff;
            lengthRegister |= (ushort)(data << 8);
        }

        private void WriteDstAddr_0(uint address, byte data)
        {
            targetRegister &= 0xffffff00;
            targetRegister |= data;
        }

        private void WriteDstAddr_1(uint address, byte data)
        {
            targetRegister &= 0xffff00ff;
            targetRegister |= (uint)(data << 8);
        }

        private void WriteDstAddr_2(uint address, byte data)
        {
            targetRegister &= 0xff00ffff;
            targetRegister |= (uint)(data << 16);
        }

        private void WriteDstAddr_3(uint address, byte data)
        {
            targetRegister &= 0x00ffffff;
            targetRegister |= (uint)(data << 24);
        }

        private void WriteSrcAddr_0(uint address, byte data)
        {
            sourceRegister &= 0xffffff00;
            sourceRegister |= data;
        }

        private void WriteSrcAddr_1(uint address, byte data)
        {
            sourceRegister &= 0xffff00ff;
            sourceRegister |= (uint)(data << 8);
        }

        private void WriteSrcAddr_2(uint address, byte data)
        {
            sourceRegister &= 0xff00ffff;
            sourceRegister |= (uint)(data << 16);
        }

        private void WriteSrcAddr_3(uint address, byte data)
        {
            sourceRegister &= 0x00ffffff;
            sourceRegister |= (uint)(data << 24);
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
            targetStep = stepLut[(controlRegister >> 5) & 3u];
            sourceStep = stepLut[(controlRegister >> 7) & 3u];

            var count = (lengthRegister);
            var width = (controlRegister & 0x0400) != 0 ? 2 : 1;

            if (Type == SPECIAL)
            {
                targetStep = 0;
                count = 4;
                width = 2;
            }

            Transfer(width, count);

            if ((controlRegister & 0x0060) == 0x0060) { target = targetRegister; }
            if ((controlRegister & 0x0200) == 0x0000) { controlRegister &= 0x7fff; }
            if ((controlRegister & 0x0200) == 0x0200) { length = controlRegister; }

            if ((controlRegister & 0x4000) != 0)
            {
                interrupt.Produce(new InterruptSignal(interruptType));
            }
        }
    }
}
