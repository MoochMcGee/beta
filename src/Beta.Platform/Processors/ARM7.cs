using System;
using Beta.Platform.Core;
using Beta.Platform.Exceptions;
using half = System.UInt16;

namespace Beta.Platform.Processors
{
    public abstract class Arm7 : Processor
    {
        private Action[] armv4Codes;
        private Action[] thumbCodes;
        private Flags cpsr = new Flags();
        private Flags spsr;
        private Flags spsrAbt = new Flags();
        private Flags spsrFiq = new Flags();
        private Flags spsrIrq = new Flags();
        private Flags spsrSvc = new Flags();
        private Flags spsrUnd = new Flags();
        private Pipeline pipeline = new Pipeline();
        private Register sp;
        private Register lr;
        private Register pc;
        private Register[] registersAbt = new Register[2];
        private Register[] registersFiq = new Register[7];
        private Register[] registersIrq = new Register[2];
        private Register[] registersSvc = new Register[2];
        private Register[] registersUnd = new Register[2];
        private Register[] registersUsr = new Register[7];
        private Register[] registers = new Register[16];
        private uint code;

        public bool halt;
        public bool interrupt;

        protected Arm7()
        {
            registersAbt.Initialize(() => new Register());
            registersFiq.Initialize(() => new Register());
            registersIrq.Initialize(() => new Register());
            registersSvc.Initialize(() => new Register());
            registersUnd.Initialize(() => new Register());
            registersUsr.Initialize(() => new Register());

            registers[0] = new Register();
            registers[1] = new Register();
            registers[2] = new Register();
            registers[3] = new Register();
            registers[4] = new Register();
            registers[5] = new Register();
            registers[6] = new Register();
            registers[7] = new Register();
            registers[15] = new Register();

            Isr(Mode.SVC, Vector.RST);
        }

        private static int MultiplierCycles(uint value)
        {
            if ((value & 0xffffff00) == 0 || (value & 0xffffff00) == 0xffffff00) return 1;
            if ((value & 0xffff0000) == 0 || (value & 0xffff0000) == 0xffff0000) return 2;
            if ((value & 0xff000000) == 0 || (value & 0xff000000) == 0xff000000) return 3;

            return 4;
        }

        private uint BarrelShifter()
        {
            var value = registers[code & 0xf].value;
            var shift = (code >> 7) & 0x1f;

            switch ((code >> 5) & 3)
            {
            case 0: return Lsl(value, shift);
            case 1: return Lsr(value, shift);
            case 2: return Asr(value, shift);
            case 3: return Ror(value, shift);
            }

            throw new CompilerPleasingException();
        }

        private void ChangeMode(uint mode)
        {
            ChangeRegisters(mode);

            if (spsr != null)
                spsr.Load(cpsr.Save());
        }

        private void ChangeRegisters(uint mode)
        {
            Register[] smallBank = null;
            Register[] largeBank = mode == Mode.FIQ
                ? registersFiq
                : registersUsr;

            registers[8] = largeBank[6];
            registers[9] = largeBank[5];
            registers[10] = largeBank[4];
            registers[11] = largeBank[3];
            registers[12] = largeBank[2];

            switch (mode)
            {
            case Mode.ABT: smallBank = registersAbt; spsr = spsrAbt; break;
            case Mode.FIQ: smallBank = registersFiq; spsr = spsrFiq; break;
            case Mode.IRQ: smallBank = registersIrq; spsr = spsrIrq; break;
            case Mode.SVC: smallBank = registersSvc; spsr = spsrSvc; break;
            case Mode.SYS: smallBank = registersUsr; spsr = null; break;
            case Mode.UND: smallBank = registersUnd; spsr = spsrUnd; break;
            case Mode.USR: smallBank = registersUsr; spsr = null; break;
            }

            sp = registers[13] = smallBank[1];
            lr = registers[14] = smallBank[0];
            pc = registers[15];
        }

        private bool GetCondition(uint condition)
        {
            switch (condition & 15)
            {
            case 0x0: /* EQ */ return cpsr.z != 0;
            case 0x1: /* NE */ return cpsr.z == 0;
            case 0x2: /* CS */ return cpsr.c != 0;
            case 0x3: /* CC */ return cpsr.c == 0;
            case 0x4: /* MI */ return cpsr.n != 0;
            case 0x5: /* PL */ return cpsr.n == 0;
            case 0x6: /* VS */ return cpsr.v != 0;
            case 0x7: /* VC */ return cpsr.v == 0;
            case 0x8: /* HI */ return cpsr.c != 0 && cpsr.z == 0;
            case 0x9: /* LS */ return cpsr.c == 0 || cpsr.z != 0;
            case 0xa: /* GE */ return cpsr.n == cpsr.v;
            case 0xb: /* LT */ return cpsr.n != cpsr.v;
            case 0xc: /* GT */ return cpsr.n == cpsr.v && cpsr.z == 0;
            case 0xd: /* LE */ return cpsr.n != cpsr.v || cpsr.z != 0;
            case 0xe: /* AL */ return true;
            case 0xf: /* NV */ return false;
            }

            throw new CompilerPleasingException();
        }

        private void Isr(uint mode, uint vector)
        {
            ChangeMode(mode);

            if (vector == Vector.FIQ ||
                vector == Vector.RST)
            {
                cpsr.f = 1;
            }

            cpsr.t = 0;
            cpsr.i = 1;
            cpsr.m = mode;

            lr.value = pipeline.decode.address;
            pc.value = vector;
            pipeline.refresh = true;
        }

        private uint LoadWord(uint address)
        {
            var data = Read(2, address & ~3u);

            switch (address & 3)
            {
            case 0: return (data >> 0) | (data << 32);
            case 1: return (data >> 8) | (data << 24);
            case 2: return (data >> 16) | (data << 16);
            case 3: return (data >> 24) | (data << 8);
            }

            throw new CompilerPleasingException();
        }

        private void OverflowCarryAdd(uint a, uint b, uint r)
        {
            var overflow = ~(a ^ b) & (a ^ r);

            cpsr.n = r >> 31;
            cpsr.z = r == 0 ? 1u : 0u;
            cpsr.c = (overflow ^ a ^ b ^ r) >> 31;
            cpsr.v = (overflow) >> 31;
        }

        private void OverflowCarrySub(uint a, uint b, uint r)
        {
            OverflowCarryAdd(a, ~b, r);
        }

        #region ALU

        private uint carryout;

        private uint Add(uint a, uint b, uint carry = 0)
        {
            var r = (a + b + carry);

            if (cpsr.t != 0U || (code & (1U << 20)) != 0)
            {
                var overflow = ~(a ^ b) & (a ^ r);

                cpsr.n = r >> 31;
                cpsr.z = r == 0 ? 1U : 0U;
                cpsr.c = (overflow ^ a ^ b ^ r) >> 31;
                cpsr.v = (overflow) >> 31;
            }

            return r;
        }

