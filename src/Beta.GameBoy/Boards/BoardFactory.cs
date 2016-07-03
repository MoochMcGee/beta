using System;

namespace Beta.GameBoy.Boards
{
    public class BoardFactory : IBoardFactory
    {
        public Board Create(byte[] binary)
        {
            var type = binary[0x0147];

            switch (type)
            {
            case 0x00: return new Board(binary); // ROM ONLY
            case 0x01:
            case 0x02:
            case 0x03: return new NintendoMbc1(binary);
            case 0x05:
            case 0x06: return new NintendoMbc2(binary);
            case 0x08: break; // ROM+RAM
            case 0x09: break; // ROM+RAM+BATTERY
            case 0x0b: break; // MMM01
            case 0x0c: break; // MMM01+RAM
            case 0x0d: break; // MMM01+RAM+BATTERY
            case 0x0f:
            case 0x10:
            case 0x11:
            case 0x12:
            case 0x13: return new NintendoMbc3(binary);
            case 0x15: break; // MBC4
            case 0x16: break; // MBC4+RAM
            case 0x17: break; // MBC4+RAM+BATTERY
            case 0x19:
            case 0x1a:
            case 0x1b:
            case 0x1c:
            case 0x1d:
            case 0x1e: return new NintendoMbc5(binary);
            case 0xfc: break; // POCKET CAMERA
            case 0xfd: break; // BANDAI TAMA5
            case 0xfe: break; // HuC3
            case 0xff: break; // HuC1+RAM+BATTERY
            }

            throw new NotSupportedException($"Unrecognized board type '${type:x2}'.");
        }
    }
}
