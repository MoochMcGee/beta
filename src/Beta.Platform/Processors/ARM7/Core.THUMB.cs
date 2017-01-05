using System;

namespace Beta.Platform.Processors.ARM7
{
    public partial class Core
    {
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

            cycles++;
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

            cycles++;
        }

        private void ThumbOpLdrReg()
        {
            registers[code & 0x7].value = LoadWord(registers[(code >> 3) & 0x7].value + registers[(code >> 6) & 0x7].value);

            cycles++;
        }

        private void ThumbOpLdrhReg()
        {
            registers[code & 0x7].value = Read(1, registers[(code >> 3) & 0x7].value + registers[(code >> 6) & 0x7].value);

            cycles++;
        }

        private void ThumbOpLdrbReg()
        {
            registers[code & 0x7].value = Read(0, registers[(code >> 3) & 0x7].value + registers[(code >> 6) & 0x7].value);

            cycles++;
        }

        private void ThumbOpLdrshReg()
        {
            registers[code & 0x7].value = Read(1, registers[(code >> 3) & 0x7].value + registers[(code >> 6) & 0x7].value);

            if ((registers[code & 0x7].value & (1 << 15)) != 0)
            {
                registers[code & 0x7].value |= 0xFFFF0000;
            }

            cycles++;
        }

        private void ThumbOpStrImm()
        {
            Write(2, registers[(code >> 3) & 0x7].value + ((code >> 6) & 0x1F) * 4, registers[code & 0x7].value);
        }

        private void ThumbOpLdrImm()
        {
            registers[code & 0x7].value = LoadWord(registers[(code >> 3) & 0x7].value + ((code >> 6) & 0x1F) * 4);

            cycles++;
        }

        private void ThumbOpStrbImm()
        {
            Write(0, registers[(code >> 3) & 0x7].value + ((code >> 6) & 0x1F), (byte)(registers[code & 0x7].value & 0xFF));
        }

        private void ThumbOpLdrbImm()
        {
            registers[code & 0x7].value = Read(0, registers[(code >> 3) & 0x7].value + ((code >> 6) & 0x1F));

            cycles++;
        }

        private void ThumbOpStrhImm()
        {
            Write(1, registers[(code >> 3) & 7].value + ((code >> 6) & 0x1F) * 2, (ushort)(registers[code & 0x7].value & 0xFFFF));
        }

        private void ThumbOpLdrhImm()
        {
            registers[code & 7].value = Read(1, registers[(code >> 3) & 7].value + ((code >> 6) & 0x1f) * 2);

            cycles++;
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

            cycles++;
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
    }
}
