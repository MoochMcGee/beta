using System;
using System.Runtime.InteropServices;
using Beta.Platform.Audio;
using Beta.Platform.Core;
using Beta.Platform.Messaging;

namespace Beta.SuperFamicom.SMP
{
    public sealed class Smp : Processor, IConsumer<ClockSignal>
    {
        static readonly int[] instrTimes =
        {// 0  1  2  3  4  5  6  7  8  9  A  B  C  D  E  F
	        2, 8, 4, 5, 3, 4, 3, 6, 2, 6, 5, 4, 5, 4, 6, 8, // 0
	        2, 8, 4, 5, 4, 5, 5, 6, 5, 5, 6, 5, 2, 2, 4, 6, // 1
	        2, 8, 4, 5, 3, 4, 3, 6, 2, 6, 5, 4, 5, 4, 5, 2, // 2
	        2, 8, 4, 5, 4, 5, 5, 6, 5, 5, 6, 5, 2, 2, 3, 8, // 3
	        2, 8, 4, 5, 3, 4, 3, 6, 2, 6, 4, 4, 5, 4, 6, 6, // 4
	        2, 8, 4, 5, 4, 5, 5, 6, 5, 5, 4, 5, 2, 2, 4, 3, // 5
	        2, 8, 4, 5, 3, 4, 3, 6, 2, 6, 4, 4, 5, 4, 5, 5, // 6
	        2, 8, 4, 5, 4, 5, 5, 6, 5, 5, 5, 5, 2, 2, 3, 6, // 7
	        2, 8, 4, 5, 3, 4, 3, 6, 2, 6, 5, 4, 5, 2, 4, 5, // 8
	        2, 8, 4, 5, 4, 5, 5, 6, 5, 5, 5, 5, 2, 2,12, 5, // 9
	        3, 8, 4, 5, 3, 4, 3, 6, 2, 6, 4, 4, 5, 2, 4, 4, // A
	        2, 8, 4, 5, 4, 5, 5, 6, 5, 5, 5, 5, 2, 2, 3, 4, // B
	        3, 8, 4, 5, 4, 5, 4, 7, 2, 5, 6, 4, 5, 2, 4, 9, // C
	        2, 8, 4, 5, 5, 6, 6, 7, 4, 5, 5, 5, 2, 2, 6, 3, // D
	        2, 8, 4, 5, 3, 4, 3, 6, 2, 4, 5, 3, 4, 3, 4, 0, // E
	        2, 8, 4, 5, 4, 5, 5, 6, 3, 4, 5, 4, 2, 2, 4, 0  // F
	    };

        private static byte[] bootRom = new byte[]
        {
            0xcd, 0xef, 0xbd, 0xe8, 0x00, 0xc6, 0x1d, 0xd0, 0xfc, 0x8f, 0xaa, 0xf4, 0x8f, 0xbb, 0xf5, 0x78,
            0xcc, 0xf4, 0xd0, 0xfb, 0x2f, 0x19, 0xeb, 0xf4, 0xd0, 0xfc, 0x7e, 0xf4, 0xd0, 0x0b, 0xe4, 0xf5,
            0xcb, 0xf4, 0xd7, 0x00, 0xfc, 0xd0, 0xf3, 0xab, 0x01, 0x10, 0xef, 0x7e, 0xf4, 0x10, 0xeb, 0xba,
            0xf6, 0xda, 0x00, 0xba, 0xf4, 0xc4, 0xf4, 0xdd, 0x5d, 0xd0, 0xdb, 0x1f, 0x00, 0x00, 0xc0, 0xff
        };

        private Registers registers;
        private Action[] codeTable;
        private Dsp dsp;
        private Timer[] timers = new Timer[3];
        private byte[] wram;
        private byte[] port;
        private bool bootRomEnabled;
        private bool flagC;
        private bool flagZ;
        private bool flagH;
        private int flagP;
        private bool flagV;
        private bool flagN;
        private int timerCycles1;
        private int timerCycles2;

        public Smp(Driver gameSystem, IAudioBackend audio)
        {
            Single = 1;

            codeTable = new Action[]
            {
                Op00, Op01, Op02, Op03, Op04, Op05, Op06, Op07, Op08, Op09, Op0A, Op0B, Op0C, Op0D, Op0E, Op0F,
                Op10, Op11, Op12, Op13, Op14, Op15, Op16, Op17, Op18, Op19, Op1A, Op1B, Op1C, Op1D, Op1E, Op1F,
                Op20, Op21, Op22, Op23, Op24, Op25, Op26, Op27, Op28, Op29, Op2A, Op2B, Op2C, Op2D, Op2E, Op2F,
                Op30, Op31, Op32, Op33, Op34, Op35, Op36, Op37, Op38, Op39, Op3A, Op3B, Op3C, Op3D, Op3E, Op3F,
                Op40, Op41, Op42, Op43, Op44, Op45, Op46, Op47, Op48, Op49, Op4A, Op4B, Op4C, Op4D, Op4E, Op4F,
                Op50, Op51, Op52, Op53, Op54, Op55, Op56, Op57, Op58, Op59, Op5A, Op5B, Op5C, Op5D, Op5E, Op5F,
                Op60, Op61, Op62, Op63, Op64, Op65, Op66, Op67, Op68, Op69, Op6A, Op6B, Op6C, Op6D, Op6E, Op6F,
                Op70, Op71, Op72, Op73, Op74, Op75, Op76, Op77, Op78, Op79, Op7A, Op7B, Op7C, Op7D, Op7E, Op7F,
                Op80, Op81, Op82, Op83, Op84, Op85, Op86, Op87, Op88, Op89, Op8A, Op8B, Op8C, Op8D, Op8E, Op8F,
                Op90, Op91, Op92, Op93, Op94, Op95, Op96, Op97, Op98, Op99, Op9A, Op9B, Op9C, Op9D, Op9E, Op9F,
                OpA0, OpA1, OpA2, OpA3, OpA4, OpA5, OpA6, OpA7, OpA8, OpA9, OpAA, OpAB, OpAC, OpAD, OpAE, OpAF,
                OpB0, OpB1, OpB2, OpB3, OpB4, OpB5, OpB6, OpB7, OpB8, OpB9, OpBA, OpBB, OpBC, OpBD, OpBE, OpBF,
                OpC0, OpC1, OpC2, OpC3, OpC4, OpC5, OpC6, OpC7, OpC8, OpC9, OpCA, OpCB, OpCC, OpCD, OpCE, OpCF,
                OpD0, OpD1, OpD2, OpD3, OpD4, OpD5, OpD6, OpD7, OpD8, OpD9, OpDA, OpDB, OpDC, OpDD, OpDE, OpDF,
                OpE0, OpE1, OpE2, OpE3, OpE4, OpE5, OpE6, OpE7, OpE8, OpE9, OpEA, OpEB, OpEC, OpED, OpEE, OpEF,
                OpF0, OpF1, OpF2, OpF3, OpF4, OpF5, OpF6, OpF7, OpF8, OpF9, OpFA, OpFB, OpFC, OpFD, OpFE, OpFF
            };
            wram = new byte[65536];
            port = new byte[4];
            dsp = new Dsp(gameSystem, audio, wram);

            registers.sph = 1;
        }

