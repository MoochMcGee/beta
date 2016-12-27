using System.Runtime.InteropServices;
using Beta.Platform.Core;
using Beta.Platform.Exceptions;

namespace Beta.Platform.Processors
{
    public abstract class LR35902
    {
        private Status sr;
        private Registers registers;
        private byte code;

        protected bool Halt;
        protected bool Stop;

        public Interrupt interrupt;

        private static int CarryBits(int a, int b, int r)
        {
            return (a & b) | ((a ^ b) & ~r);
        }

        private bool Flag()
        {
            switch ((code >> 3) & 3u)
            {
            default: return sr.Z == 0;
            case 1u: return sr.Z == 1;
            case 2u: return sr.C == 0;
            case 3u: return sr.C == 1;
            }
        }

        private byte Operand(int field)
        {
            switch (field & 7)
            {
            case 0: return registers.b;
            case 1: return registers.c;
            case 2: return registers.d;
            case 3: return registers.e;
            case 4: return registers.h;
            case 5: return registers.l;
            case 6: return Read(registers.hl);
            case 7: return registers.a;
            }

            throw new CompilerPleasingException();
        }

        private void Operand(int field, byte data)
        {
            switch (field & 7)
            {
            case 0: registers.b = data; break;
            case 1: registers.c = data; break;
            case 2: registers.d = data; break;
            case 3: registers.e = data; break;
            case 4: registers.h = data; break;
            case 5: registers.l = data; break;
            case 6: Write(registers.hl, data); break;
            case 7: registers.a = data; break;
            }
        }

        private void ExtCode()
        {
            var op = Operand(code = Read(registers.pc++));

            switch (code >> 3)
            {
            case 0x00: Operand(code, Shl(op, op >> 7)); break; // rlc
            case 0x01: Operand(code, Shr(op, op & 1)); break; // rrc
            case 0x02: Operand(code, Shl(op, sr.C)); break; // rl
            case 0x03: Operand(code, Shr(op, sr.C)); break; // rr
            case 0x04: Operand(code, Shl(op)); break; // sla
            case 0x05: Operand(code, Shr(op, op >> 7)); break; // sra
            case 0x06: Operand(code, Swap(op)); break; // swap
            case 0x07: Operand(code, Shr(op)); break; // srl
            case 0x08: case 0x09: case 0x0a: case 0x0b: case 0x0c: case 0x0d: case 0x0e: case 0x0f: Bit(op); break; // bit
            case 0x10: case 0x11: case 0x12: case 0x13: case 0x14: case 0x15: case 0x16: case 0x17: Operand(code, Res(op)); break; // res
            case 0x18: case 0x19: case 0x1a: case 0x1b: case 0x1c: case 0x1d: case 0x1e: case 0x1f: Operand(code, Set(op)); break; // set
            }
        }

