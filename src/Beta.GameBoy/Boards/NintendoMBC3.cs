using System;

namespace Beta.GameBoy.Boards
{
    public class NintendoMbc3 : Board
    {
        private DateTime rtc;
        private bool ramEnable;
        private int ramPage;
        private int romPage = 1 << 14;
        private int rtcLatch;

        public NintendoMbc3(byte[] rom)
            : base(rom)
        {
        }

        private byte Read_0000_3FFF(ushort address)
        {
            return Rom[((address & 0x3FFF)) & RomMask];
        }

        private byte Read_4000_7FFF(ushort address)
        {
            return Rom[((address & 0x3FFF) | romPage) & RomMask];
        }

        private byte Read_A000_BFFF(ushort address)
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

        private void Write_0000_1FFF(ushort address, byte data)
        {
            ramEnable = (data == 0x0A);
        }

        private void Write_2000_3FFF(ushort address, byte data)
        {
            romPage = (data & 0x7F) << 14;

            if (romPage == 0)
            {
                romPage += 1 << 14;
            }
        }

        private void Write_4000_5FFF(ushort address, byte data)
        {
            ramPage = data;
        }

        private void Write_6000_7FFF(ushort address, byte data)
        {
            if (rtcLatch < (data & 0x01))
            {
                rtcLatch = 0x01;
                rtc = DateTime.Now;
            }
        }

        private void Write_A000_BFFF(ushort address, byte data)
        {
            if (!ramEnable)
            {
                return;
            }

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

        public override byte Read(ushort address)
        {
            if (address >= 0x0000 && address <= 0x3FFF) return Read_0000_3FFF(address);
            if (address >= 0x4000 && address <= 0x7FFF) return Read_4000_7FFF(address);
            if (address >= 0xA000 && address <= 0xBFFF) return Read_A000_BFFF(address);
            return 0xff;
        }

        public override void Write(ushort address, byte data)
        {
            if (address >= 0x0000 && address <= 0x1FFF) Write_0000_1FFF(address, data);
            if (address >= 0x2000 && address <= 0x3FFF) Write_2000_3FFF(address, data);
            if (address >= 0x4000 && address <= 0x5FFF) Write_4000_5FFF(address, data);
            if (address >= 0x6000 && address <= 0x7FFF) Write_6000_7FFF(address, data);
            if (address >= 0xA000 && address <= 0xBFFF) Write_A000_BFFF(address, data);
        }
    }
}