        private static byte ReadBootRom(int address)
        {
            return bootRom[address & 0x3f];
        }

        public void Initialize()
        {
            wram[0xf0] = 0x0a;
            wram[0xf1] = 0xb0;
            bootRomEnabled = true;
            registers.a = 0;
            registers.x = 0;
            registers.y = 0;
            registers.sp = 0x100;
            registers.pc = ReadFullWord(0xfffe);
            flagP = 0;
            flagC = false;
            flagZ = false;
            flagH = false;
            flagV = false;
            flagN = false;
            timerCycles1 = 0;
            timerCycles2 = 0;
            timers[0].Enabled = false;
            timers[1].Enabled = false;
            timers[2].Enabled = false;
            timers[0].Compare = 255;
            timers[1].Compare = 255;
            timers[2].Compare = 255;
            timers[0].Stage1 = 0;
            timers[1].Stage1 = 0;
            timers[2].Stage1 = 0;
            timers[0].Stage2 = 0;
            timers[1].Stage2 = 0;
            timers[2].Stage2 = 0;
            dsp.Initialize();
        }

        private byte Shl(byte value, int carry = 0)
        {
            flagC = (value & 0x80) != 0;
            value = (byte)((value << 1) | carry);
            SetZnByte(value);
            return value;
        }

        private byte Shr(byte value, int carry = 0)
        {
            flagC = (value & 0x01) != 0;
            value = (byte)((value >> 1) | carry);
            SetZnByte(value);
            return value;
        }

        private byte ImmediateByte()
        {
            return ReadCode(registers.pc++);
        }

        private ushort AbsoluteAddress()
        {
            var l = ImmediateByte();
            var h = ImmediateByte();

            return (ushort)((h << 8) | l);
        }

        private ushort AbsoluteAddressX()
        {
            return (ushort)(AbsoluteAddress() + registers.x);
        }

        private ushort AbsoluteAddressY()
        {
            return (ushort)(AbsoluteAddress() + registers.y);
        }

        private ushort DirectPageAddress()
        {
            return (ushort)(flagP | ImmediateByte());
        }

        private ushort DirectPageAddressIndirect()
        {
            return ReadDataWord(DirectPageAddress());
        }

        private ushort DirectPageAddressX()
        {
            return (ushort)(((ImmediateByte() + registers.x) & 0xff) | flagP);
        }

        private ushort DirectPageAddressY()
        {
            return (ushort)(((ImmediateByte() + registers.y) & 0xff) | flagP);
        }

        private ushort DirectPageAddressXIndirect()
        {
            return ReadDataWord(DirectPageAddressX());
        }

        private ushort DirectPageAddressYIndirect()
        {
            return (ushort)(DirectPageAddressIndirect() + registers.y);
        }

        private ushort DirectPageXAddress()
        {
            return (ushort)(flagP | registers.x);
        }

        private ushort DirectPageXAddressIncrement()
        {
            return (ushort)(flagP | (registers.x++));
        }

        private ushort DirectPageYAddress()
        {
            return (ushort)(flagP | registers.y);
        }

        private void UpdateTimers()
        {
            timers[0].Update(timerCycles1 >> 7);
            timers[1].Update(timerCycles1 >> 7);
            timers[2].Update(timerCycles2 >> 4);

            timerCycles1 &= 127;
            timerCycles2 &= 15;
        }

        private void TakeBranch()
        {
            var offset = (ushort)(sbyte)ImmediateByte();
            registers.pc += offset;
            AddCycles(2);
        }

        private byte ReadFull(int address)
        {
            if (address >= 0xffc0 && bootRomEnabled)
            {
                return ReadBootRom(address);
            }

            return ReadData(address);
        }

        private byte ReadData(int address)
        {
            if ((address & 0xfff0) != 0x00f0)
            {
                return ReadWram(address);
            }

            byte result;

            switch (address)
            {
            case 0x00f3: return dsp.Peek();
            case 0x00fd: UpdateTimers(); result = (byte)timers[0].Stage2; timers[0].Stage2 = 0; break;
            case 0x00fe: UpdateTimers(); result = (byte)timers[1].Stage2; timers[1].Stage2 = 0; break;
            case 0x00ff: UpdateTimers(); result = (byte)timers[2].Stage2; timers[2].Stage2 = 0; break;
            default: return ReadWram(address);
            }

            return result;
        }

        private byte ReadCode(int address)
        {
            if (address >= 0xffc0 && bootRomEnabled)
            {
                return ReadBootRom(address);
            }

            return ReadWram(address);
        }

        private byte ReadWram(int address)
        {
            return wram[address];
        }

        private void WriteWram(int address, int data)
        {
            wram[address] = (byte)data;
        }

        private ushort ReadFullWord(int address)
        {
            var l = ReadFull(address);
            var h = ReadFull((address + 1) & 0xffff);

            return (ushort)(l | (h << 8));
        }

        private ushort ReadDataWord(int address)
        {
            var l = ReadData(address);
            var h = ReadData((address + 1) & 0xffff);

            return (ushort)(l | (h << 8));
        }

        private void AddCycles(int amount)
        {
            Cycles += amount * (39375 * 24);
            timerCycles1 += amount;
            timerCycles2 += amount;
            dsp.Update(amount);
        }

        private void SetZnByte(int value)
        {
            flagZ = (value & 0xff) == 0;
            flagN = (value & 0x80) != 0;
        }

        private void SetZnWord(ushort value)
        {
            flagZ = (value & 0xffff) == 0;
            flagN = (value & 0x8000) != 0;
        }

        private byte PullByte()
        {
            registers.spl++;
            return ReadWram(registers.sp);
        }

        private ushort PullWord()
        {
            var l = PullByte();
            var h = PullByte();

            return (ushort)((h << 8) | l);
        }

        private void PushByte(byte value)
        {
            WriteWram(registers.sp, value);
            registers.spl--;
        }

        private void PushWord(ushort value)
        {
            PushByte((byte)(value >> 8));
            PushByte((byte)(value >> 0));
        }