        private uint Sub(uint a, uint b, uint carry = 1)
        {
            return Add(a, ~b, carry);
        }

        private uint Mul(uint a, uint b, uint c)
        {
            Cycles += MultiplierCycles(b);

            a += b * c;

            if (cpsr.t != 0U || (code & (1 << 20)) != 0)
            {
                cpsr.n = a >> 31;
                cpsr.z = a == 0U ? 1U : 0U;
            }

            return a;
        }

        private uint Mov(uint value)
        {
            if (cpsr.t != 0U || (code & (1U << 20)) != 0)
            {
                cpsr.n = value >> 31;
                cpsr.z = value == 0U ? 1U : 0U;
                cpsr.c = carryout;
            }

            return value;
        }

        private uint Lsl(uint value, uint shift)
        {
            shift = (shift & 255U);

            carryout = cpsr.c;
            if (shift == 0) return value;

            carryout = shift > 32 ? 0 : (value >> (int)(32 - shift)) & 1U;
            value = shift > 31 ? 0 : (value << (int)shift);
            return value;
        }

        private uint Lsr(uint value, uint shift)
        {
            shift = (shift & 255U);

            carryout = cpsr.c;
            if (shift == 0) return value;

            carryout = shift > 32 ? 0 : (value >> (int)(shift - 1)) & 1U;
            value = shift > 31 ? 0 : (value >> (int)shift);
            return value;
        }

        private uint Asr(uint value, uint shift)
        {
            shift = (shift & 255U);

            carryout = cpsr.c;
            if (shift == 0) return value;

            carryout = shift > 32 ? (value >> 31) : (value >> (int)(shift - 1)) & 1U;
            value = shift > 31 ? (uint)((int)value >> 31) : (uint)((int)value >> (int)shift);
            return value;
        }

        private uint Ror(uint value, uint shift)
        {
            shift = (shift & 255U);

            carryout = cpsr.c;
            if (shift == 0) return value;

            if ((shift &= 31) != 0)
                value = (value >> (int)shift) | (value << (int)(32 - shift));

            carryout = (value >> 31);
            return value;
        }

        private uint Rrx(uint value)
        {
            carryout = value & 1U;
            return (value >> 1) | (cpsr.c << 31);
        }

        #endregion

        #region ARMv4

        private static uint Armv4Encode(uint code)
        {
            return ((code >> 16) & 0xff0) | ((code >> 4) & 0x00f);
        }

        private void Armv4Execute()
        {
            if (pipeline.refresh)
            {
                pipeline.refresh = false;
                pipeline.fetch.address = pc.value & ~3U;
                pipeline.fetch.data = Read(2, pipeline.fetch.address);

                Armv4Step();
            }

            Armv4Step();

            if (interrupt && cpsr.i == 0) // irq after pipeline initialized in correct mode
            {
                Isr(Mode.IRQ, Vector.IRQ);
                return;
            }

            code = pipeline.execute.data;

            if (GetCondition(code >> 28))
            {
                armv4Codes[Armv4Encode(code)]();
            }
        }

        private void Armv4Initialize()
        {
            armv4Codes = new Action[Armv4Encode(0xffffffff) + 1];

            Armv4Map("---- ---- ---- ---- ---- ---- ---- ----", OpUnd);
            Armv4Map("---- 0000 00-- ---- ---- ---- 1001 ----", OpMultiply);
            Armv4Map("---- 0000 1--- ---- ---- ---- 1001 ----", OpMultiplyLong);
            Armv4Map("---- 0001 0-00 ---- ---- 0000 1001 ----", OpSwap);
            Armv4Map("---- 0000 ---- ---- ---- ---- ---0 ----", OpAluRegImm);
            Armv4Map("---- 0001 0--1 ---- ---- ---- ---0 ----", OpAluRegImm);
            Armv4Map("---- 0001 1--- ---- ---- ---- ---0 ----", OpAluRegImm);
            Armv4Map("---- 0000 ---- ---- ---- ---- 0--1 ----", OpAluRegReg);
            Armv4Map("---- 0001 0--1 ---- ---- ---- 0--1 ----", OpAluRegReg);
            Armv4Map("---- 0001 1--- ---- ---- ---- 0--1 ----", OpAluRegReg);
            Armv4Map("---- 0010 ---- ---- ---- ---- ---- ----", OpAluImm);
            Armv4Map("---- 0011 0--1 ---- ---- ---- ---- ----", OpAluImm);
            Armv4Map("---- 0011 1--- ---- ---- ---- ---- ----", OpAluImm);
            Armv4Map("---- 0000 -00- ---- ---- 0000 1011 ----", OpMoveHalfReg);
            Armv4Map("---- 0001 -0-- ---- ---- 0000 1011 ----", OpMoveHalfReg);
            Armv4Map("---- 0000 -10- ---- ---- ---- 1011 ----", OpMoveHalfImm);
            Armv4Map("---- 0001 -1-- ---- ---- ---- 1011 ----", OpMoveHalfImm);
            Armv4Map("---- 0000 -001 ---- ---- 0000 11-1 ----", OpLoadReg);
            Armv4Map("---- 0001 -0-1 ---- ---- 0000 11-1 ----", OpLoadReg);
            Armv4Map("---- 0000 -101 ---- ---- ---- 11-1 ----", OpLoadImm);
            Armv4Map("---- 0001 -1-1 ---- ---- ---- 11-1 ----", OpLoadImm);
            Armv4Map("---- 0001 0010 ---- ---- ---- 0001 ----", OpBx);
            Armv4Map("---- 100- ---- ---- ---- ---- ---- ----", OpBlockTransfer);
            Armv4Map("---- 1010 ---- ---- ---- ---- ---- ----", OpB);
            Armv4Map("---- 1011 ---- ---- ---- ---- ---- ----", OpBl);
            Armv4Map("---- 1111 ---- ---- ---- ---- ---- ----", OpSwi);
            Armv4Map("---- 0001 0000 ---- ---- ---- 0000 ----", OpMrsCpsr);
            Armv4Map("---- 0001 0010 ---- ---- ---- 0000 ----", OpMsrCpsrReg);
            Armv4Map("---- 0001 0100 ---- ---- ---- 0000 ----", OpMrsSpsr);
            Armv4Map("---- 0001 0110 ---- ---- ---- 0000 ----", OpMsrSpsrReg);
            Armv4Map("---- 0011 0010 ---- ---- ---- 0000 ----", OpMsrCpsrImm);
            Armv4Map("---- 0011 0110 ---- ---- ---- 0000 ----", OpMsrSpsrImm);
            Armv4Map("---- 010- ---0 ---- ---- ---- ---- ----", OpStr);
            Armv4Map("---- 010- ---1 ---- ---- ---- ---- ----", OpLdr);
            Armv4Map("---- 011- ---0 ---- ---- ---- ---0 ----", OpStr);
            Armv4Map("---- 011- ---1 ---- ---- ---- ---0 ----", OpLdr);
        }

