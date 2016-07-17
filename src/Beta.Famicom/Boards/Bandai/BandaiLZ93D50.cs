using Beta.Famicom.CPU;
using Beta.Famicom.Formats;
using Beta.Platform.Exceptions;
using Beta.Platform.Messaging;

namespace Beta.Famicom.Boards.Bandai
{
    [BoardName("BANDAI-LZ93D50")]
    [BoardName("BANDAI-LZ93D50+24C01")]
    [BoardName("BANDAI-LZ93D50+24C02")]
    public class BandaiLZ93D50 : Board
    {
        private int[] chrPages;
        private int[] prgPages;

        private EepromBase eeprom;
        private bool irqEnabled;
        private int irqCounter;
        private int mirroring;

        public BandaiLZ93D50(CartridgeImage image)
            : base(image)
        {
            chrPages = new int[8];
            prgPages = new int[2];
        }

        private void Peek800D(ushort address, ref byte data)
        {
            if (eeprom != null)
            {
                data = eeprom.Peek();
            }
        }

        private void Poke8000(ushort address, byte data)
        {
            chrPages[0] = data << 10;
        }

        private void Poke8001(ushort address, byte data)
        {
            chrPages[1] = data << 10;
        }

        private void Poke8002(ushort address, byte data)
        {
            chrPages[2] = data << 10;
        }

        private void Poke8003(ushort address, byte data)
        {
            chrPages[3] = data << 10;
        }

        private void Poke8004(ushort address, byte data)
        {
            chrPages[4] = data << 10;
        }

        private void Poke8005(ushort address, byte data)
        {
            chrPages[5] = data << 10;
        }

        private void Poke8006(ushort address, byte data)
        {
            chrPages[6] = data << 10;
        }

        private void Poke8007(ushort address, byte data)
        {
            chrPages[7] = data << 10;
        }

        private void Poke8008(ushort address, byte data)
        {
            prgPages[0] = data << 14;
        }

        private void Poke8009(ushort address, byte data)
        {
            mirroring = (data & 0x03);
        }

        private void Poke800A(ushort address, byte data)
        {
            irqEnabled = (data & 0x01) != 0;
            Cpu.Irq(0);
        }

        private void Poke800B(ushort address, byte data)
        {
            irqCounter = (irqCounter & ~0x00ff) | (data << 0);
        }

        private void Poke800C(ushort address, byte data)
        {
            irqCounter = (irqCounter & ~0xff00) | (data << 8);
        }

        private void Poke800D(ushort address, byte data)
        {
            if (eeprom != null)
            {
                eeprom.Poke(data & 0x20, data & 0x40);
            }
        }

        protected override int DecodeChr(ushort address)
        {
            return (address & 0x3ff) | chrPages[(address >> 10) & 7];
        }

        protected override int DecodePrg(ushort address)
        {
            return (address & 0x3fff) | prgPages[(address >> 14) & 1];
        }

        public override void Consume(ClockSignal e)
        {
            if (irqEnabled)
            {
                irqCounter = ((irqCounter - 1) & 0xffff);

                if (irqCounter == 0x0000)
                {
                    Cpu.Irq(1);
                }
            }
        }

        public override void Initialize()
        {
            base.Initialize();

            prgPages[0] = +0 << 14;
            prgPages[1] = -1 << 14;

            switch (Type)
            {
            case "BANDAI-LZ93D50+24C01": eeprom = new Eeprom128(); break; // Xicor 24C01P
            case "BANDAI-LZ93D50+24C02": eeprom = new Eeprom256(); break; // Xicor 24C02P
            }
        }