        private void LoadFlags(byte value)
        {
            flagC = (value & 0x01) != 0;
            flagZ = (value & 0x02) != 0;
            flagH = (value & 0x08) != 0;
            flagP = (value & 0x20) != 0 ? 256 : 0;
            flagV = (value & 0x40) != 0;
            flagN = (value & 0x80) != 0;
        }

        private void WriteByte(ushort address, int data)
        {
            if ((address & 0xfff0) == 0x00f0)
            {
                switch (address)
                {
                case 0xf1:
                    UpdateTimers();

                    if (!timers[0].Enabled && (data & 0x01) != 0)
                    {
                        timers[0].Stage1 = 0;
                        timers[0].Stage2 = 0;
                    }

                    timers[0].Enabled = (data & 0x01) != 0;

                    if (!timers[1].Enabled && (data & 0x02) != 0)
                    {
                        timers[1].Stage1 = 0;
                        timers[1].Stage2 = 0;
                    }

                    timers[1].Enabled = (data & 0x02) != 0;

                    if (!timers[2].Enabled && (data & 0x04) != 0)
                    {
                        timers[2].Stage1 = 0;
                        timers[2].Stage2 = 0;
                    }

                    timers[2].Enabled = (data & 0x04) != 0;

                    if ((data & 0x10) != 0)
                    {
                        wram[0xf4] = 0;
                        wram[0xf5] = 0;
                    }

                    if ((data & 0x20) != 0)
                    {
                        wram[0xf6] = 0;
                        wram[0xf7] = 0;
                    }

                    bootRomEnabled = (data & 0x80) != 0;
                    break;

                case 0xf3: dsp.Poke((byte)data); break;
                case 0xf4: port[0] = (byte)data; return;
                case 0xf5: port[1] = (byte)data; return;
                case 0xf6: port[2] = (byte)data; return;
                case 0xf7: port[3] = (byte)data; return;
                case 0xfa: UpdateTimers(); timers[1 - 1].Compare = data; break;
                case 0xfb: UpdateTimers(); timers[2 - 1].Compare = data; break;
                case 0xfc: UpdateTimers(); timers[3 - 1].Compare = data; break;
                case 0xfd: return;
                case 0xfe: return;
                case 0xff: return;
                }
            }

            WriteWram(address, data);
        }

        private void WriteWord(ushort address, ushort data)
        {
            WriteByte(address, (byte)(data >> 0));
            address++;
            WriteByte(address, (byte)(data >> 8));
        }

        private void PushFlags()
        {
            byte flags = 0;

            if (flagC) { flags |= 0x01; }
            if (flagZ) { flags |= 0x02; }
            if (flagH) { flags |= 0x08; }
            if (flagP != 0) { flags |= 0x20; }
            if (flagV) { flags |= 0x40; }
            if (flagN) { flags |= 0x80; }

            PushByte(flags);
        }

        private void Branch(bool flag)
        {
            if (flag)
            {
                TakeBranch();
            }
            else
            {
                registers.pc++;
            }
        }

        private byte And(byte value)
        {
            return registers.a &= value;
        }

        private byte Eor(byte value)
        {
            return registers.a ^= value;
        }

        private byte Ora(byte value)
        {
            return registers.a |= value;
        }

        private byte Asl(byte value)
        {
            return Shl(value);
        }

        private byte Lsr(byte value)
        {
            return Shr(value);
        }

        private byte Rol(byte value)
        {
            return Shl(value, flagC ? 0x01 : 0);
        }

        private byte Ror(byte value)
        {
            return Shr(value, flagC ? 0x80 : 0);
        }

        private byte Adc(byte left, byte right)
        {
            var temporary = left + right + (flagC ? 1 : 0);
            flagC = (temporary > 0xff);
            temporary &= 0xff;
            flagV = (~(left ^ right) & (right ^ temporary) & 0x80) != 0;
            flagH = ((left ^ right ^ temporary) & 0x10) != 0;
            SetZnByte(temporary);
            return (byte)temporary;
        }

        private ushort Add(ushort left, ushort right)
        {
            flagC = false;

            int temporary;
            temporary = Adc((byte)(left >> 0), (byte)(right >> 0)) << 0;
            temporary |= Adc((byte)(left >> 8), (byte)(right >> 8)) << 8;

            flagZ = temporary == 0;

            return (ushort)temporary;
        }

        private byte Sbc(byte left, byte right)
        {
            right ^= 0xff;

            return Adc(left, right);
        }

        private ushort Sub(ushort left, ushort right)
        {
            right = (ushort)-right;

            return Add(left, right);
        }

        private void CmpByte(byte left, byte right)
        {
            flagC = left >= right;
            left -= right;
            SetZnByte(left);
        }

        private void CmpWord(ushort left, ushort right)
        {
            flagC = left >= right;
            left -= right;
            SetZnWord(left);
        }

        public override void Update(int cycles)
        {
            while (Cycles < cycles)
            {
                var opcode = ImmediateByte();

                codeTable[opcode]();

                AddCycles(instrTimes[opcode]);
            }

            Cycles -= cycles;
        }

        public byte ReadPort(int number, int timestamp)
        {
            Update(timestamp);
            return port[number];
        }

        public void WritePort(int number, int data, int timestamp)
        {
            Update(timestamp);
            wram[number + 0xf4] = (byte)data;
        }

        #region Codes

        private void Op00()
        {
        }

        private void Op01()
        {
            var var1 = ReadFullWord(0xffde);
            PushWord(registers.pc);
            registers.pc = (var1);
        }

        private void Op02()
        {
            var var1 = DirectPageAddress();
            var var2 = ReadFull(var1) | 0x01;
            WriteByte(var1, var2);
        }

        private void Op03()
        {
            Branch((ReadData(DirectPageAddress()) & 1) != 0);
        }

        private void Op04()
        {
            SetZnByte(Ora(ReadData(DirectPageAddress())));
        }

        private void Op05()
        {
            SetZnByte(Ora(ReadFull(AbsoluteAddress())));
        }

        private void Op06()
        {
            SetZnByte(Ora(ReadData(DirectPageXAddress())));
        }

        private void Op07()
        {
            SetZnByte(Ora(ReadFull(DirectPageAddressXIndirect())));
        }

        private void Op08()
        {
            SetZnByte(Ora(ImmediateByte()));
        }

        private void Op09()
        {
            var var1 = ReadData(DirectPageAddress());
            var var2 = DirectPageAddress();
            var1 |= ReadFull(var2);
            WriteByte(var2, var1);
            SetZnByte(var1);
        }