        private void Armv4Map(string pattern, Action code)
        {
            var mask = Armv4Encode(BitString.Mask(pattern));
            var zero = Armv4Encode(BitString.Min(pattern));
            var full = Armv4Encode(BitString.Max(pattern));

            for (var i = zero; i <= full; i++)
            {
                if ((i & mask) == zero)
                {
                    armv4Codes[i] = code;
                }
            }
        }

        private void Armv4Step()
        {
            pc.value += 4U;

            pipeline.execute = pipeline.decode;
            pipeline.decode = pipeline.fetch;
            pipeline.fetch.address = pc.value & ~3U;
            pipeline.fetch.data = Read(2, pipeline.fetch.address);
        }

        #region Opcodes

        private void OpAlu(uint value)
        {
            var rd = (code >> 12) & 15u;
            var rn = (code >> 16) & 15u;
            var check = false;

            switch (code >> 21 & 15u)
            {
            case 0x0U: check = true; registers[rd].value = Mov(registers[rn].value & value); break; // AND
            case 0x1U: check = true; registers[rd].value = Mov(registers[rn].value ^ value); break; // EOR
            case 0x2U: check = true; registers[rd].value = Sub(registers[rn].value, value); break; // SUB
            case 0x3U: check = true; registers[rd].value = Sub(value, registers[rn].value); break; // RSB
            case 0x4U: check = true; registers[rd].value = Add(registers[rn].value, value); break; // ADD
            case 0x5U: check = true; registers[rd].value = Add(registers[rn].value, value, cpsr.c); break; // ADC
            case 0x6U: check = true; registers[rd].value = Sub(registers[rn].value, value, cpsr.c); break; // SBC
            case 0x7U: check = true; registers[rd].value = Sub(value, registers[rn].value, cpsr.c); break; // RSC
            case 0x8U: Mov(registers[rn].value & value); break; // TST
            case 0x9U: Mov(registers[rn].value ^ value); break; // TEQ
            case 0xAU: Sub(registers[rn].value, value); break; // CMP
            case 0xBU: Add(registers[rn].value, value); break; // CMN
            case 0xCU: check = true; registers[rd].value = Mov(registers[rn].value | value); break; // ORR
            case 0xDU: check = true; registers[rd].value = Mov(value); break; // MOV
            case 0xEU: check = true; registers[rd].value = Mov(registers[rn].value & ~value); break; // BIC
            case 0xFU: check = true; registers[rd].value = Mov(~value); break; // MVN
            }

            if (rd == 15 && check)
            {
                if ((code & (1 << 20)) != 0 && spsr != null)
                {
                    cpsr.Load(spsr.Save());
                    ChangeMode(spsr.m);
                }

                pipeline.refresh = true;
            }
        }

        private void OpAluImm()
        {
            var shift = (code >> 7) & 30;
            var value = (code >> 0) & 255;

            carryout = cpsr.c;
            if (shift != 0) value = Ror(value, shift);

            OpAlu(value);
        }

        private void OpAluRegImm()
        {
            var shift = (code >> 7) & 31;
            var value = registers[(code >> 0) & 15].value;

            carryout = cpsr.c;

            switch ((code >> 5) & 3)
            {
            case 0: value = Lsl(value, shift); break;
            case 1: value = Lsr(value, shift == 0 ? 32 : shift); break;
            case 2: value = Asr(value, shift == 0 ? 32 : shift); break;
            case 3: value = shift != 0 ? Ror(value, shift) : Rrx(value); break;
            }

            OpAlu(value);
        }

        private void OpAluRegReg()
        {
            var rs = (code >> 8) & 15;
            var rm = (code >> 0) & 15;
            var shift = registers[rs].value & 255;
            var value = registers[rm].value;

            if (rm == 15) value += 4;
            carryout = cpsr.c;

            switch ((code >> 5) & 3)
            {
            case 0: value = Lsl(value, shift < 33 ? shift : 33); break;
            case 1: value = Lsr(value, shift < 33 ? shift : 33); break;
            case 2: value = Asr(value, shift < 32 ? shift : 32); break;
            case 3: if (shift != 0) value = Ror(value, (shift & 31) == 0 ? 32 : shift & 31); break;
            }

            OpAlu(value);
        }

        private void OpMultiply()
        {
            var accumulate = ((code >> 21) & 1) != 0;
            var d = (code >> 16) & 15;
            var n = (code >> 12) & 15;
            var s = (code >> 8) & 15;
            var m = (code >> 0) & 15;

            registers[d].value = Mul(accumulate ? registers[n].value : 0, registers[m].value, registers[s].value);
        }

        private void OpMultiplyLong()
        {
            var signextend = ((code >> 22) & 1) != 0;
            var accumulate = ((code >> 21) & 1) != 0;
            var save = ((code >> 20) & 1) != 0;
            var dhi = (code >> 16) & 15;
            var dlo = (code >> 12) & 15;
            var s = (code >> 8) & 15;
            var m = (code >> 0) & 15;

            ulong rm = registers[m].value;
            ulong rs = registers[s].value;

            if (signextend)
            {
                rm = (rm ^ 0x80000000) - 0x80000000;
                rs = (rs ^ 0x80000000) - 0x80000000;
            }

            var rd = rm * rs;
            if (accumulate) rd += ((ulong)registers[dhi].value << 32) + ((ulong)registers[dlo].value << 0);

            registers[dhi].value = (uint)(rd >> 32);
            registers[dlo].value = (uint)(rd);

            if (save)
            {
                cpsr.n = (registers[dhi].value >> 31);
                cpsr.z = (registers[dhi].value == 0) && (registers[dlo].value == 0) ? 1u : 0u;
            }
        }

        private void OpMoveHalfImm()
        {
            var p = (code >> 24) & 1;
            var u = (code >> 23) & 1;
            var w = (code >> 21) & 1;
            var l = (code >> 20) & 1;
            var n = (code >> 16) & 15;
            var d = (code >> 12) & 15;
            var ih = (code >> 8) & 15;
            var il = (code >> 0) & 15;

            var rn = registers[n].value;
            var nn = (ih << 4) + (il << 0);

            if (p == 1) rn = u != 0 ? rn + nn : rn - nn;
            if (l == 1) registers[d].value = Read(1, rn); // todo: load half
            if (l == 0) Write(1, rn, (half)registers[d].value); // todo: store half
            if (p == 0) rn = u != 0 ? rn + nn : rn - nn;
            if (p == 0 || w == 1 && n != d) registers[n].value = rn;

            if (d == 15) pipeline.refresh = true;
        }