        private void StdCode()
        {
            switch (code = Read(registers.pc++))
            {
            case 0x00: break;
            case 0x10: /*stop = true;
                while ( stop ) gameSystem.Dispatch( );*/
                OnStop();
                break; // todo: find out why gb cpu test rom uses this without joypad interrupts enabled.
            case 0x76:
                Halt = true;
                while (Halt) Dispatch(); break;
            case 0xcb: ExtCode(); break;
            case 0xf3: interrupt.ff1 = 0; break;
            case 0xfb: interrupt.ff1 = 1; break;

            case 0x07: Rol(registers.a >> 7); break;
            case 0x0f: Ror(registers.a & 1); break;
            case 0x17: Rol(sr.C); break;
            case 0x1f: Ror(sr.C); break;
            case 0x27: Daa(); break;
            case 0x2f: Cpl(); break;
            case 0x37: Scf(); break;
            case 0x3f: Ccf(); break;

            case 0xcd: case 0xc4: case 0xcc: case 0xd4: case 0xdc: Call(); break;
            case 0xc9: case 0xc0: case 0xc8: case 0xd0: case 0xd8: Ret(); break;
            case 0xc3: case 0xc2: case 0xca: case 0xd2: case 0xda: Jp(); break;
            case 0x18: case 0x20: case 0x28: case 0x30: case 0x38: Jr(); break;

            case 0x01: Ld(ref registers.bc); break;
            case 0x11: Ld(ref registers.de); break;
            case 0x21: Ld(ref registers.hl); break;
            case 0x31: Ld(ref registers.sp); break;

            case 0x02: Write(registers.bc, registers.a); break;
            case 0x12: Write(registers.de, registers.a); break;
            case 0x22: Write(registers.hl, registers.a); registers.hl++; break;
            case 0x32: Write(registers.hl, registers.a); registers.hl--; break;

            case 0x0a: registers.a = Read(registers.bc); break;
            case 0x1a: registers.a = Read(registers.de); break;
            case 0x2a: registers.a = Read(registers.hl); registers.hl++; break;
            case 0x3a: registers.a = Read(registers.hl); registers.hl--; break;

            case 0xe0: Write((ushort)(0xff00 + Read(registers.pc++)), registers.a); break;
            case 0xe2: Write((ushort)(0xff00 + registers.c), registers.a); break;
            case 0xf0: registers.a = Read((ushort)(0xff00 + Read(registers.pc++))); break;
            case 0xf2: registers.a = Read((ushort)(0xff00 + registers.c)); break;

            case 0xea: // ld ($nnnn),a
                registers.aal = Read(registers.pc++);
                registers.aah = Read(registers.pc++);

                Write(registers.aa, registers.a);
                break;

            case 0xfa: // ld a,($nnnn)
                registers.aal = Read(registers.pc++);
                registers.aah = Read(registers.pc++);

                registers.a = Read(registers.aa);
                break;

            case 0x08: // ld ($nnnn),sp
                registers.aal = Read(registers.pc++);
                registers.aah = Read(registers.pc++);

                Write(registers.aa, registers.spl); registers.aa++;
                Write(registers.aa, registers.sph);
                break;

            case 0xd9: Reti(); break;

            case 0xe8: // add sp,sp,#$nn
                {
                    var data = Read(registers.pc++);
                    var temp = (ushort)(registers.sp + (sbyte)data);
                    var bits = CarryBits(registers.sp, data, temp);

                    sr.Z = 0;
                    sr.N = 0;
                    sr.H = (bits >> 3) & 1;
                    sr.C = (bits >> 7) & 1;

                    registers.sp = temp;

                    Dispatch();
                    Dispatch();
                }
                break;

            case 0xf8: // add hl,sp,#$nn
                {
                    var data = Read(registers.pc++);
                    var temp = (ushort)(registers.sp + (sbyte)data);
                    var bits = CarryBits(registers.sp, data, temp);

                    sr.Z = 0;
                    sr.N = 0;
                    sr.H = (bits >> 3) & 1;
                    sr.C = (bits >> 7) & 1;

                    registers.hl = temp;

                    Dispatch();
                }
                break;

            case 0xe9: /*       */ registers.pc = registers.hl; break; // ld pc,hl
            case 0xf9: Dispatch(); registers.sp = registers.hl; break; // ld sp,hl

            case 0x03: Dispatch(); registers.bc++; break;
            case 0x13: Dispatch(); registers.de++; break;
            case 0x23: Dispatch(); registers.hl++; break;
            case 0x33: Dispatch(); registers.sp++; break;

            case 0x0b: Dispatch(); registers.bc--; break;
            case 0x1b: Dispatch(); registers.de--; break;
            case 0x2b: Dispatch(); registers.hl--; break;
            case 0x3b: Dispatch(); registers.sp--; break;

            case 0x09: Add(ref registers.bc); break;
            case 0x19: Add(ref registers.de); break;
            case 0x29: Add(ref registers.hl); break;
            case 0x39: Add(ref registers.sp); break;

            case 0x04: case 0x0c: case 0x14: case 0x1c: case 0x24: case 0x2c: case 0x34: case 0x3c: Operand(code >> 3, Inc(Operand(code >> 3))); break;
            case 0x05: case 0x0d: case 0x15: case 0x1d: case 0x25: case 0x2d: case 0x35: case 0x3d: Operand(code >> 3, Dec(Operand(code >> 3))); break;
            case 0x06: case 0x0e: case 0x16: case 0x1e: case 0x26: case 0x2e: case 0x36: case 0x3e: Operand(code >> 3, Read(registers.pc++)); break;
            case 0x40: case 0x41: case 0x42: case 0x43: case 0x44: case 0x45: case 0x46: case 0x47: Ld(); break;
            case 0x48: case 0x49: case 0x4a: case 0x4b: case 0x4c: case 0x4d: case 0x4e: case 0x4f: Ld(); break;
            case 0x50: case 0x51: case 0x52: case 0x53: case 0x54: case 0x55: case 0x56: case 0x57: Ld(); break;
            case 0x58: case 0x59: case 0x5a: case 0x5b: case 0x5c: case 0x5d: case 0x5e: case 0x5f: Ld(); break;
            case 0x60: case 0x61: case 0x62: case 0x63: case 0x64: case 0x65: case 0x66: case 0x67: Ld(); break;
            case 0x68: case 0x69: case 0x6a: case 0x6b: case 0x6c: case 0x6d: case 0x6e: case 0x6f: Ld(); break;
            case 0x70: case 0x71: case 0x72: case 0x73: case 0x74: case 0x75: /*  76  */ case 0x77: Ld(); break;
            case 0x78: case 0x79: case 0x7a: case 0x7b: case 0x7c: case 0x7d: case 0x7e: case 0x7f: Ld(); break;
            case 0x80: case 0x81: case 0x82: case 0x83: case 0x84: case 0x85: case 0x86: case 0x87: Add(Operand(code)); break;
            case 0x88: case 0x89: case 0x8a: case 0x8b: case 0x8c: case 0x8d: case 0x8e: case 0x8f: Add(Operand(code), sr.C); break;
            case 0x90: case 0x91: case 0x92: case 0x93: case 0x94: case 0x95: case 0x96: case 0x97: Sub(Operand(code)); break;
            case 0x98: case 0x99: case 0x9a: case 0x9b: case 0x9c: case 0x9d: case 0x9e: case 0x9f: Sub(Operand(code), sr.C); break;
            case 0xa0: case 0xa1: case 0xa2: case 0xa3: case 0xa4: case 0xa5: case 0xa6: case 0xa7: And(Operand(code)); break;
            case 0xa8: case 0xa9: case 0xaa: case 0xab: case 0xac: case 0xad: case 0xae: case 0xaf: Xor(Operand(code)); break;
            case 0xb0: case 0xb1: case 0xb2: case 0xb3: case 0xb4: case 0xb5: case 0xb6: case 0xb7: Or(Operand(code)); break;
            case 0xb8: case 0xb9: case 0xba: case 0xbb: case 0xbc: case 0xbd: case 0xbe: case 0xbf: Cp(Operand(code)); break;
            case 0xc7: Rst(0x00); break;
            case 0xcf: Rst(0x08); break;
            case 0xd7: Rst(0x10); break;
            case 0xdf: Rst(0x18); break;
            case 0xe7: Rst(0x20); break;
            case 0xef: Rst(0x28); break;
            case 0xf7: Rst(0x30); break;
            case 0xff: Rst(0x38); break;

            case 0xc1: Pop(ref registers.bc); /*                 */ break;
            case 0xd1: Pop(ref registers.de); /*                 */ break;
            case 0xe1: Pop(ref registers.hl); /*                 */ break;
            case 0xf1: Pop(ref registers.af); sr.Load(registers.f); break;

            case 0xc5: /*                    */ Push(ref registers.bc); break;
            case 0xd5: /*                    */ Push(ref registers.de); break;
            case 0xe5: /*                    */ Push(ref registers.hl); break;
            case 0xf5: registers.f = sr.Save(); Push(ref registers.af); break;

            case 0xc6: Add(Read(registers.pc++)); break;
            case 0xce: Add(Read(registers.pc++), sr.C); break;
            case 0xd6: Sub(Read(registers.pc++)); break;
            case 0xde: Sub(Read(registers.pc++), sr.C); break;
            case 0xe6: And(Read(registers.pc++)); break;
            case 0xee: Xor(Read(registers.pc++)); break;
            case 0xf6: Or(Read(registers.pc++)); break;
            case 0xfe: Cp(Read(registers.pc++)); break;

            case 0xd3:
            case 0xdb:
            case 0xdd:
            case 0xe3:
            case 0xe4:
            case 0xeb:
            case 0xec:
            case 0xed:
            case 0xf4:
            case 0xfc:
            case 0xfd: Jam(); break;
            }
        }