        public override void MapToCpu(R2A03Bus bus)
        {
            base.MapToCpu(bus);

            bus.Map("011- ---- ---- 0000", Peek800D, Poke8000);
            bus.Map("011- ---- ---- 0001", Peek800D, Poke8001);
            bus.Map("011- ---- ---- 0010", Peek800D, Poke8002);
            bus.Map("011- ---- ---- 0011", Peek800D, Poke8003);
            bus.Map("011- ---- ---- 0100", Peek800D, Poke8004);
            bus.Map("011- ---- ---- 0101", Peek800D, Poke8005);
            bus.Map("011- ---- ---- 0110", Peek800D, Poke8006);
            bus.Map("011- ---- ---- 0111", Peek800D, Poke8007);
            bus.Map("011- ---- ---- 1000", Peek800D, Poke8008);
            bus.Map("011- ---- ---- 1001", Peek800D, Poke8009);
            bus.Map("011- ---- ---- 1010", Peek800D, Poke800A);
            bus.Map("011- ---- ---- 1011", Peek800D, Poke800B);
            bus.Map("011- ---- ---- 1100", Peek800D, Poke800C);
            bus.Map("011- ---- ---- 1101", Peek800D, Poke800D);
            // .Map("011- ---- ---- 1110"); // open bus?
            // .Map("011- ---- ---- 1111"); // open bus?

            bus.Map("1--- ---- ---- 0000", Peek800D, Poke8000);
            bus.Map("1--- ---- ---- 0001", Peek800D, Poke8001);
            bus.Map("1--- ---- ---- 0010", Peek800D, Poke8002);
            bus.Map("1--- ---- ---- 0011", Peek800D, Poke8003);
            bus.Map("1--- ---- ---- 0100", Peek800D, Poke8004);
            bus.Map("1--- ---- ---- 0101", Peek800D, Poke8005);
            bus.Map("1--- ---- ---- 0110", Peek800D, Poke8006);
            bus.Map("1--- ---- ---- 0111", Peek800D, Poke8007);
            bus.Map("1--- ---- ---- 1000", Peek800D, Poke8008);
            bus.Map("1--- ---- ---- 1001", Peek800D, Poke8009);
            bus.Map("1--- ---- ---- 1010", Peek800D, Poke800A);
            bus.Map("1--- ---- ---- 1011", Peek800D, Poke800B);
            bus.Map("1--- ---- ---- 1100", Peek800D, Poke800C);
            bus.Map("1--- ---- ---- 1101", Peek800D, Poke800D);
            // .Map("1--- ---- ---- 1110"); // open bus?
            // .Map("1--- ---- ---- 1111"); // open bus?
        }

        public override int VRamA10(ushort address)
        {
            var x = (address >> 10) & 1;
            var y = (address >> 11) & 1;

            switch (mirroring)
            {
            case 0: return x;
            case 1: return y;
            case 2: return 0;
            case 3: return 1;
            }

            throw new CompilerPleasingException();
        }

        public class EepromBase
        {
            private int sclLine;
            private int sdaLine;

            protected Mode CurrentMode;
            protected Mode CurrentNext;
            protected byte Output;
            protected byte LatchData;
            protected byte[] Mem = new byte[256];
            protected int LatchAddr;
            protected int LatchBit;

            protected virtual void Fall()
            {
            }

            protected virtual void Idle()
            {
                CurrentMode = Mode.Idle;
                Output = 0x10;
            }

            protected virtual void Open()
            {
            }

            protected virtual void Rise(int bit)
            {
            }

            public byte Peek()
            {
                return Output;
            }

            public void Poke(int scl, int sda)
            {
                if (sdaLine > sda && sclLine != 0) { Open(); goto update; } // SCL: 1, SDA: 1->0
                if (sdaLine < sda && sclLine != 0) { Idle(); goto update; } // SCL: 1, SDA: 0->1
                if (sclLine > scl) { Fall(); goto update; } // SCL: 1->0
                if (sclLine < scl) { Rise(sda >> 6); } // SCL: 0->1

                update:
                sclLine = scl;
                sdaLine = sda;
            }

            protected enum Mode
            {
                Idle,
                Data,
                Address,
                Read,
                Write,
                Ack,
                NotAck,
                AckWait
            }
        }

        private class Eeprom128 : EepromBase
        {
            protected override void Fall()
            {
                switch (CurrentMode)
                {
                case Mode.Address:
                    if (LatchBit == 8)
                    {
                        CurrentMode = Mode.Ack;
                        Output = 0x10;
                    }
                    break;

                case Mode.Ack:
                    CurrentMode = CurrentNext;
                    LatchBit = 0;
                    Output = 0x10;
                    break;

                case Mode.Read:
                    if (LatchBit == 8)
                    {
                        CurrentMode = Mode.AckWait;
                        LatchAddr = (LatchAddr + 1) & 0x7F;
                    }
                    break;

                case Mode.Write:
                    if (LatchBit == 8)
                    {
                        CurrentMode = Mode.Ack;
                        CurrentNext = Mode.Idle;
                        Mem[LatchAddr] = LatchData;
                        LatchAddr = (LatchAddr + 1) & 0x7F;
                    }
                    break;
                }
            }

            protected override void Open()
            {
                CurrentMode = Mode.Address;
                Output = 0x10;
                LatchBit = 0;
                LatchAddr = 0;
            }