        private void OpMoveHalfReg()
        {
            var m = (code >> 0) & 15;
            var d = (code >> 12) & 15;
            var n = (code >> 16) & 15;
            var l = (code >> 20) & 1;
            var w = (code >> 21) & 1;
            var u = (code >> 23) & 1;
            var p = (code >> 24) & 1;

            var rn = registers[n].value;
            var rm = registers[m].value;

            if (p == 1) rn = u != 0 ? rn + rm : rn - rm;
            if (l == 1) registers[d].value = Read(1, rn); // todo: load half
            if (l == 0) Write(1, rn, (half)registers[d].value); // todo: store half
            if (p == 0) rn = u != 0 ? rn + rm : rn - rm;
            if (p == 0 || w == 1 && n != d) registers[n].value = rn;

            if (d == 15) pipeline.refresh = true;
        }

        private void OpLoadImm()
        {
            var il = (code >> 0) & 15;
            var h = (code >> 5) & 1;
            var ih = (code >> 8) & 15;
            var d = (code >> 12) & 15;
            var n = (code >> 16) & 15;
            var w = (code >> 21) & 1;
            var u = (code >> 23) & 1;
            var p = (code >> 24) & 1;

            var rn = registers[n].value;
            var nn = (ih << 4) + (il << 0);

            if (p == 1) rn = u != 0 ? rn + nn : rn - nn;

            if (h != 0)
            {
                registers[d].value = (uint)(short)Read(1, rn); // todo: load half
            }
            else
            {
                registers[d].value = (uint)(sbyte)Read(0, rn); // todo: load byte
            }

            if (p == 0) rn = u != 0 ? rn + nn : rn - nn;
            if (p == 0 || w == 1 && n != d) registers[n].value = rn;

            if (d == 15) pipeline.refresh = true;
        }

        private void OpLoadReg()
        {
            var m = (code >> 0) & 15;
            var h = (code >> 5) & 1;
            var d = (code >> 12) & 15;
            var n = (code >> 16) & 15;
            var w = (code >> 21) & 1;
            var u = (code >> 23) & 1;
            var p = (code >> 24) & 1;

            var rn = registers[n].value;
            var rm = registers[m].value;

            if (p == 1) rn = u != 0 ? rn + rm : rn - rm;

            if (h != 0)
            {
                registers[d].value = (uint)(short)Read(1, rn); // todo: load half
            }
            else
            {
                registers[d].value = (uint)(sbyte)Read(0, rn); // todo: load byte
            }

            if (p == 0) rn = u != 0 ? rn + rm : rn - rm;
            if (p == 0 || w == 1 && n != d) registers[n].value = rn;

            if (d == 15) pipeline.refresh = true;
        }

        private void OpSwap()
        {
            var rm = (code >> 0) & 15;
            var rd = (code >> 12) & 15;
            var rn = (code >> 16) & 15;
            uint tmp;

            switch ((code >> 22) & 1)
            {
            case 0:
                tmp = LoadWord(registers[rn].value);
                Write(2, registers[rn].value, registers[rm].value);
                registers[rd].value = tmp;
                break;

            case 1:
                tmp = Read(0, registers[rn].value);
                Write(0, registers[rn].value, registers[rm].value);
                registers[rd].value = tmp;
                break;
            }
        }

        private void OpMrsCpsr()
        {
            // MRS rd, cpsr
            registers[(code >> 12) & 15].value = cpsr.Save();
        }

        private void OpMrsSpsr()
        {
            // MRS rd, spsr
            if (spsr == null) return;
            registers[(code >> 12) & 15].value = spsr.Save();
        }

        private void OpMsrCpsrReg()
        {
            // MSR cpsr, rm
            var value = registers[code & 15].value;

            if ((code & (1 << 16)) != 0 && cpsr.m != Mode.USR)
            {
                ChangeRegisters(value & 31);
                cpsr.m = (value >> 0) & 31;
                cpsr.t = (value >> 5) & 1;
                cpsr.f = (value >> 6) & 1;
                cpsr.i = (value >> 7) & 1;
            }

            if ((code & (1 << 19)) != 0)
            {
                cpsr.v = (value >> 28) & 1;
                cpsr.c = (value >> 29) & 1;
                cpsr.z = (value >> 30) & 1;
                cpsr.n = (value >> 31) & 1;
            }
        }

        private void OpMsrSpsrReg()
        {
            // MSR spsr, rm
            if (spsr == null)
            {
                return;
            }

            var flags = spsr.Save();
            var value = registers[code & 0xf].value;

            if ((code & (1 << 16)) != 0) { flags &= 0xffffff00; flags |= value & 0x000000ff; }
            if ((code & (1 << 17)) != 0) { flags &= 0xffff00ff; flags |= value & 0x0000ff00; }
            if ((code & (1 << 18)) != 0) { flags &= 0xff00ffff; flags |= value & 0x00ff0000; }
            if ((code & (1 << 19)) != 0) { flags &= 0x00ffffff; flags |= value & 0xff000000; }

            spsr.Load(flags);
        }

        private void OpMsrCpsrImm()
        {
            // MSR cpsr, #nn
            var value = (code >> 0) & 255;
            var shift = (code >> 7) & 30;

            value = Ror(value, shift);

            if ((code & (1 << 16)) != 0 && cpsr.m != Mode.USR)
            {
                ChangeRegisters(value & 31);
                cpsr.m = (value >> 0) & 31;
                cpsr.t = (value >> 5) & 1;
                cpsr.f = (value >> 6) & 1;
                cpsr.i = (value >> 7) & 1;
            }

            if ((code & (1 << 19)) != 0)
            {
                cpsr.v = (value >> 28) & 1;
                cpsr.c = (value >> 29) & 1;
                cpsr.z = (value >> 30) & 1;
                cpsr.n = (value >> 31) & 1;
            }
        }

        private void OpMsrSpsrImm()
        {
            // MSR spsr, #nn
            if (spsr == null)
            {
                return;
            }

            var flags = spsr.Save();
            var value = (code >> 0) & 255;
            var shift = (code >> 7) & 30;

            value = Ror(value, shift);

            if ((code & (1 << 16)) != 0) { flags &= 0xffffff00; flags |= value & 0x000000ff; }
            if ((code & (1 << 17)) != 0) { flags &= 0xffff00ff; flags |= value & 0x0000ff00; }
            if ((code & (1 << 18)) != 0) { flags &= 0xff00ffff; flags |= value & 0x00ff0000; }
            if ((code & (1 << 19)) != 0) { flags &= 0x00ffffff; flags |= value & 0xff000000; }

            spsr.Load(flags);
        }

        private void OpBx()
        {
            var rm = (code >> 0) & 15;
            cpsr.t = registers[rm].value & 1;

            pc.value = registers[rm].value & ~1u;
            pipeline.refresh = true;
        }