        #region Extended Opcodes

        private void Bit(byte data)
        {
            var shift = (code >> 3) & 7;

            sr.Z = (data & (1 << shift)) == 0 ? 1 : 0;
            sr.N = 0;
            sr.H = 1;
        }

        private byte Shl(byte data, int carry = 0)
        {
            sr.C = (data >> 7);

            data = (byte)((data << 1) | (carry));

            sr.Z = (data & 0xff) == 0 ? 1 : 0;
            sr.N = 0;
            sr.H = 0;

            return data;
        }

        private byte Shr(byte data, int carry = 0)
        {
            sr.C = (data & 0x01);

            data = (byte)((data >> 1) | (carry << 7));

            sr.Z = (data & 0xff) == 0 ? 1 : 0;
            sr.N = 0;
            sr.H = 0;

            return data;
        }

        private byte Res(byte data)
        {
            var mask = 1 << ((code >> 3) & 7);

            return (byte)(data & ~mask);
        }

        private byte Set(byte data)
        {
            var mask = 1 << ((code >> 3) & 7);

            return (byte)(data | mask);
        }

        private byte Swap(byte data)
        {
            data = (byte)((data >> 4) | (data << 4));

            sr.Z = data == 0 ? 1 : 0;
            sr.N = 0;
            sr.H = 0;
            sr.C = 0;

            return data;
        }

