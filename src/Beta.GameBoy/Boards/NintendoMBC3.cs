using System;

namespace Beta.GameBoy.Boards
{
    public class NintendoMbc3 : Board
    {
        private DateTime rtc;
        private bool ramEnable;
        private uint ramPage;
        private uint romPage = (1U << 14);
        private uint rtcLatch;

        public NintendoMbc3(GameSystem gameboy, byte[] rom)
            : base(gameboy, rom)
        {
        }

        private byte Peek_0000_3FFF(uint address)
        {
            return Rom[((address & 0x3FFF)) & RomMask];
        }

        private byte Peek_4000_7FFF(uint address)
        {
            return Rom[((address & 0x3FFF) | romPage) & RomMask];
        }

        private byte Peek_A000_BFFF(uint address)
        {
            if (!ramEnable)
                return 0;

            switch (ramPage)
            {
            case 0x0: return Ram[(address & 0x1FFF) | 0x0000];
            case 0x1: return Ram[(address & 0x1FFF) | 0x2000];
            case 0x2: return Ram[(address & 0x1FFF) | 0x4000];
            case 0x3: return Ram[(address & 0x1FFF) | 0x6000];

            // RTC Registers
            case 0x8: return (byte)rtc.Second;
            case 0x9: return (byte)rtc.Minute;
            case 0xA: return (byte)rtc.Hour;
            case 0xB: break;
            case 0xC: break;
            }

            return 0;
        }

        private void Poke_0000_1FFF(uint address, byte data)
        {
            ramEnable = (data == 0x0A);
        }

        private void Poke_2000_3FFF(uint address, byte data)
        {
            romPage = (data & 0x7FU) << 14;

            if (romPage == 0)
                romPage += (1U << 14);
        }

        private void Poke_4000_5FFF(uint address, byte data)
        {
            ramPage = (data & 0xFFU);
        }

        private void Poke_6000_7FFF(uint address, byte data)
        {
            if (rtcLatch < (data & 0x01))
            {
                rtcLatch = 0x01;
                rtc = DateTime.Now;
            }
        }

        private void Poke_A000_BFFF(uint address, byte data)
        {
            if (!ramEnable)
                return;

            switch (ramPage)
            {
            case 0x0: Ram[(address & 0x1FFF) | 0x0000] = data; break;
            case 0x1: Ram[(address & 0x1FFF) | 0x2000] = data; break;
            case 0x2: Ram[(address & 0x1FFF) | 0x4000] = data; break;
            case 0x3: Ram[(address & 0x1FFF) | 0x6000] = data; break;

            // RTC Registers
            case 0x8: break;
            case 0x9: break;
            case 0xA: break;
            case 0xB: break;
            case 0xC: break;
            }
        }

        protected override void DisableBios(uint address, byte data)
        {
            GameSystem.Hook(0x0000, 0x00FF, Peek_0000_3FFF, Poke_0000_1FFF);
            GameSystem.Hook(0x0200, 0x08FF, Peek_0000_3FFF, Poke_0000_1FFF);
        }

        protected override void HookRam()
        {
            GameSystem.Hook(0xA000, 0xBFFF, Peek_A000_BFFF, Poke_A000_BFFF);
        }

        protected override void HookRom()
        {
            GameSystem.Hook(0x0000, 0x1FFF, Peek_0000_3FFF, Poke_0000_1FFF);
            GameSystem.Hook(0x2000, 0x3FFF, Peek_0000_3FFF, Poke_2000_3FFF);
            GameSystem.Hook(0x4000, 0x5FFF, Peek_4000_7FFF, Poke_4000_5FFF);
            GameSystem.Hook(0x6000, 0x7FFF, Peek_4000_7FFF, Poke_6000_7FFF);
        }
    }
}