            protected override void Rise(int bit)
            {
                switch (CurrentMode)
                {
                case Mode.Address:
                    if (LatchBit < 7)
                    {
                        LatchAddr &= ~(1 << LatchBit);
                        LatchAddr |= (bit << LatchBit++);
                    }
                    else if (LatchBit < 8)
                    {
                        LatchBit = 8;

                        if (bit != 0)
                        {
                            CurrentNext = Mode.Read;
                            LatchData = Mem[LatchAddr];
                        }
                        else
                        {
                            CurrentNext = Mode.Write;
                        }
                    }
                    break;

                case Mode.Ack:
                    Output = 0x00;
                    break;

                case Mode.Read:
                    if (LatchBit < 8)
                    {
                        Output = (byte)((LatchData & (1 << LatchBit++)) != 0 ? 0x10 : 0x00);
                    }
                    break;

                case Mode.Write:
                    if (LatchBit < 8)
                    {
                        LatchData &= (byte)~(1 << LatchBit);
                        LatchData |= (byte)(bit << LatchBit++);
                    }
                    break;

                case Mode.AckWait:
                    if (bit == 0)
                    {
                        CurrentNext = Mode.Idle;
                    }
                    break;
                }
            }
        }

        private class Eeprom256 : EepromBase
        {
            private int rw;

            protected override void Fall()
            {
                switch (CurrentMode)
                {
                case Mode.Data:
                    if (LatchBit == 8)
                    {
                        if ((LatchData & 0xA0) == 0xA0)
                        {
                            LatchBit = 0;
                            CurrentMode = Mode.Ack;
                            rw = LatchData & 0x01;
                            Output = 0x10;

                            if (rw != 0)
                            {
                                CurrentNext = Mode.Read;
                                LatchData = Mem[LatchAddr];
                            }
                            else
                            {
                                CurrentNext = Mode.Address;
                            }
                        }
                        else
                        {
                            CurrentMode = Mode.NotAck;
                            CurrentNext = Mode.Idle;
                            Output = 0x10;
                        }
                    }
                    break;

                case Mode.Address:
                    if (LatchBit == 8)
                    {
                        LatchBit = 0;
                        CurrentMode = Mode.Ack;
                        CurrentNext = (rw != 0 ? Mode.Idle : Mode.Write);
                        Output = 0x10;
                    }
                    break;

                case Mode.Read:
                    if (LatchBit == 8)
                    {
                        CurrentMode = Mode.AckWait;
                        LatchAddr = (LatchAddr + 1) & 0xff;
                    }
                    break;

                case Mode.Write:
                    if (LatchBit == 8)
                    {
                        LatchBit = 0;
                        CurrentMode = Mode.Ack;
                        CurrentNext = Mode.Write;
                        Mem[LatchAddr] = LatchData;
                        LatchAddr = (LatchAddr + 1) & 0xff;
                    }
                    break;

                case Mode.NotAck:
                    CurrentMode = Mode.Idle;
                    LatchBit = 0;
                    Output = 0x10;
                    break;

                case Mode.Ack:
                case Mode.AckWait:
                    CurrentMode = CurrentNext;
                    LatchBit = 0;
                    Output = 0x10;
                    break;
                }
            }

            protected override void Open()
            {
                CurrentMode = Mode.Data;
                Output = 0x10;
                LatchBit = 0;
            }

            protected override void Rise(int bit)
            {
                switch (CurrentMode)
                {
                case Mode.Data:
                case Mode.Write:
                    if (LatchBit < 8)
                    {
                        LatchData &= (byte)~(1 << (7 - LatchBit));
                        LatchData |= (byte)(bit << (7 - LatchBit++));
                    }
                    break;

                case Mode.Address:
                    if (LatchBit < 8)
                    {
                        LatchAddr &= (byte)~(1 << (7 - LatchBit));
                        LatchAddr |= (byte)(bit << (7 - LatchBit++));
                    }
                    break;

                case Mode.Read:
                    if (LatchBit < 8)
                        Output = (byte)((LatchData & (1 << (7 - LatchBit++))) != 0 ? 0x10 : 0x00);
                    break;

                case Mode.NotAck: Output = 0x10; break;
                case Mode.Ack: Output = 0x00; break;
                case Mode.AckWait:
                    if (bit == 0)
                    {
                        CurrentNext = Mode.Read;
                        LatchData = Mem[LatchAddr];
                    }
                    break;
                }
            }
        }
    }
}