        private void Op0A()
        {
            var var1 = AbsoluteAddress();
            var var2 = 1 << (var1 >> 13);
            var1 &= 8191;
            if ((ReadFull(var1) & var2) != 0)
            {
                flagC = (true);
            }
        }

        private void Op0B()
        {
            var address = DirectPageAddress();
            WriteByte(address, Asl(ReadFull(address)));
        }

        private void Op0C()
        {
            var address = AbsoluteAddress();
            WriteByte(address, Asl(ReadFull(address)));
        }

        private void Op0D()
        {
            PushFlags();
        }

        private void Op0E()
        {
            var address = AbsoluteAddress();
            var data = ReadFull(address);
            SetZnByte(registers.a - data & 255);
            data |= registers.a;
            WriteByte(address, data);
        }

        private void Op0F()
        {
            var var1 = ReadFullWord(0xfffe);
            PushWord(registers.pc);
            PushFlags();
            registers.pc = var1;
        }

        private void Op10()
        {
            Branch(!flagN);
        }

        private void Op11()
        {
            var var1 = ReadFullWord(0xffdc);
            PushWord(registers.pc);
            registers.pc = var1;
        }

        private void Op12()
        {
            var address = DirectPageAddress();
            var var2 = ReadFull(address) & ~0x01;
            WriteByte(address, var2);
        }

        private void Op13()
        {
            Branch((ReadData(DirectPageAddress()) & 1) == 0);
        }

        private void Op14()
        {
            SetZnByte(Ora(ReadData(DirectPageAddressX())));
        }

        private void Op15()
        {
            SetZnByte(Ora(ReadFull(AbsoluteAddressX())));
        }

        private void Op16()
        {
            SetZnByte(Ora(ReadFull(AbsoluteAddressY())));
        }

        private void Op17()
        {
            SetZnByte(Ora(ReadFull(DirectPageAddressYIndirect())));
        }

        private void Op18()
        {
            var data = ImmediateByte();
            var address = DirectPageAddress();
            data |= ReadFull(address);
            WriteByte(address, data);
            SetZnByte(data);
        }

        private void Op19()
        {
            var data = ReadData(flagP | registers.y);
            var address = DirectPageXAddress();
            data |= ReadFull(address);
            WriteByte(address, data);
            SetZnByte(data);
        }

        private void Op1A()
        {
            var address = DirectPageAddress();
            var data = ReadFullWord(address);
            WriteWord(address, --data);
            SetZnWord(data);
        }

        private void Op1B()
        {
            var address = DirectPageAddressX();
            WriteByte(address, Asl(ReadFull(address)));
        }

        private void Op1C()
        {
            registers.a = Asl(registers.a);
        }

        private void Op1D()
        {
            SetZnByte(--registers.x);
        }

        private void Op1E()
        {
            CmpByte(registers.x, ReadFull(AbsoluteAddress()));
        }

        private void Op1F()
        {
            registers.pc = (ReadFullWord(AbsoluteAddressX()));
        }

        private void Op20()
        {
            flagP = (0);
        }

        private void Op21()
        {
            var var1 = ReadFullWord(0xffda);
            PushWord(registers.pc);
            registers.pc = var1;
        }

        private void Op22()
        {
            var address = DirectPageAddress();
            var data = ReadFull(address) | 0x02;
            WriteByte(address, data);
        }

        private void Op23()
        {
            Branch((ReadData(DirectPageAddress()) & 2) != 0);
        }

        private void Op24()
        {
            SetZnByte(And(ReadData(DirectPageAddress())));
        }

        private void Op25()
        {
            SetZnByte(And(ReadFull(AbsoluteAddress())));
        }

        private void Op26()
        {
            SetZnByte(And(ReadData(DirectPageXAddress())));
        }

        private void Op27()
        {
            SetZnByte(And(ReadFull(DirectPageAddressXIndirect())));
        }

        private void Op28()
        {
            SetZnByte(And(ImmediateByte()));
        }

        private void Op29()
        {
            var data = ReadData(DirectPageAddress());
            var address = DirectPageAddress();
            data &= ReadFull(address);
            WriteByte(address, data);
            SetZnByte(data);
        }

        private void Op2A()
        {
            var var1 = AbsoluteAddress();
            var var2 = 1 << (var1 >> 13);
            var1 &= 8191;
            if ((ReadFull(var1) & var2) == 0)
            {
                flagC = (true);
            }
        }

        private void Op2B()
        {
            var address = DirectPageAddress();
            WriteByte(address, Rol(ReadFull(address)));
        }

        private void Op2C()
        {
            var address = AbsoluteAddress();
            WriteByte(address, Rol(ReadFull(address)));
        }

        private void Op2D()
        {
            PushByte(registers.a);
        }

        private void Op2E()
        {
            Branch(ReadData(DirectPageAddress()) != registers.a);
        }

        private void Op2F()
        {
            TakeBranch();
        }

        private void Op30()
        {
            Branch(flagN);
        }

        private void Op31()
        {
            var var1 = ReadFullWord(0xffd8);
            PushWord(registers.pc);
            registers.pc = var1;
        }

        private void Op32()
        {
            var address = DirectPageAddress();
            var data = ReadFull(address) & ~0x02;
            WriteByte(address, data);
        }

        private void Op33()
        {
            Branch((ReadData(DirectPageAddress()) & 2) == 0);
        }

        private void Op34()
        {
            SetZnByte(And(ReadData(DirectPageAddressX())));
        }

        private void Op35()
        {
            SetZnByte(And(ReadFull(AbsoluteAddressX())));
        }

        private void Op36()
        {
            SetZnByte(And(ReadFull(AbsoluteAddressY())));
        }

        private void Op37()
        {
            SetZnByte(And(ReadFull(DirectPageAddressYIndirect())));
        }

        private void Op38()
        {
            var data = ImmediateByte();
            var address = DirectPageAddress();
            data &= ReadFull(address);
            WriteByte(address, data);
            SetZnByte(data);
        }

        private void Op39()
        {
            var data = ReadData(DirectPageYAddress());
            var address = DirectPageXAddress();
            data &= ReadFull(address);
            WriteByte(address, data);
            SetZnByte(data);
        }

        private void Op3A()
        {
            var address = DirectPageAddress();
            var data = ReadFullWord(address);
            WriteWord(address, ++data);
            SetZnWord(data);
        }

        private void Op3B()
        {
            var address = DirectPageAddressX();
            WriteByte(address, Rol(ReadFull(address)));
        }

        private void Op3C()
        {
            registers.a = (Rol(registers.a));
        }

        private void Op3D()
        {
            SetZnByte(++registers.x);
        }

        private void Op3E()
        {
            CmpByte(registers.x, ReadData(DirectPageAddress()));
        }