        #endregion

        #region Standard Opcodes

        // -- 8-bit instructions --
        private void Call()
        {
            var lo = Read(registers.pc++);
            var hi = Read(registers.pc++);

            if (code == 0xcd || Flag())
            {
                Dispatch();

                Write(--registers.sp, registers.pch);
                Write(--registers.sp, registers.pcl);

                registers.pcl = lo;
                registers.pch = hi;
            }
        }

        private void Ccf()
        {
            sr.N  = 0;
            sr.H  = 0;
            sr.C ^= 1;
        }

        private void Cpl()
        {
            registers.a ^= 0xff;
            sr.N = 1;
            sr.H = 1;
        }

        private void Daa()
        {
            if (sr.N == 1)
            {
                if (sr.C == 1) registers.a -= 0x60;
                if (sr.H == 1) registers.a -= 0x06;
            }
            else
            {
                if (sr.C == 1 || (registers.a & 0xff) > 0x99) { registers.a += 0x60; sr.C = 1; }
                if (sr.H == 1 || (registers.a & 0x0f) > 0x09) { registers.a += 0x06; }
            }

            sr.Z = registers.a == 0 ? 1 : 0;
            sr.H = 0;
        }

        private void Jam()
        {
            throw new ProcessorJammedException("Invalid instruction $" + code.ToString("x2"));
        }

        private void Jp()
        {
            var lo = Read(registers.pc++);
            var hi = Read(registers.pc++);

            if (code == 0xc3 || Flag())
            {
                Dispatch();

                registers.pcl = lo;
                registers.pch = hi;
            }
        }

        private void Jr()
        {
            var data = Read(registers.pc++);

            if (code == 0x18 || Flag())
            {
                Dispatch();
                registers.pc = (ushort)(registers.pc + (sbyte)data);
            }
        }

        private void Ld()
        {
            Operand(code >> 3, Operand(code));
        }

        private void Ret()
        {
            if (code == 0xc9 || Flag())
            {
                if (code != 0xc9) Dispatch();
                registers.pcl = Read(registers.sp++);
                registers.pch = Read(registers.sp++);
            }

            Dispatch();
        }

        private void Reti()
        {
            registers.pcl = Read(registers.sp++);
            registers.pch = Read(registers.sp++);

            Dispatch();

            interrupt.ff1 = 1;
        }

        private void Scf()
        {
            sr.N = 0;
            sr.H = 0;
            sr.C = 1;
        }

        private void And(byte data)
        {
            sr.Z = (registers.a &= data) == 0 ? 1 : 0;
            sr.N = 0;
            sr.H = 1;
            sr.C = 0;
        }

        private void Cp(byte data)
        {
            var temp = (registers.a + ~data + 1);
            var bits = ~CarryBits(registers.a, ~data, temp);

            sr.Z = (temp & 0xff) == 0 ? 1 : 0;
            sr.N = 1;
            sr.H = (bits >> 3) & 1;
            sr.C = (bits >> 7) & 1;
        }

        private byte Dec(byte data)
        {
            data--;

            sr.Z = (data & 0xff) == 0x00 ? 1 : 0;
            sr.N = 1;
            sr.H = (data & 0x0f) == 0x0f ? 1 : 0;

            return data;
        }

        private byte Inc(byte data)
        {
            data++;

            sr.Z = (data & 0xff) == 0x00 ? 1 : 0;
            sr.N = 0;
            sr.H = (data & 0x0f) == 0x00 ? 1 : 0;

            return data;
        }

        private void Or(byte data)
        {
            sr.Z = (registers.a |= data) == 0 ? 1 : 0;
            sr.N = 0;
            sr.H = 0;
            sr.C = 0;
        }