        private void OpStr()
        {
            // STR rd, [rn, immed]
            var n = (code >> 16) & 15;
            var d = (code >> 12) & 15;
            var w = (code >> 21) & 1;
            var b = (code & (1 << 22)) == 0 ? 2 : 0;
            var u = (code >> 23) & 1;
            var p = (code >> 24) & 1;
            var i = (code >> 25) & 1;

            var address = registers[n].value;
            var offset = i == 1 ? BarrelShifter() : (code & 0xfff);
            var data = registers[d].value;
            if (d == 15) data += 4;

            if (u == 0) { offset = 0 - offset; }
            if (p == 1) { address += offset; }

            Write(b, address, data);

            if (p == 0) { address += offset; }
            if (p == 0 || w == 1) { registers[n].value = address; }
        }

        private void OpLdr()
        {
            // LDR rd, [rn, immed]
            var n = (code >> 16) & 15;
            var d = (code >> 12) & 15;
            var w = (code >> 21) & 1;
            var b = (code >> 22) & 1;
            var u = (code >> 23) & 1;
            var p = (code >> 24) & 1;
            var i = (code >> 25) & 1;
            var address = registers[n].value;
            var offset = i == 1
                ? BarrelShifter()
                : (code & 0xfff);

            if (u == 0) { offset = 0 - offset; }
            if (p == 1) { address += offset; }

            registers[d].value = b == 1 ? Read(0, address) : LoadWord(address);

            if (d == 15)
            {
                registers[d].value &= ~3u;
                pipeline.refresh = true;
            }

            if (n != d)
            {
                if (p == 0) { address += offset; }
                if (p == 0 || w == 1) { registers[n].value = address; }
            }
        }

        private void OpB()
        {
            pc.value += MathHelper.SignExtend(code, 24) << 2;
            pipeline.refresh = true;
        }

        private void OpBl()
        {
            lr.value = pipeline.decode.address;
            pc.value += MathHelper.SignExtend(code, 24) << 2;
            pipeline.refresh = true;
        }

        private void OpSwi()
        {
            Isr(Mode.SVC, Vector.SWI);
        }

        private void OpUnd()
        {
            Isr(Mode.UND, Vector.UND);
        }

        private void OpBlockTransfer()
        {
            var n = (code >> 16) & 15;
            var l = (code >> 20) & 1;
            var w = (code >> 21) & 1;
            var m = (code >> 22) & 1;
            var u = (code >> 23) & 1;
            var p = (code >> 24) & 1;

            var bits = (uint)Utility.BitsSet(code & 0xffff);

            var address = u != 0
                ? p != 0
                    ? registers[n].value + 4
                    : registers[n].value
                : p != 0
                    ? registers[n].value - (bits * 4)
                    : registers[n].value - (bits * 4) + 4;

            var storeAddress = u != 0
                ? registers[n].value + bits * 4
                : registers[n].value - bits * 4;

            switch (l)
            {
            case 0: OpStm(n, m != 0, w != 0, cpsr.m, address, storeAddress); break;
            case 1: OpLdm(n, m != 0, w != 0, cpsr.m, address, storeAddress); break;
            }
        }

        private void OpLdm(uint rn, bool m, bool w, uint currentMode, uint address, uint storeAddress)
        {
            if (m)
            {
                ChangeRegisters(Mode.USR);
            }

            for (int i = 0; i < 16; i++)
            {
                if ((code & (1 << i)) != 0)
                {
                    if (w)
                    {
                        registers[rn].value = storeAddress;
                    }

                    registers[i].value = LoadWord(address);
                    address += 4;
                }
            }

            if (m)
            {
                ChangeRegisters(currentMode);
            }

            if ((code & (1 << 15)) != 0)
            {
                if (m && spsr != null)
                {
                    ChangeMode(spsr.m);
                    cpsr.Load(spsr.Save());
                }

                pipeline.refresh = true;
            }
        }

        private void OpStm(uint rn, bool m, bool w, uint currentMode, uint address, uint storeAddress)
        {
            if (m)
            {
                ChangeRegisters(Mode.USR);
            }

            for (int i = 0; i < 16; i++)
            {
                if ((code & (1 << i)) != 0)
                {
                    Write(2, address, registers[i].value);
                    address += 4;

                    if (w)
                    {
                        registers[rn].value = storeAddress;
                    }
                }
            }

            if (m)
            {
                ChangeRegisters(currentMode);
            }
        }

        #endregion

        #endregion

        #region THUMB

        private static uint ThumbEncode(uint code)
        {
            return (code >> 8) & 0xff;
        }

        private void ThumbExecute()
        {
            if (pipeline.refresh)
            {
                pipeline.refresh = false;
                pipeline.fetch.address = pc.value & ~1u;
                pipeline.fetch.data = Read(1, pipeline.fetch.address);

                ThumbStep();
            }

            ThumbStep();

            if (interrupt && cpsr.i == 0) // irq after pipeline initialized in correct mode
            {
                Isr(Mode.IRQ, Vector.IRQ);
                lr.value += 2U;
                return;
            }

            code = pipeline.execute.data;

            thumbCodes[ThumbEncode(code)]();
        }