        private void Op3F()
        {
            var var1 = AbsoluteAddress();
            PushWord(registers.pc);
            registers.pc = var1;
        }

        private void Op40()
        {
            flagP = (256);
        }

        private void Op41()
        {
            var var1 = ReadFullWord(0xffd6);
            PushWord(registers.pc);
            registers.pc = var1;
        }

        private void Op42()
        {
            var address = DirectPageAddress();
            var data = ReadFull(address) | 0x04;
            WriteByte(address, data);
        }

        private void Op43()
        {
            Branch((ReadData(DirectPageAddress()) & 4) != 0);
        }

        private void Op44()
        {
            SetZnByte(Eor(ReadData(DirectPageAddress())));
        }

        private void Op45()
        {
            SetZnByte(Eor(ReadFull(AbsoluteAddress())));
        }

        private void Op46()
        {
            SetZnByte(Eor(ReadData(DirectPageXAddress())));
        }

        private void Op47()
        {
            SetZnByte(Eor(ReadFull(DirectPageAddressXIndirect())));
        }

        private void Op48()
        {
            SetZnByte(Eor(ImmediateByte()));
        }

        private void Op49()
        {
            var data = ReadData(DirectPageAddress());
            var address = DirectPageAddress();
            data ^= ReadFull(address);
            WriteByte(address, data);
            SetZnByte(data);
        }

        private void Op4A()
        {
            var var1 = AbsoluteAddress();
            var var2 = 1 << (var1 >> 13);
            var1 &= 8191;
            flagC = (flagC && (ReadFull(var1) & var2) != 0);
        }

        private void Op4B()
        {
            var address = DirectPageAddress();
            WriteByte(address, Lsr(ReadFull(address)));
        }

        private void Op4C()
        {
            var address = AbsoluteAddress();
            WriteByte(address, Lsr(ReadFull(address)));
        }

        private void Op4D()
        {
            PushByte(registers.x);
        }

        private void Op4E()
        {
            var address = AbsoluteAddress();
            int data = ReadFull(address);
            SetZnByte(registers.a - data & 255);
            data &= ~registers.a;
            WriteByte(address, data);
        }

        private void Op4F()
        {
            var var1 = (ushort)(0xff00 | ImmediateByte());
            PushWord(registers.pc);
            registers.pc = var1;
        }

        private void Op50()
        {
            Branch(!flagV);
        }

        private void Op51()
        {
            var var1 = ReadFullWord(0xffd4);
            PushWord(registers.pc);
            registers.pc = var1;
        }

        private void Op52()
        {
            var var1 = DirectPageAddress();
            var var2 = ReadFull(var1) & ~0x04;
            WriteByte(var1, var2);
        }

        private void Op53()
        {
            Branch((ReadData(DirectPageAddress()) & 4) == 0);
        }

        private void Op54()
        {
            SetZnByte(Eor(ReadData(DirectPageAddressX())));
        }

        private void Op55()
        {
            SetZnByte(Eor(ReadFull(AbsoluteAddressX())));
        }

        private void Op56()
        {
            SetZnByte(Eor(ReadFull(AbsoluteAddressY())));
        }

        private void Op57()
        {
            SetZnByte(Eor(ReadFull(DirectPageAddressYIndirect())));
        }

        private void Op58()
        {
            var var1 = ImmediateByte();
            var var2 = DirectPageAddress();
            var1 ^= ReadFull(var2);
            WriteByte(var2, var1);
            SetZnByte(var1);
        }

        private void Op59()
        {
            var var1 = ReadData(flagP | registers.y);
            var var2 = DirectPageXAddress();
            var1 ^= ReadFull(var2);
            WriteByte(var2, var1);
            SetZnByte(var1);
        }

        private void Op5A()
        {
            var var1 = DirectPageAddressIndirect();
            CmpWord(registers.ya, var1);
        }

        private void Op5B()
        {
            var var1 = DirectPageAddressX();
            WriteByte(var1, Lsr(ReadFull(var1)));
        }

        private void Op5C()
        {
            registers.a = (Lsr(registers.a));
        }

        private void Op5D()
        {
            SetZnByte(registers.x = registers.a);
        }

        private void Op5E()
        {
            CmpByte(registers.y, ReadFull(AbsoluteAddress()));
        }

        private void Op5F()
        {
            registers.pc = (AbsoluteAddress());
        }

        private void Op60()
        {
            flagC = (false);
        }

        private void Op61()
        {
            var var1 = ReadFullWord(0xffd2);
            PushWord(registers.pc);
            registers.pc = var1;
        }

        private void Op62()
        {
            var var1 = DirectPageAddress();
            var var2 = ReadFull(var1) | 0x08;
            WriteByte(var1, var2);
        }

        private void Op63()
        {
            Branch((ReadData(DirectPageAddress()) & 8) != 0);
        }

        private void Op64()
        {
            CmpByte(registers.a, ReadData(DirectPageAddress()));
        }

        private void Op65()
        {
            CmpByte(registers.a, ReadFull(AbsoluteAddress()));
        }

        private void Op66()
        {
            CmpByte(registers.a, ReadData(DirectPageXAddress()));
        }

        private void Op67()
        {
            CmpByte(registers.a, ReadFull(DirectPageAddressXIndirect()));
        }

        private void Op68()
        {
            CmpByte(registers.a, ImmediateByte());
        }

        private void Op69()
        {
            var var1 = ReadData(DirectPageAddress());
            CmpByte(ReadData(DirectPageAddress()), var1);
        }

        private void Op6A()
        {
            var var1 = AbsoluteAddress();
            var var2 = 1 << (var1 >> 13);
            var1 &= 8191;
            flagC = (flagC && (ReadFull(var1) & var2) == 0);
        }

        private void Op6B()
        {
            var var1 = DirectPageAddress();
            WriteByte(var1, Ror(ReadFull(var1)));
        }

        private void Op6C()
        {
            var var1 = AbsoluteAddress();
            WriteByte(var1, Ror(ReadFull(var1)));
        }

        private void Op6D()
        {
            PushByte(registers.y);
        }

        private void Op6E()
        {
            var var1 = DirectPageAddress();
            var var2 = ReadFull(var1) - 1 & 255;
            WriteByte(var1, var2);
            Branch(var2 != 0);
        }

        private void Op6F()
        {
            registers.pc = (PullWord());
        }

        private void Op70()
        {
            Branch(flagV);
        }

        private void Op71()
        {
            var var1 = ReadFullWord(0xffd0);
            PushWord(registers.pc);
            registers.pc = var1;
        }