        private void Rol(int carry)
        {
            sr.Z = 0;
            sr.N = 0;
            sr.H = 0;
            sr.C = (registers.a >> 7);

            registers.a = (byte)((registers.a << 1) | (carry));
        }

        private void Ror(int carry)
        {
            sr.Z = 0;
            sr.N = 0;
            sr.H = 0;
            sr.C = (registers.a & 0x01);

            registers.a = (byte)((registers.a >> 1) | (carry << 7));
        }

        private void Xor(byte data)
        {
            sr.Z = (registers.a ^= data) == 0 ? 1 : 0;
            sr.N = 0;
            sr.H = 0;
            sr.C = 0;
        }

        private void Add(byte data, int carry = 0)
        {
            var temp = (registers.a + data) + carry;
            var bits = CarryBits(registers.a, data, temp);

            sr.Z = (temp & 0xff) == 0 ? 1 : 0;
            sr.N = 0;
            sr.H = (bits >> 3) & 1;
            sr.C = (bits >> 7) & 1;

            registers.a = (byte)temp;
        }

        private void Sub(byte data, int carry = 0)
        {
            var temp = (registers.a - data) - carry;
            var bits = ~CarryBits(registers.a, ~data, temp);

            sr.Z = (temp & 0xff) == 0 ? 1 : 0;
            sr.N = 1;
            sr.H = (bits >> 3) & 1;
            sr.C = (bits >> 7) & 1;

            registers.a = (byte)temp;
        }

        protected void Rst(byte addr)
        {
            Dispatch();

            Write(--registers.sp, registers.pch);
            Write(--registers.sp, registers.pcl);

            registers.pcl = addr;
            registers.pch = 0;
        }

        // -- 16-bit instructions --
        private void Add(ref ushort data)
        {
            var temp = (ushort)(registers.hl + data);
            var bits = CarryBits(registers.hl, data, temp);

            sr.N = 0;
            sr.H = (bits >> 11) & 1;
            sr.C = (bits >> 15) & 1;

            registers.hl = temp;

            Dispatch();
        }

        private void Ld(ref ushort data)
        {
            var l = Read(registers.pc++);
            var h = Read(registers.pc++);
            data = (ushort)((h << 8) | l);
        }

        private void Pop(ref ushort data)
        {
            var l = Read(registers.sp++);
            var h = Read(registers.sp++);
            data = (ushort)((h << 8) | l);
        }

        private void Push(ref ushort data)
        {
            Dispatch();

            Write(--registers.sp, (byte)(data >> 8));
            Write(--registers.sp, (byte)(data >> 0));
        }

        #endregion

        protected virtual void OnStop()
        {
        }

        protected virtual void OnHalt()
        {
        }

        protected abstract void Dispatch();

        protected abstract byte Read(ushort address);

        protected abstract void Write(ushort address, byte data);

        public virtual void Update()
        {
            StdCode();
        }

        private struct Status
        {
            public int Z;
            public int N;
            public int H;
            public int C;

            public void Load(byte value)
            {
                Z = (value >> 7) & 1;
                N = (value >> 6) & 1;
                H = (value >> 5) & 1;
                C = (value >> 4) & 1;
            }

            public byte Save()
            {
                return (byte)(
                    (Z << 7) |
                    (N << 6) |
                    (H << 5) |
                    (C << 4));
            }
        }

        public struct Interrupt
        {
            public int ff1;
            public int ff2;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct Registers
        {
            [FieldOffset(1)]
            public byte b;

            [FieldOffset(0)]
            public byte c;

            [FieldOffset(3)]
            public byte d;

            [FieldOffset(2)]
            public byte e;

            [FieldOffset(5)]
            public byte h;

            [FieldOffset(4)]
            public byte l;

            [FieldOffset(7)]
            public byte a;

            [FieldOffset(6)]
            public byte f;

            //
            [FieldOffset(8)]
            public byte spl;

            [FieldOffset(9)]
            public byte sph;

            [FieldOffset(10)]
            public byte pcl;

            [FieldOffset(11)]
            public byte pch;

            [FieldOffset(12)]
            public byte aal;

            [FieldOffset(13)]
            public byte aah;

            [FieldOffset(0)]
            public ushort bc;

            [FieldOffset(2)]
            public ushort de;

            [FieldOffset(4)]
            public ushort hl;

            [FieldOffset(6)]
            public ushort af;

            [FieldOffset(8)]
            public ushort sp;

            [FieldOffset(10)]
            public ushort pc;

            [FieldOffset(12)]
            public ushort aa;
        }
    }
}
