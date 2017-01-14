using System;
using half = System.UInt16;

namespace Beta.Platform.Processors.ARM7
{
    public partial class Core
    {
        private static uint Armv4Encode(uint code)
        {
            return ((code >> 16) & 0xff0) | ((code >> 4) & 0x00f);
        }

        private void Armv4Execute()
        {
            if (pipeline.refresh)
            {
                pipeline.refresh = false;
                pipeline.fetch = Read(2, pc.value & ~3U);

                Armv4Step();
            }

            Armv4Step();

            if (interrupt && cpsr.i == 0) // irq after pipeline initialized in correct mode
            {
                Isr(Mode.IRQ, Vector.IRQ);
                return;
            }

            code = pipeline.execute;

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
            var test = Armv4Encode(BitString.Test(pattern));

            for (var i = 0; i <= armv4Codes.Length; i++)
            {
                if ((i & mask) == test)
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
            pipeline.fetch = Read(2, pc.value & ~3U);
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
            lr.value = pc.value - 4;
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
    }
}