        private void Op72()
        {
            var var1 = DirectPageAddress();
            var var2 = ReadFull(var1) & ~0x08;
            WriteByte(var1, var2);
        }

        private void Op73()
        {
            Branch((ReadData(DirectPageAddress()) & 8) == 0);
        }

        private void Op74()
        {
            CmpByte(registers.a, ReadData(DirectPageAddressX()));
        }

        private void Op75()
        {
            CmpByte(registers.a, ReadFull(AbsoluteAddressX()));
        }

        private void Op76()
        {
            CmpByte(registers.a, ReadFull(AbsoluteAddressY()));
        }

        private void Op77()
        {
            CmpByte(registers.a, ReadFull(DirectPageAddressYIndirect()));
        }

        private void Op78()
        {
            var var1 = ImmediateByte();
            CmpByte(ReadData(DirectPageAddress()), var1);
        }

        private void Op79()
        {
            var var1 = ReadData(flagP | registers.y);
            CmpByte(ReadData(DirectPageXAddress()), var1);
        }

        private void Op7A()
        {
            registers.ya = Add(registers.ya, DirectPageAddressIndirect());
        }

        private void Op7B()
        {
            var var1 = DirectPageAddressX();
            WriteByte(var1, Ror(ReadFull(var1)));
        }

        private void Op7C()
        {
            registers.a = (Ror(registers.a));
        }

        private void Op7D()
        {
            SetZnByte(registers.a = (registers.x));
        }

        private void Op7E()
        {
            CmpByte(registers.y, ReadData(DirectPageAddress()));
        }

        private void Op7F()
        {
            LoadFlags(PullByte());
            registers.pc = (PullWord());
        }

        private void Op80()
        {
            flagC = (true);
        }

        private void Op81()
        {
            var var1 = ReadFullWord(0xffce);
            PushWord(registers.pc);
            registers.pc = var1;
        }

        private void Op82()
        {
            var var1 = DirectPageAddress();
            var var2 = ReadFull(var1) | 0x10;
            WriteByte(var1, var2);
        }

        private void Op83()
        {
            Branch((ReadData(DirectPageAddress()) & 16) != 0);
        }

        private void Op84()
        {
            registers.a = Adc(registers.a, ReadData(DirectPageAddress()));
        }

        private void Op85()
        {
            registers.a = (Adc(registers.a, ReadFull(AbsoluteAddress())));
        }

        private void Op86()
        {
            registers.a = (Adc(registers.a, ReadData(DirectPageXAddress())));
        }

        private void Op87()
        {
            registers.a = (Adc(registers.a, ReadFull(DirectPageAddressXIndirect())));
        }

        private void Op88()
        {
            registers.a = (Adc(registers.a, ImmediateByte()));
        }

        private void Op89()
        {
            var var1 = ReadData(DirectPageAddress());
            var var2 = DirectPageAddress();
            var var3 = ReadFull(var2);
            var3 = Adc(var3, var1);
            WriteByte(var2, var3);
        }

        private void Op8A()
        {
            var var1 = AbsoluteAddress();
            var var2 = 1 << (var1 >> 13);
            var1 &= 8191;
            flagC = (flagC ^ (ReadFull(var1) & var2) != 0);
        }

        private void Op8B()
        {
            var var1 = DirectPageAddress();
            var var2 = ReadFull(var1) - 1 & 255;
            WriteByte(var1, var2);
            SetZnByte(var2);
        }

        private void Op8C()
        {
            var var1 = AbsoluteAddress();
            var var2 = ReadFull(var1) - 1 & 255;
            WriteByte(var1, var2);
            SetZnByte(var2);
        }

        private void Op8D()
        {
            SetZnByte(registers.y = (ImmediateByte()));
        }

        private void Op8E()
        {
            LoadFlags(PullByte());
        }

        private void Op8F()
        {
            var var1 = ImmediateByte();
            WriteByte(DirectPageAddress(), var1);
        }

        private void Op90()
        {
            Branch(!flagC);
        }

        private void Op91()
        {
            var var1 = ReadFullWord(0xffcc);
            PushWord(registers.pc);
            registers.pc = var1;
        }

        private void Op92()
        {
            var var1 = DirectPageAddress();
            var var2 = ReadFull(var1) & ~0x10;
            WriteByte(var1, var2);
        }

        private void Op93()
        {
            Branch((ReadData(DirectPageAddress()) & 16) == 0);
        }

        private void Op94()
        {
            registers.a = (Adc(registers.a, ReadData(DirectPageAddressX())));
        }

        private void Op95()
        {
            registers.a = (Adc(registers.a, ReadFull(AbsoluteAddressX())));
        }

        private void Op96()
        {
            registers.a = (Adc(registers.a, ReadFull(AbsoluteAddressY())));
        }

        private void Op97()
        {
            registers.a = (Adc(registers.a, ReadFull(DirectPageAddressYIndirect())));
        }

        private void Op98()
        {
            var var1 = ImmediateByte();
            var var2 = DirectPageAddress();
            var var3 = ReadFull(var2);
            var3 = Adc(var3, var1);
            WriteByte(var2, var3);
        }

        private void Op99()
        {
            var var1 = ReadData(flagP | registers.y);
            var var2 = DirectPageXAddress();
            var var3 = ReadFull(var2);
            var3 = Adc(var3, var1);
            WriteByte(var2, var3);
        }

        private void Op9A()
        {
            registers.ya = Sub(registers.ya, DirectPageAddressIndirect());
        }

        private void Op9B()
        {
            var var1 = DirectPageAddressX();
            var var2 = ReadFull(var1) - 1 & 255;
            WriteByte(var1, var2);
            SetZnByte(var2);
        }

        private void Op9C()
        {
            SetZnByte(--registers.a);
        }

        private void Op9D()
        {
            SetZnByte(registers.x = registers.spl);
        }

        private void Op9E()
        {
            flagV = (registers.y & 0xff) >= (registers.x & 0xff);
            flagH = (registers.y & 0x0f) >= (registers.x & 0x0f);

            int y = registers.ya % registers.x;
            int a = registers.ya / registers.x;

            registers.y = (byte)y;
            registers.a = (byte)a;

            SetZnByte(registers.a);
        }

        private void Op9F()
        {
            registers.a = (byte)((registers.a << 4) | (registers.a >> 4));
            SetZnByte(registers.a);
        }

        private void OpA0()
        {
        }

        private void OpA1()
        {
            var var1 = ReadFullWord(0xffca);
            PushWord(registers.pc);
            registers.pc = var1;
        }

        private void OpA2()
        {
            var var1 = DirectPageAddress();
            var var2 = ReadFull(var1) | 0x20;
            WriteByte(var1, var2);
        }