        private void ThumbInitialize()
        {
            thumbCodes = new Action[]
            {
                ThumbOpShift,   ThumbOpShift,   ThumbOpShift,   ThumbOpShift,   ThumbOpShift,   ThumbOpShift,   ThumbOpShift,    ThumbOpShift,
                ThumbOpShift,   ThumbOpShift,   ThumbOpShift,   ThumbOpShift,   ThumbOpShift,   ThumbOpShift,   ThumbOpShift,    ThumbOpShift,
                ThumbOpShift,   ThumbOpShift,   ThumbOpShift,   ThumbOpShift,   ThumbOpShift,   ThumbOpShift,   ThumbOpShift,    ThumbOpShift,
                ThumbOpAdjust,  ThumbOpAdjust,  ThumbOpAdjust,  ThumbOpAdjust,  ThumbOpAdjust,  ThumbOpAdjust,  ThumbOpAdjust,   ThumbOpAdjust,
                ThumbOpMovImm,  ThumbOpMovImm,  ThumbOpMovImm,  ThumbOpMovImm,  ThumbOpMovImm,  ThumbOpMovImm,  ThumbOpMovImm,   ThumbOpMovImm,
                ThumbOpCmpImm,  ThumbOpCmpImm,  ThumbOpCmpImm,  ThumbOpCmpImm,  ThumbOpCmpImm,  ThumbOpCmpImm,  ThumbOpCmpImm,   ThumbOpCmpImm,
                ThumbOpAddImm,  ThumbOpAddImm,  ThumbOpAddImm,  ThumbOpAddImm,  ThumbOpAddImm,  ThumbOpAddImm,  ThumbOpAddImm,   ThumbOpAddImm,
                ThumbOpSubImm,  ThumbOpSubImm,  ThumbOpSubImm,  ThumbOpSubImm,  ThumbOpSubImm,  ThumbOpSubImm,  ThumbOpSubImm,   ThumbOpSubImm,
                ThumbOpAlu,     ThumbOpAlu,     ThumbOpAlu,     ThumbOpAlu,     ThumbOpAddHi,   ThumbOpCmpHi,   ThumbOpMovHi,    ThumbOpBx,
                ThumbOpLdrPc,   ThumbOpLdrPc,   ThumbOpLdrPc,   ThumbOpLdrPc,   ThumbOpLdrPc,   ThumbOpLdrPc,   ThumbOpLdrPc,    ThumbOpLdrPc,
                ThumbOpStrReg,  ThumbOpStrReg,  ThumbOpStrhReg, ThumbOpStrhReg, ThumbOpStrbReg, ThumbOpStrbReg, ThumbOpLdrsbReg, ThumbOpLdrsbReg,
                ThumbOpLdrReg,  ThumbOpLdrReg,  ThumbOpLdrhReg, ThumbOpLdrhReg, ThumbOpLdrbReg, ThumbOpLdrbReg, ThumbOpLdrshReg, ThumbOpLdrshReg,
                ThumbOpStrImm,  ThumbOpStrImm,  ThumbOpStrImm,  ThumbOpStrImm,  ThumbOpStrImm,  ThumbOpStrImm,  ThumbOpStrImm,   ThumbOpStrImm,
                ThumbOpLdrImm,  ThumbOpLdrImm,  ThumbOpLdrImm,  ThumbOpLdrImm,  ThumbOpLdrImm,  ThumbOpLdrImm,  ThumbOpLdrImm,   ThumbOpLdrImm,
                ThumbOpStrbImm, ThumbOpStrbImm, ThumbOpStrbImm, ThumbOpStrbImm, ThumbOpStrbImm, ThumbOpStrbImm, ThumbOpStrbImm,  ThumbOpStrbImm,
                ThumbOpLdrbImm, ThumbOpLdrbImm, ThumbOpLdrbImm, ThumbOpLdrbImm, ThumbOpLdrbImm, ThumbOpLdrbImm, ThumbOpLdrbImm,  ThumbOpLdrbImm,
                ThumbOpStrhImm, ThumbOpStrhImm, ThumbOpStrhImm, ThumbOpStrhImm, ThumbOpStrhImm, ThumbOpStrhImm, ThumbOpStrhImm,  ThumbOpStrhImm,
                ThumbOpLdrhImm, ThumbOpLdrhImm, ThumbOpLdrhImm, ThumbOpLdrhImm, ThumbOpLdrhImm, ThumbOpLdrhImm, ThumbOpLdrhImm,  ThumbOpLdrhImm,
                ThumbOpStrSp,   ThumbOpStrSp,   ThumbOpStrSp,   ThumbOpStrSp,   ThumbOpStrSp,   ThumbOpStrSp,   ThumbOpStrSp,    ThumbOpStrSp,
                ThumbOpLdrSp,   ThumbOpLdrSp,   ThumbOpLdrSp,   ThumbOpLdrSp,   ThumbOpLdrSp,   ThumbOpLdrSp,   ThumbOpLdrSp,    ThumbOpLdrSp,
                ThumbOpAddPc,   ThumbOpAddPc,   ThumbOpAddPc,   ThumbOpAddPc,   ThumbOpAddPc,   ThumbOpAddPc,   ThumbOpAddPc,    ThumbOpAddPc,
                ThumbOpAddSp,   ThumbOpAddSp,   ThumbOpAddSp,   ThumbOpAddSp,   ThumbOpAddSp,   ThumbOpAddSp,   ThumbOpAddSp,    ThumbOpAddSp,
                ThumbOpSubSp,   ThumbOpUnd,     ThumbOpUnd,     ThumbOpUnd,     ThumbOpPush,    ThumbOpPush,    ThumbOpUnd,      ThumbOpUnd,
                ThumbOpUnd,     ThumbOpUnd,     ThumbOpUnd,     ThumbOpUnd,     ThumbOpPop,     ThumbOpPop,     ThumbOpUnd,      ThumbOpUnd,
                ThumbOpStmia,   ThumbOpStmia,   ThumbOpStmia,   ThumbOpStmia,   ThumbOpStmia,   ThumbOpStmia,   ThumbOpStmia,    ThumbOpStmia,
                ThumbOpLdmia,   ThumbOpLdmia,   ThumbOpLdmia,   ThumbOpLdmia,   ThumbOpLdmia,   ThumbOpLdmia,   ThumbOpLdmia,    ThumbOpLdmia,
                ThumbOpBCond,   ThumbOpBCond,   ThumbOpBCond,   ThumbOpBCond,   ThumbOpBCond,   ThumbOpBCond,   ThumbOpBCond,    ThumbOpBCond,
                ThumbOpBCond,   ThumbOpBCond,   ThumbOpBCond,   ThumbOpBCond,   ThumbOpBCond,   ThumbOpBCond,   ThumbOpUnd,      ThumbOpSwi,
                ThumbOpB,       ThumbOpB,       ThumbOpB,       ThumbOpB,       ThumbOpB,       ThumbOpB,       ThumbOpB,        ThumbOpB,
                ThumbOpUnd,     ThumbOpUnd,     ThumbOpUnd,     ThumbOpUnd,     ThumbOpUnd,     ThumbOpUnd,     ThumbOpUnd,      ThumbOpUnd,
                ThumbOpBl1,     ThumbOpBl1,     ThumbOpBl1,     ThumbOpBl1,     ThumbOpBl1,     ThumbOpBl1,     ThumbOpBl1,      ThumbOpBl1,
                ThumbOpBl2,     ThumbOpBl2,     ThumbOpBl2,     ThumbOpBl2,     ThumbOpBl2,     ThumbOpBl2,     ThumbOpBl2,      ThumbOpBl2
            };
        }

        private void ThumbMap(string pattern, Action code)
        {
            var mask = ThumbEncode(BitString.Mask(pattern));
            var zero = ThumbEncode(BitString.Min(pattern));
            var full = ThumbEncode(BitString.Max(pattern));

            for (var i = zero; i <= full; i++)
            {
                if ((i & mask) == zero)
                {
                    thumbCodes[i] = code;
                }
            }
        }

        private void ThumbStep()
        {
            pc.value += 2;

            pipeline.execute = pipeline.decode;
            pipeline.decode = pipeline.fetch;
            pipeline.fetch.address = pc.value & ~1u;
            pipeline.fetch.data = Read(1, pipeline.fetch.address);
        }

        #region Opcodes

        private void ThumbOpShift()
        {
            // lsl rd, rm, #nn
            var rd = (code >> 0) & 0x07;
            var rm = (code >> 3) & 0x07;
            var nn = (code >> 6) & 0x1f;

            switch ((code >> 11) & 0x03)
            {
            case 0x00: registers[rd].value = Mov(Lsl(registers[rm].value, nn)); break;
            case 0x01: registers[rd].value = Mov(Lsr(registers[rm].value, nn == 0U ? 32U : nn)); break;
            case 0x02: registers[rd].value = Mov(Asr(registers[rm].value, nn == 0U ? 32U : nn)); break;
            }
        }

