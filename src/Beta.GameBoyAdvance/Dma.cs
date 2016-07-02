using Beta.Platform;

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

        private Driver gameSystem;
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

        public Dma(Driver gameSystem, ushort interruptType)
        {
            this.gameSystem = gameSystem;
            this.interruptType = interruptType;
        }

        private void Transfer(int size, ushort count)
        {
            targetStep <<= size;
            sourceStep <<= size;

            do
            {
                gameSystem.Write(size, target, gameSystem.Read(size, source));
                target += targetStep;
                source += sourceStep;
            }
            while (--count != 0);
        }

        #region Registers

        private byte PeekControl_0(uint address)
        {
            return controlRegister.l;
        }

        private byte PeekControl_1(uint address)
        {
            return controlRegister.h;
        }

        private void PokeControl_0(uint address, byte data)
        {
            controlRegister.l = data;
        }

        private void PokeControl_1(uint address, byte data)
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

        private void PokeCounter_0(uint address, byte data)
        {
            lengthRegister.l = data;
        }

        private void PokeCounter_1(uint address, byte data)
        {
            lengthRegister.h = data;
        }

        private void PokeDstAddr_0(uint address, byte data)
        {
            targetRegister.ub0 = data;
        }

        private void PokeDstAddr_1(uint address, byte data)
        {
            targetRegister.ub1 = data;
        }

        private void PokeDstAddr_2(uint address, byte data)
        {
            targetRegister.ub2 = data;
        }

        private void PokeDstAddr_3(uint address, byte data)
        {
            targetRegister.ub3 = data;
        }

        private void PokeSrcAddr_0(uint address, byte data)
        {
            sourceRegister.ub0 = data;
        }

        private void PokeSrcAddr_1(uint address, byte data)
        {
            sourceRegister.ub1 = data;
        }

        private void PokeSrcAddr_2(uint address, byte data)
        {
            sourceRegister.ub2 = data;
        }

        private void PokeSrcAddr_3(uint address, byte data)
        {
            sourceRegister.ub3 = data;
        }

        #endregion

        public void Initialize(uint address)
        {
            gameSystem.mmio.Map(address + 0x0u, PokeSrcAddr_0); // $40000B0 - 32 - DMA0SAD
            gameSystem.mmio.Map(address + 0x1u, PokeSrcAddr_1);
            gameSystem.mmio.Map(address + 0x2u, PokeSrcAddr_2);
            gameSystem.mmio.Map(address + 0x3u, PokeSrcAddr_3);
            gameSystem.mmio.Map(address + 0x4u, PokeDstAddr_0); // $40000B4 - 32 - DMA0DAD
            gameSystem.mmio.Map(address + 0x5u, PokeDstAddr_1);
            gameSystem.mmio.Map(address + 0x6u, PokeDstAddr_2);
            gameSystem.mmio.Map(address + 0x7u, PokeDstAddr_3);
            gameSystem.mmio.Map(address + 0x8u, PokeCounter_0); // $40000B8 - 16 - DMA0CNT_L
            gameSystem.mmio.Map(address + 0x9u, PokeCounter_1);
            gameSystem.mmio.Map(address + 0xau, PeekControl_0, PokeControl_0); // $40000BA - 16 - DMA0CNT_H
            gameSystem.mmio.Map(address + 0xbu, PeekControl_1, PokeControl_1);
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
                gameSystem.Cpu.Interrupt(interruptType);
            }
        }
    }
}