        private void OpA3()
        {
            Branch((ReadData(DirectPageAddress()) & 32) != 0);
        }

        private void OpA4()
        {
            registers.a = (Sbc(registers.a, ReadData(DirectPageAddress())));
        }

        private void OpA5()
        {
            registers.a = (Sbc(registers.a, ReadFull(AbsoluteAddress())));
        }

        private void OpA6()
        {
            registers.a = (Sbc(registers.a, ReadData(DirectPageXAddress())));
        }

        private void OpA7()
        {
            registers.a = (Sbc(registers.a, ReadFull(DirectPageAddressXIndirect())));
        }

        private void OpA8()
        {
            registers.a = (Sbc(registers.a, ImmediateByte()));
        }

        private void OpA9()
        {
            var var1 = ReadData(DirectPageAddress());
            var var2 = DirectPageAddress();
            var var3 = ReadFull(var2);
            var3 = Sbc(var3, var1);
            WriteByte(var2, var3);
        }

        private void OpAA()
        {
            var var1 = AbsoluteAddress();
            var var2 = 1 << (var1 >> 13);
            var1 &= 8191;
            flagC = ((ReadFull(var1) & var2) != 0);
        }

        private void OpAB()
        {
            var var1 = DirectPageAddress();
            var var2 = ReadFull(var1) + 1 & 255;
            WriteByte(var1, var2);
            SetZnByte(var2);
        }

        private void OpAC()
        {
            var var1 = AbsoluteAddress();
            var var2 = ReadFull(var1) + 1 & 255;
            WriteByte(var1, var2);
            SetZnByte(var2);
        }

        private void OpAD()
        {
            CmpByte(registers.y, ImmediateByte());
        }

        private void OpAE()
        {
            registers.a = (PullByte());
        }

        private void OpAF()
        {
            var var1 = registers.a;
            WriteByte(DirectPageXAddressIncrement(), var1);
        }

        private void OpB0()
        {
            Branch(flagC);
        }

        private void OpB1()
        {
            var var1 = ReadFullWord(0xffc8);
            PushWord(registers.pc);
            registers.pc = var1;
        }

        private void OpB2()
        {
            var var1 = DirectPageAddress();
            var var2 = ReadFull(var1) & ~0x20;
            WriteByte(var1, var2);
        }

        private void OpB3()
        {
            Branch((ReadData(DirectPageAddress()) & 32) == 0);
        }

        private void OpB4()
        {
            registers.a = (Sbc(registers.a, ReadData(DirectPageAddressX())));
        }

        private void OpB5()
        {
            registers.a = (Sbc(registers.a, ReadFull(AbsoluteAddressX())));
        }

        private void OpB6()
        {
            registers.a = (Sbc(registers.a, ReadFull(AbsoluteAddressY())));
        }

        private void OpB7()
        {
            registers.a = (Sbc(registers.a, ReadFull(DirectPageAddressYIndirect())));
        }

        private void OpB8()
        {
            var var1 = ImmediateByte();
            var var2 = DirectPageAddress();
            var var3 = ReadFull(var2);
            var3 = Sbc(var3, var1);
            WriteByte(var2, var3);
        }

        private void OpB9()
        {
            var var1 = ReadData(flagP | registers.y);
            var var2 = DirectPageXAddress();
            var var3 = ReadFull(var2);
            var3 = Sbc(var3, var1);
            WriteByte(var2, var3);
        }

        private void OpBA()
        {
            var var1 = DirectPageAddressIndirect();
            registers.ya = var1;
            SetZnWord(var1);
        }

        private void OpBB()
        {
            var var1 = DirectPageAddressX();
            var var2 = ReadFull(var1) + 1 & 255;
            WriteByte(var1, var2);
            SetZnByte(var2);
        }

        private void OpBC()
        {
            SetZnByte(++registers.a);
        }

        private void OpBD()
        {
            registers.spl = registers.x;
        }

        private void OpBE()
        {
            var var1 = registers.a;
            if (!flagC || var1 > 153)
            {
                var1 -= 96;
                flagC = (false);
            }
            if (!flagH || (var1 & 15) > 9)
            {
                var1 -= 6;
            }
            var1 &= 255;
            registers.a = var1;
            SetZnByte(var1);
        }

        private void OpBF()
        {
            SetZnByte(registers.a = (ReadData(DirectPageXAddressIncrement())));
        }

        private void OpC0()
        {
        }

        private void OpC1()
        {
            var var1 = ReadFullWord(0xffc6);
            PushWord(registers.pc);
            registers.pc = var1;
        }

        private void OpC2()
        {
            var var1 = DirectPageAddress();
            var var2 = ReadFull(var1) | 0x40;
            WriteByte(var1, var2);
        }

        private void OpC3()
        {
            Branch((ReadData(DirectPageAddress()) & 64) != 0);
        }

        private void OpC4()
        {
            var var1 = registers.a;
            WriteByte(DirectPageAddress(), var1);
        }

        private void OpC5()
        {
            var var1 = registers.a;
            WriteByte(AbsoluteAddress(), var1);
        }

        private void OpC6()
        {
            var var1 = registers.a;
            WriteByte(DirectPageXAddress(), var1);
        }

        private void OpC7()
        {
            var var1 = registers.a;
            WriteByte(DirectPageAddressXIndirect(), var1);
        }

        private void OpC8()
        {
            CmpByte(registers.x, ImmediateByte());
        }

        private void OpC9()
        {
            var var1 = registers.x;
            WriteByte(AbsoluteAddress(), var1);
        }

        private void OpCA()
        {
            var var1 = AbsoluteAddress();
            var var2 = 1 << (var1 >> 13);
            var1 &= 8191;
            var2 = ReadFull(var1) & ~var2 | (flagC ? var2 : 0);
            WriteByte(var1, var2);
        }

        private void OpCB()
        {
            var var1 = registers.y;
            WriteByte(DirectPageAddress(), var1);
        }

        private void OpCC()
        {
            var var1 = registers.y;
            WriteByte(AbsoluteAddress(), var1);
        }

        private void OpCD()
        {
            SetZnByte(registers.x = (ImmediateByte()));
        }

        private void OpCE()
        {
            registers.x = (PullByte());
        }

        private void OpCF()
        {
            registers.ya = (ushort)(registers.y * registers.a);

            flagN = (registers.y & 0x80) != 0;
            flagZ = (registers.ya == 0);
        }

        private void OpD0()
        {
            Branch(!flagZ);
        }

        private void OpD1()
        {
            var var1 = ReadFullWord(0xffc4);
            PushWord(registers.pc);
            registers.pc = var1;
        }