        private void ThumbOpAdjust()
        {
            // add rd, rn, rm
            var rd = (code >> 0) & 0x07;
            var rn = (code >> 3) & 0x07;
            var rm = (code >> 6) & 0x07;

            switch ((code >> 9) & 0x03)
            {
            case 0x00: registers[rd].value = Add(registers[rn].value, registers[rm].value); break;
            case 0x01: registers[rd].value = Sub(registers[rn].value, registers[rm].value); break;
            case 0x02: registers[rd].value = Add(registers[rn].value, rm); break;
            case 0x03: registers[rd].value = Sub(registers[rn].value, rm); break;
            }
        }

        private void ThumbOpMovImm()
        {
            // mov rd, #nn
            var rd = (code >> 8) & 0x07;
            var nn = (code >> 0) & 0xff;
            registers[rd].value = Mov(nn);
        }

        private void ThumbOpCmpImm()
        {
            // cmp rn, #nn
            var rd = (code >> 8) & 0x07;
            var nn = (code >> 0) & 0xff;
            Sub(registers[rd].value, nn);
        }

        private void ThumbOpAddImm()
        {
            // add rd, #nn
            var rd = (code >> 8) & 0x07;
            var nn = (code >> 0) & 0xff;
            registers[rd].value = Add(registers[rd].value, nn);
        }

        private void ThumbOpSubImm()
        {
            // sub rd, #nn
            var rd = (code >> 8) & 0x07;
            var nn = (code >> 0) & 0xff;
            registers[rd].value = Sub(registers[rd].value, nn);
        }

        private void ThumbOpAlu()
        {
            var rd = (code >> 0) & 7;
            var rn = (code >> 3) & 7;

            switch ((code >> 6) & 15)
            {
            case 0x0: registers[rd].value = Mov(registers[rd].value & registers[rn].value); break;
            case 0x1: registers[rd].value = Mov(registers[rd].value ^ registers[rn].value); break;
            case 0x2: registers[rd].value = Mov(Lsl(registers[rd].value, registers[rn].value & 0xff)); break;
            case 0x3: registers[rd].value = Mov(Lsr(registers[rd].value, registers[rn].value & 0xff)); break;
            case 0x4: registers[rd].value = Mov(Asr(registers[rd].value, registers[rn].value & 0xff)); break;
            case 0x5: registers[rd].value = Add(registers[rd].value, registers[rn].value, cpsr.c); break;
            case 0x6: registers[rd].value = Sub(registers[rd].value, registers[rn].value, cpsr.c); break;
            case 0x7: registers[rd].value = Mov(Ror(registers[rd].value, registers[rn].value & 0xff)); break;
            case 0x8: Mov(registers[rd].value & registers[rn].value); break;
            case 0x9: registers[rd].value = Sub(0U, registers[rn].value); break;
            case 0xa: Sub(registers[rd].value, registers[rn].value); break;
            case 0xb: Add(registers[rd].value, registers[rn].value); break;
            case 0xc: registers[rd].value = Mov(registers[rd].value | registers[rn].value); break;
            case 0xd: registers[rd].value = Mul(0U, registers[rd].value, registers[rn].value); break;
            case 0xe: registers[rd].value = Mov(registers[rd].value & ~registers[rn].value); break;
            case 0xf: registers[rd].value = Mov(~registers[rn].value); break;
            }
        }

        private void ThumbOpAddHi()
        {
            var rd = ((code & (1 << 7)) >> 4) | (code & 0x7);
            var rm = (code >> 3) & 0xF;

            registers[rd].value += registers[rm].value;

            if (rd == 15)
            {
                registers[rd].value &= ~1U;
                pipeline.refresh = true;
            }
        }

        private void ThumbOpCmpHi()
        {
            var rd = ((code & (1 << 7)) >> 4) | (code & 0x7);
            var rm = (code >> 3) & 0xF;

            var alu = registers[rd].value - registers[rm].value;

            cpsr.n = alu >> 31;
            cpsr.z = alu == 0 ? 1U : 0U;
            OverflowCarrySub(registers[rd].value, registers[rm].value, alu);
        }

        private void ThumbOpMovHi()
        {
            var rd = ((code & (1 << 7)) >> 4) | (code & 0x7);
            var rm = (code >> 3) & 0xF;

            registers[rd].value = registers[rm].value;

            if (rd == 15)
            {
                registers[rd].value &= ~1U;
                pipeline.refresh = true;
            }
        }

        private void ThumbOpBx()
        {
            var rm = (code >> 3) & 15U;
            cpsr.t = registers[rm].value & 1U;

            pc.value = registers[rm].value & ~1U;
            pipeline.refresh = true;
        }

        private void ThumbOpLdrPc()
        {
            var rd = (code >> 8) & 0x7;

            registers[rd].value = LoadWord((pc.value & ~2u) + (code & 0xFF) * 4);

            Cycles++;
        }

        private void ThumbOpStrReg()
        {
            Write(2, registers[(code >> 3) & 0x7].value + registers[(code >> 6) & 0x7].value, registers[code & 0x7].value);
        }

        private void ThumbOpStrhReg()
        {
            Write(1, registers[(code >> 3) & 0x7].value + registers[(code >> 6) & 0x7].value, (ushort)(registers[code & 0x7].value & 0xFFFF));
        }

        private void ThumbOpStrbReg()
        {
            Write(0, registers[(code >> 3) & 0x7].value + registers[(code >> 6) & 0x7].value, (byte)(registers[code & 0x7].value & 0xFF));
        }

        private void ThumbOpLdrsbReg()
        {
            registers[code & 0x7].value = Read(0, registers[(code >> 3) & 0x7].value + registers[(code >> 6) & 0x7].value);

            if ((registers[code & 0x7].value & (1 << 7)) != 0)
            {
                registers[code & 0x7].value |= 0xFFFFFF00;
            }

            Cycles++;
        }

        private void ThumbOpLdrReg()
        {
            registers[code & 0x7].value = LoadWord(registers[(code >> 3) & 0x7].value + registers[(code >> 6) & 0x7].value);

            Cycles++;
        }

        private void ThumbOpLdrhReg()
        {
            registers[code & 0x7].value = Read(1, registers[(code >> 3) & 0x7].value + registers[(code >> 6) & 0x7].value);

            Cycles++;
        }

        private void ThumbOpLdrbReg()
        {
            registers[code & 0x7].value = Read(0, registers[(code >> 3) & 0x7].value + registers[(code >> 6) & 0x7].value);

            Cycles++;
        }

        private void ThumbOpLdrshReg()
        {
            registers[code & 0x7].value = Read(1, registers[(code >> 3) & 0x7].value + registers[(code >> 6) & 0x7].value);

            if ((registers[code & 0x7].value & (1 << 15)) != 0)
            {
                registers[code & 0x7].value |= 0xFFFF0000;
            }

            Cycles++;
        }

