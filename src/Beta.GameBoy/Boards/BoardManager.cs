using System;

namespace Beta.GameBoy.Boards
{
    public static class BoardManager
    {
        public static Board GetBoard(GameSystem gameSystem, byte[] binary)
        {
            switch (binary[0x0147])
            {
            case 0x00: return new Board(gameSystem, binary); // ROM ONLY
            case 0x01:
            case 0x02:
            case 0x03: return new NintendoMbc1(gameSystem, binary);
            case 0x05:
            case 0x06: return new NintendoMbc2(gameSystem, binary);
            case 0x08: throw new NotSupportedException(); // ROM+RAM
            case 0x09: throw new NotSupportedException(); // ROM+RAM+BATTERY
            case 0x0b: throw new NotSupportedException(); // MMM01
            case 0x0c: throw new NotSupportedException(); // MMM01+RAM
            case 0x0d: throw new NotSupportedException(); // MMM01+RAM+BATTERY
            case 0x0f:
            case 0x10:
            case 0x11:
            case 0x12:
            case 0x13: return new NintendoMbc3(gameSystem, binary);
            case 0x15: throw new NotSupportedException(); // MBC4
            case 0x16: throw new NotSupportedException(); // MBC4+RAM
            case 0x17: throw new NotSupportedException(); // MBC4+RAM+BATTERY
            case 0x19:
            case 0x1a:
            case 0x1b:
            case 0x1c:
            case 0x1d:
            case 0x1e: return new NintendoMbc5(gameSystem, binary);
            case 0xfc: throw new NotSupportedException(); // POCKET CAMERA
            case 0xfd: throw new NotSupportedException(); // BANDAI TAMA5
            case 0xfe: throw new NotSupportedException(); // HuC3
            case 0xff: throw new NotSupportedException(); // HuC1+RAM+BATTERY
            }

            return null;
        }
    }
}
