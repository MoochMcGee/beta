using System;

namespace Beta.GameBoy.Boards
{
    public class Board
    {
        protected IAddressSpace AddressSpace;
        protected byte[] Ram;
        protected byte[] Rom;
        protected int RamMask;
        protected int RomMask = 0x7fff;

        public Board(IAddressSpace addressSpace, byte[] rom)
        {
            AddressSpace = addressSpace;
            Rom = (byte[])rom.Clone();

            SetRomSize(Rom[0x148]);
            SetRamSize(Rom[0x149]);
        }

        private byte ReadRam(ushort address)
        {
            return Ram[address & RamMask];
        }

        private byte ReadRom(ushort address)
        {
            return Rom[address & RomMask];
        }

        private void WriteRam(ushort address, byte data)
        {
            Ram[address & RamMask] = data;
        }

        private void WriteRom(ushort address, byte data)
        {
        }

        protected virtual void SetRamSize(byte value)
        {
            switch (value)
            {
            case 0x00:
                Ram = null;
                break;

            case 0x01:
                Ram = new byte[0x0800];
                RamMask = 0x07ff;
                break;

            case 0x02:
                Ram = new byte[0x2000];
                RamMask = 0x1fff;
                break;

            case 0x03:
                Ram = new byte[0x8000];
                RamMask = 0x1fff;
                break;
            }
        }

        protected virtual void SetRomSize(byte value)
        {
            switch (value)
            {
            case 0x00:
                RomMask = 0x7fff;
                break; // 00h -  32KByte (no ROM banking)
            case 0x01:
                RomMask = 0xffff;
                break; // 01h -  64KByte (4 banks)
            case 0x02:
                RomMask = 0x1ffff;
                break; // 02h - 128KByte (8 banks)
            case 0x03:
                RomMask = 0x3ffff;
                break; // 03h - 256KByte (16 banks)
            case 0x04:
                RomMask = 0x7ffff;
                break; // 04h - 512KByte (32 banks)
            case 0x05:
                RomMask = 0xfffff;
                break; // 05h -   1MByte (64 banks)  - only 63 banks used by MBC1
            case 0x06:
                RomMask = 0x1fffff;
                break; // 06h -   2MByte (128 banks) - only 125 banks used by MBC1
            case 0x07:
                RomMask = 0x3fffff;
                break; // 07h -   4MByte (256 banks)
            case 0x52:
                throw new NotSupportedException("Multi-chip ROMs aren't supported yet."); // 52h - 1.1MByte (72 banks)
            case 0x53:
                throw new NotSupportedException("Multi-chip ROMs aren't supported yet."); // 53h - 1.2MByte (80 banks)
            case 0x54:
                throw new NotSupportedException("Multi-chip ROMs aren't supported yet."); // 54h - 1.5MByte (96 banks)
            }
        }

        public virtual void Initialize()
        {
            if (Rom != null)
            {
                HookRom();
            }

            if (Ram != null)
            {
                HookRam();
            }

            AddressSpace.Map(0xff50, writer: DisableBios);
        }

        protected virtual void DisableBios(ushort address, byte data)
        {
            AddressSpace.Map(0x0000, 0x00ff, ReadRom, WriteRom);
        }

        protected virtual void HookRam()
        {
            AddressSpace.Map(0xa000, 0xbfff, ReadRam, WriteRam);
        }

        protected virtual void HookRom()
        {
            AddressSpace.Map(0x0000, 0x7fff, ReadRom, WriteRom);
        }
    }
}