        private void ThumbOpStrImm()
        {
            Write(2, registers[(code >> 3) & 0x7].value + ((code >> 6) & 0x1F) * 4, registers[code & 0x7].value);
        }

        private void ThumbOpLdrImm()
        {
            registers[code & 0x7].value = LoadWord(registers[(code >> 3) & 0x7].value + ((code >> 6) & 0x1F) * 4);

            Cycles++;
        }

        private void ThumbOpStrbImm()
        {
            Write(0, registers[(code >> 3) & 0x7].value + ((code >> 6) & 0x1F), (byte)(registers[code & 0x7].value & 0xFF));
        }

        private void ThumbOpLdrbImm()
        {
            registers[code & 0x7].value = Read(0, registers[(code >> 3) & 0x7].value + ((code >> 6) & 0x1F));

            Cycles++;
        }

        private void ThumbOpStrhImm()
        {
            Write(1, registers[(code >> 3) & 7].value + ((code >> 6) & 0x1F) * 2, (ushort)(registers[code & 0x7].value & 0xFFFF));
        }

        private void ThumbOpLdrhImm()
        {
            registers[code & 7].value = Read(1, registers[(code >> 3) & 7].value + ((code >> 6) & 0x1f) * 2);

            Cycles++;
        }

        private void ThumbOpStrSp()
        {
            Write(2, sp.value + ((code << 2) & 0x3fc), registers[(code >> 8) & 7].value);
        }

        private void ThumbOpLdrSp()
        {
            registers[(code >> 8) & 7].value = LoadWord(sp.value + ((code << 2) & 0x3fc));
        }

        private void ThumbOpAddPc()
        {
            registers[(code >> 8) & 7].value = (pc.value & ~2u) + ((code << 2) & 0x3fc);
        }

        private void ThumbOpAddSp()
        {
            registers[(code >> 8) & 7].value = (sp.value & ~0u) + ((code << 2) & 0x3fc);
        }

        private void ThumbOpSubSp()
        {
            if ((code & (1u << 7)) != 0)
                sp.value -= (code << 2) & 0x1fc;
            else
                sp.value += (code << 2) & 0x1fc;
        }

        private void ThumbOpPush()
        {
            if ((code & 0x100) != 0)
            {
                sp.value -= 4u;
                Write(2, sp.value, lr.value);
            }

            for (var i = 7; i >= 0; i--)
            {
                if (((code >> i) & 1U) != 0)
                {
                    sp.value -= 4U;
                    Write(2, sp.value, registers[i].value);
                }
            }
        }

        private void ThumbOpPop()
        {
            for (var i = 0; i < 8; i++)
            {
                if (((code >> i) & 1) != 0)
                {
                    registers[i].value = LoadWord(sp.value);
                    sp.value += 4u;
                }
            }

            if ((code & 0x100) != 0)
            {
                pc.value = LoadWord(sp.value) & ~1u;
                sp.value += 4u;
                pipeline.refresh = true;
            }

            Cycles++;
        }

        private void ThumbOpStmia()
        {
            var rn = (code >> 8) & 0x07;

            for (var i = 0; i < 8; i++)
            {
                if (((code >> i) & 1) != 0)
                {
                    Write(2, registers[rn].value & ~3u, registers[i].value);
                    registers[rn].value += 4;
                }
            }
        }

        private void ThumbOpLdmia()
        {
            var rn = (code >> 8) & 0x07;

            var address = registers[rn].value;

            for (var i = 0; i < 8; i++)
            {
                if (((code >> i) & 1) != 0)
                {
                    registers[i].value = Read(2, address & ~3u);
                    address += 4;
                }
            }

            registers[rn].value = address;
        }

        private void ThumbOpBCond()
        {
            if (GetCondition(code >> 8))
            {
                pc.value += MathHelper.SignExtend(code, 8) << 1;
                pipeline.refresh = true;
            }
        }

        private void ThumbOpSwi()
        {
            Isr(Mode.SVC, Vector.SWI);
        }

        private void ThumbOpB()
        {
            pc.value += MathHelper.SignExtend(code, 11) << 1;
            pipeline.refresh = true;
        }

        private void ThumbOpBl1()
        {
            lr.value = pc.value + (MathHelper.SignExtend(code, 11) << 12);
        }

        private void ThumbOpBl2()
        {
            pc.value = lr.value + ((code & 0x7ff) << 1);
            lr.value = pipeline.decode.address | 1;
            pipeline.refresh = true;
        }

        private void ThumbOpUnd()
        {
            Isr(Mode.UND, Vector.UND);
        }

        #endregion

        #endregion

        protected abstract void Dispatch();

        protected abstract uint Read(int size, uint address);

        protected abstract void Write(int size, uint address, uint data);

        public virtual void Initialize()
        {
            Armv4Initialize();
            ThumbInitialize();
        }

        public override void Update()
        {
            if (halt)
            {
                Cycles += 1;
            }
            else
            {
                if (cpsr.t == 0) { Armv4Execute(); }
                if (cpsr.t == 1) { ThumbExecute(); }
            }

            Dispatch();
            Cycles = 0;
        }

        public uint GetProgramCursor()
        {
            return pc.value;
        }

        private class Flags
        {
            public uint n, z, c, v;
            public uint r;
            public uint i, f, t, m;

            public void Load(uint value)
            {
                n = (value >> 31) & 1;
                z = (value >> 30) & 1;
                c = (value >> 29) & 1;
                v = (value >> 28) & 1;
                r = (value >> 8) & 0xfffff;
                i = (value >> 7) & 1;
                f = (value >> 6) & 1;
                t = (value >> 5) & 1;
                m = (value >> 0) & 31;
            }

            public uint Save()
            {
                return
                    (n << 31) | (z << 30) | (c << 29) | (v << 28) |
                    (r << 8) |
                    (i << 7) | (f << 6) | (t << 5) | (m << 0);
            }
        }

        private class Pipeline
        {
            public Stage execute;
            public Stage decode;
            public Stage fetch;
            public bool refresh;

            public struct Stage
            {
                public uint address;
                public uint data;
            }
        }

        private class Register
        {
            public uint value;
        }

        private static class Mode
        {
            public const uint USR = 0x10;
            public const uint FIQ = 0x11;
            public const uint IRQ = 0x12;
            public const uint SVC = 0x13;
            public const uint ABT = 0x17;
            public const uint UND = 0x1b;
            public const uint SYS = 0x1f;
        }

        private static class Vector
        {
            public const uint RST = 0x00;
            public const uint UND = 0x04;
            public const uint SWI = 0x08;
            public const uint PAB = 0x0c;
            public const uint DAB = 0x10;
            public const uint RES = 0x14;
            public const uint IRQ = 0x18;
            public const uint FIQ = 0x1c;
        }
    }
}