        private void OpD2()
        {
            var var1 = DirectPageAddress();
            var var2 = ReadFull(var1) & ~0x40;
            WriteByte(var1, var2);
        }

        private void OpD3()
        {
            Branch((ReadData(DirectPageAddress()) & 64) == 0);
        }

        private void OpD4()
        {
            var var1 = registers.a;
            WriteByte(DirectPageAddressX(), var1);
        }

        private void OpD5()
        {
            var var1 = registers.a;
            WriteByte(AbsoluteAddressX(), var1);
        }

        private void OpD6()
        {
            var var1 = registers.a;
            WriteByte(AbsoluteAddressY(), var1);
        }

        private void OpD7()
        {
            var var1 = registers.a;
            WriteByte(DirectPageAddressYIndirect(), var1);
        }

        private void OpD8()
        {
            var var1 = registers.x;
            WriteByte(DirectPageAddress(), var1);
        }

        private void OpD9()
        {
            var var1 = registers.x;
            WriteByte(DirectPageAddressY(), var1);
        }

        private void OpDA()
        {
            WriteWord(DirectPageAddress(), registers.ya);
        }

        private void OpDB()
        {
            var var1 = registers.y;
            WriteByte(DirectPageAddressX(), var1);
        }

        private void OpDC()
        {
            SetZnByte(--registers.y);
        }

        private void OpDD()
        {
            SetZnByte(registers.a = (registers.y));
        }

        private void OpDE()
        {
            Branch(ReadData(DirectPageAddressX()) != registers.a);
        }

        private void OpDF()
        {
            var var1 = registers.a;
            if (flagC || var1 > 153)
            {
                var1 += 96;
                flagC = (true);
            }
            if (flagH || (var1 & 15) > 9)
            {
                var1 += 6;
            }
            var1 &= 255;
            registers.a = var1;
            SetZnByte(var1);
        }

        private void OpE0()
        {
            flagV = (false);
            flagH = (false);
        }

        private void OpE1()
        {
            var var1 = ReadFullWord(0xffc2);
            PushWord(registers.pc);
            registers.pc = var1;
        }

        private void OpE2()
        {
            var var1 = DirectPageAddress();
            var var2 = ReadFull(var1) | 0x80;
            WriteByte(var1, var2);
        }

        private void OpE3()
        {
            Branch((ReadData(DirectPageAddress()) & 128) != 0);
        }

        private void OpE4()
        {
            SetZnByte(registers.a = (ReadData(DirectPageAddress())));
        }

        private void OpE5()
        {
            SetZnByte(registers.a = (ReadFull(AbsoluteAddress())));
        }

        private void OpE6()
        {
            SetZnByte(registers.a = (ReadData(DirectPageXAddress())));
        }

        private void OpE7()
        {
            SetZnByte(registers.a = (ReadFull(DirectPageAddressXIndirect())));
        }

        private void OpE8()
        {
            SetZnByte(registers.a = (ImmediateByte()));
        }

        private void OpE9()
        {
            SetZnByte(registers.x = (ReadFull(AbsoluteAddress())));
        }

        private void OpEA()
        {
            var var1 = AbsoluteAddress();
            var var2 = 1 << (var1 >> 13);
            var1 &= 8191;
            WriteByte(var1, ReadFull(var1) ^ var2);
        }

        private void OpEB()
        {
            SetZnByte(registers.y = (ReadData(DirectPageAddress())));
        }

        private void OpEC()
        {
            SetZnByte(registers.y = (ReadFull(AbsoluteAddress())));
        }

        private void OpED()
        {
            flagC = (!flagC);
        }

        private void OpEE()
        {
            registers.y = (PullByte());
        }

        private void OpEF()
        {
            registers.pc--;
        }

        private void OpF0()
        {
            Branch(flagZ);
        }

        private void OpF1()
        {
            var var1 = ReadFullWord(0xffc0);
            PushWord(registers.pc);
            registers.pc = var1;
        }

        private void OpF2()
        {
            var var1 = DirectPageAddress();
            var var2 = ReadFull(var1) & ~0x80;
            WriteByte(var1, var2);
        }

        private void OpF3()
        {
            Branch((ReadData(DirectPageAddress()) & 128) == 0);
        }

        private void OpF4()
        {
            SetZnByte(registers.a = (ReadData(DirectPageAddressX())));
        }

        private void OpF5()
        {
            SetZnByte(registers.a = (ReadFull(AbsoluteAddressX())));
        }

        private void OpF6()
        {
            SetZnByte(registers.a = (ReadFull(AbsoluteAddressY())));
        }

        private void OpF7()
        {
            SetZnByte(registers.a = (ReadFull(DirectPageAddressYIndirect())));
        }

        private void OpF8()
        {
            SetZnByte(registers.x = (ReadData(DirectPageAddress())));
        }

        private void OpF9()
        {
            SetZnByte(registers.x = (ReadData(DirectPageAddressY())));
        }

        private void OpFA()
        {
            var var1 = ReadData(DirectPageAddress());
            WriteByte(DirectPageAddress(), var1);
        }

        private void OpFB()
        {
            SetZnByte(registers.y = (ReadData(DirectPageAddressX())));
        }

        private void OpFC()
        {
            SetZnByte(++registers.y);
        }

        private void OpFD()
        {
            SetZnByte(registers.y = registers.a);
        }

        private void OpFE()
        {
            registers.y--;
            Branch(registers.y != 0);
        }

        private void OpFF()
        {
            registers.pc--;
        }

        #endregion

        public void Consume(ClockSignal e)
        {
            Update(e.Cycles * 45056);
        }

        private struct Timer
        {
            public bool Enabled;
            public int Compare;
            public int Stage1;
            public int Stage2;

            public void Update(int clocks)
            {
                if (Enabled)
                {
                    for (var i = 0; i < clocks; i++)
                    {
                        Stage1 = (Stage1 + 1) & 0xff;

                        if (Stage1 == Compare)
                        {
                            Stage1 = 0;
                            Stage2++;
                        }
                    }

                    Stage2 &= 15;
                }
            }
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct Registers
        {
            [FieldOffset(0)]
            public ushort sp;

            [FieldOffset(2)]
            public ushort pc;

            [FieldOffset(4)]
            public byte a;

            [FieldOffset(5)]
            public byte y;

            [FieldOffset(6)]
            public byte x;

            [FieldOffset(4)]
            public ushort ya;

            #region Byte Accessors

            [FieldOffset(0)]
            public byte spl;

            [FieldOffset(1)]
            public byte sph;

            [FieldOffset(2)]
            public byte pcl;

            [FieldOffset(3)]
            public byte pch;

            #endregion
        }
    }
}
