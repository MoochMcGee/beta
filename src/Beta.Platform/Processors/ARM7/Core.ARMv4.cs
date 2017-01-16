using Beta.Platform.Exceptions;
using System;
using half = System.UInt16;

namespace Beta.Platform.Processors.ARM7
{
    public partial class Core
    {
        private static uint ARMv4Decode(uint code)
        {
            return ((code >> 16) & 0xff0) | ((code >> 4) & 0x00f);
        }

        private void ARMv4Execute()
        {
            if (pipeline.refresh)
            {
                pipeline.refresh = false;
                pipeline.fetch = Read(2, pc.value & ~3U);

                ARMv4Step();
            }

            ARMv4Step();

            if (interrupt && cpsr.i == 0) // irq after pipeline initialized in correct mode
            {
                Isr(Mode.IRQ, Vector.IRQ);
                return;
            }

            code = pipeline.execute;

            if (GetCondition(code >> 28))
            {
                armv4Codes[ARMv4Decode(code)]();
            }
        }

        private void ARMv4Initialize()
        {
            armv4Codes = new Action[ARMv4Decode(~0U) + 1];

            ARMv4Map("---- ---- ---- ---- ---- ---- ---- ----", OpUND);
            ARMv4Map("---- 000- ---- ---- ---- ---- ---0 ----", OpDataProcessingConstantShift);
            ARMv4Map("---- 000- ---- ---- ---- ---- 0--1 ----", OpDataProcessingRegisterShift);
            ARMv4Map("---- 001- ---- ---- ---- ---- ---- ----", OpDataProcessingConstant);
            ARMv4Map("---- 010- ---- ---- ---- ---- ---- ----", OpDataTransferConstantOffset);
            ARMv4Map("---- 011- ---- ---- ---- ---- ---0 ----", OpDataTransferRegisterOffset);
            ARMv4Map("---- 100- ---- ---- ---- ---- ---- ----", OpDataTransferMultiple);
            ARMv4Map("---- 101- ---- ---- ---- ---- ---- ----", OpBranch);
            ARMv4Map("---- 1111 ---- ---- ---- ---- ---- ----", OpSWI);

            ARMv4Map("---- 0000 -00- ---- ---- 0000 1--1 ----", OpDataTransfer2RegisterOffset);
            ARMv4Map("---- 0001 -0-- ---- ---- 0000 1--1 ----", OpDataTransfer2RegisterOffset);
            ARMv4Map("---- 0000 -10- ---- ---- ---- 1--1 ----", OpDataTransfer2ConstantOffset);
            ARMv4Map("---- 0001 -1-- ---- ---- ---- 1--1 ----", OpDataTransfer2ConstantOffset);

            ARMv4Map("---- 0000 00-- ---- ---- ---- 1001 ----", OpMultiply32);
            ARMv4Map("---- 0000 1--- ---- ---- ---- 1001 ----", OpMultiply64);
            ARMv4Map("---- 0001 0-00 ---- ---- 0000 1001 ----", OpSwap);
            ARMv4Map("---- 0001 0010 ---- ---- ---- 0001 ----", OpBranchExchange);

            ARMv4Map("---- 0001 0-00 ---- ---- ---- 0000 ----", OpMoveStatusToRegister);
            ARMv4Map("---- 0001 0-10 ---- ---- ---- 0000 ----", OpMoveRegisterToStatus);
            ARMv4Map("---- 0011 0-10 ---- ---- ---- 0000 ----", OpMoveConstantToStatus);
        }

        private void ARMv4Map(string pattern, Action code)
        {
            var mask = ARMv4Decode(BitString.Mask(pattern));
            var test = ARMv4Decode(BitString.Test(pattern));

            for (var i = 0; i < armv4Codes.Length; i++)
            {
                if ((i & mask) == test)
                {
                    armv4Codes[i] = code;
                }
            }
        }

        private void ARMv4Step()
        {
            pc.value += 4U;

            pipeline.execute = pipeline.decode;
            pipeline.decode = pipeline.fetch;
            pipeline.fetch = Read(2, pc.value & ~3U);
        }

        #region Opcodes

        private void OpBranch()
        {
            var link = (code >> 24) & 1;
            if (link == 1)
            {
                Set(14, pc.value - 4);
                Set(15, pc.value + (MathHelper.SignExtend(code, 24) << 2));
            }
            else
            {
                Set(15, pc.value + (MathHelper.SignExtend(code, 24) << 2));
            }
        }

        private void OpBranchExchange()
        {
            var rm = Get(code & 15);
            cpsr.t = rm & 1;

            Set(15, rm & ~1U);
        }

        private void OpDataProcessingConstant()
        {
            var shift = (code >> 7) & 30;
            var value = (code >> 0) & 255;

            OpDataProcessing(ROR(value, shift));
        }

        private void OpDataProcessingConstantShift()
        {
            var value = GetOperand2ConstantShift();

            OpDataProcessing(value);
        }

        private void OpDataProcessingRegisterShift()
        {
            var value = GetOperand2RegisterShift();

            OpDataProcessing(value);
        }

        private void OpDataProcessing(uint value)
        {
            var o = (code >> 21) & 15;
            var n = (code >> 16) & 15;
            var d = (code >> 12) & 15;

            uint rn = Get(n);

            switch (o)
            {
            case 0x0: Set(d, Mov(rn & value)); break; // AND
            case 0x1: Set(d, Mov(rn ^ value)); break; // EOR
            case 0x2: Set(d, Sub(rn, value)); break; // SUB
            case 0x3: Set(d, Sub(value, rn)); break; // RSB
            case 0x4: Set(d, Add(rn, value)); break; // ADD
            case 0x5: Set(d, Add(rn, value, cpsr.c)); break; // ADC
            case 0x6: Set(d, Sub(rn, value, cpsr.c)); break; // SBC
            case 0x7: Set(d, Sub(value, rn, cpsr.c)); break; // RSC
            case 0x8: Mov(rn & value); break; // TST
            case 0x9: Mov(rn ^ value); break; // TEQ
            case 0xa: Sub(rn, value); break; // CMP
            case 0xb: Add(rn, value); break; // CMN
            case 0xc: Set(d, Mov(rn | value)); break; // ORR
            case 0xd: Set(d, Mov(value)); break; // MOV
            case 0xe: Set(d, Mov(rn & ~value)); break; // BIC
            case 0xf: Set(d, Mov(~value)); break; // MVN
            }

            var s = (code >> 20) & 1;
            if (s == 1 && d == 15 && spsr != null)
            {
                MoveSPSRToCPSR();
            }
        }

        private void OpDataTransferConstantOffset()
        {
            var offset = code & 0xfff;

            OpDataTransfer(offset);
        }

        private void OpDataTransferRegisterOffset()
        {
            var offset = GetOperand2ConstantShift();

            OpDataTransfer(offset);
        }

        private void OpDataTransfer(uint offset)
        {
            var p = (code >> 24) & 1;
            var u = (code >> 23) & 1;
            var b = (code >> 22) & 1;
            var w = (code >> 21) & 1;
            var l = (code >> 20) & 1;
            var n = (code >> 16) & 15;
            var d = (code >> 12) & 15;

            if (u == 0) offset = ((uint)-offset);

            var rn = Get(n);

            var address = p == 1
                ? rn + offset
                : rn;

            if (l == 1)
            {
                var rd = b == 1 ? ReadByte(address) : ReadWord(address);

                if (p == 0 || w == 1)
                {
                    Set(n, rn + offset);
                    Set(d, rd);
                }
                else
                {
                    Set(d, rd);
                }
            }
            else
            {
                var rd = Get(d, 4);

                Write(b == 1 ? 0 : 2, address, rd);

                if (p == 0 || w == 1)
                {
                    Set(n, rn + offset);
                }
            }
        }

        private void OpDataTransfer2ConstantOffset()
        {
            var upper = (code >> 8) & 15;
            var lower = (code >> 0) & 15;

            OpDataTransfer2((upper << 4) + lower);
        }

        private void OpDataTransfer2RegisterOffset()
        {
            var offset = Get(code & 15);

            OpDataTransfer2(offset);
        }

        private void OpDataTransfer2(uint offset)
        {
            var p = (code >> 24) & 1;
            var u = (code >> 23) & 1;
            var w = (code >> 21) & 1;
            var l = (code >> 20) & 1;
            var n = (code >> 16) & 15;
            var d = (code >> 12) & 15;

            if (u == 0) offset = ((uint)-offset);

            var rn = Get(n);

            var address = p == 1
                ? rn + offset
                : rn;

            if (l == 1)
            {
                uint rd = 0;

                switch ((code >> 5) & 3)
                {
                case 0: OpUND(); break; // Reserved
                case 1: rd = ReadHalf(address); break; // LDRH
                case 2: rd = ReadByteSignExtended(address); break; // LDRSB
                case 3: rd = ReadHalfSignExtended(address); break; // LDRSH
                }

                if (p == 0 || w == 1)
                {
                    Set(n, rn + offset);
                    Set(d, rd);
                }
                else
                {
                    Set(d, rd);
                }
            }
            else
            {
                var rd = Get(d, 4);

                switch ((code >> 5) & 3)
                {
                case 0: OpUND(); break; // Reserved
                case 1: Write(1, address, (half)rd); break; // STRH - TODO: store half
                case 2: OpUND(); break; // Reserved
                case 3: OpUND(); break; // Reserved
                }

                if (p == 0 || w == 1)
                {
                    Set(n, rn + offset);
                }
            }
        }

        private void OpDataTransferMultiple()
        {
            var p = (code >> 24) & 1;
            var u = (code >> 23) & 1;
            var s = (code >> 22) & 1;
            var w = (code >> 21) & 1;
            var l = (code >> 20) & 1;
            var n = (code >> 16) & 15;

            var bits = (uint)Utility.BitsSet(code & 0xffff);

            var rn = Get(n);

            var address = u == 1
                ? p == 1
                    ? rn + 4
                    : rn + 0
                : p == 1
                    ? rn + 0 - (bits * 4)
                    : rn + 4 - (bits * 4);

            var last = u == 1
                ? rn + (bits * 4)
                : rn - (bits * 4);

            switch (l)
            {
            case 0: OpStm(n, address, last, w != 0, s != 0, cpsr.m); break;
            case 1: OpLdm(n, address, last, w != 0, s != 0, cpsr.m); break;
            }
        }

        private void OpLdm(uint n, uint address, uint last, bool w, bool s, uint currentMode)
        {
            if (s)
            {
                ChangeRegisters(Mode.USR);
            }

            for (int i = 0; i < 16; i++)
            {
                var r = ((uint)i);

                if ((code & (1 << i)) != 0)
                {
                    if (w)
                    {
                        Set(n, last);
                    }

                    Set(r, Read(2, address & ~3U));
                    address += 4;
                }
            }

            if (s)
            {
                ChangeRegisters(currentMode);
            }

            if (s && (code & 0x8000) != 0)
            {
                MoveSPSRToCPSR();
            }
        }

        private void OpStm(uint n, uint address, uint last, bool w, bool s, uint currentMode)
        {
            if (s)
            {
                ChangeRegisters(Mode.USR);
            }

            for (int i = 0; i < 16; i++)
            {
                var r = ((uint)i);

                if ((code & (1 << i)) != 0)
                {
                    Write(2, address & ~3U, Get(r));
                    address += 4;

                    if (w)
                    {
                        Set(n, last);
                    }
                }
            }

            if (s)
            {
                ChangeRegisters(currentMode);
            }
        }

        private void OpMoveStatusToRegister()
        {
            var p = (code >> 22) & 1;
            var d = (code >> 12) & 15;

            if (p == 0)
            {
                Set(d, cpsr.Save());
            }
            else
            {
                Set(d, spsr.Save());
            }
        }

        private void OpMoveRegisterToStatus()
        {
            var value = Get(code & 15);

            OpMoveToStatus(value);
        }

        private void OpMoveConstantToStatus()
        {
            var value = (code >> 0) & 255;
            var shift = (code >> 7) & 30;

            OpMoveToStatus(ROR(value, shift));
        }

        private void OpMoveToStatus(uint value)
        {
            var p = (code >> 22) & 1;
            var f = (code >> 19) & 1;
            var c = (code >> 16) & 1;

            var psr = p == 0
                ? cpsr
                : spsr;

            if (psr != null)
            {
                if (f == 1)
                {
                    psr.n = (value >> 31) & 1;
                    psr.z = (value >> 30) & 1;
                    psr.c = (value >> 29) & 1;
                    psr.v = (value >> 28) & 1;
                }

                if (c == 1 && cpsr.m != Mode.USR)
                {
                    if (p == 0)
                    {
                        ChangeRegisters(value & 31);
                    }

                    psr.i = (value >> 7) & 1;
                    psr.f = (value >> 6) & 1;
                    psr.t = (value >> 5) & 1;
                    psr.m = (value >> 0) & 31;
                }
            }
        }

        private void OpMultiply32()
        {
            var a = (code >> 21) & 1;
            var d = (code >> 16) & 15;
            var n = (code >> 12) & 15;
            var s = (code >> 8) & 15;
            var m = (code >> 0) & 15;

            var rm = Get(m);
            var rs = Get(s);
            var rn = a == 1 ? Get(n) : 0;

            Set(d, Mul(rm, rs, rn));
        }

        private void OpMultiply64()
        {
            var signExtend = (code >> 22) & 1;
            var accumulate = (code >> 21) & 1;
            var h = (code >> 16) & 15;
            var l = (code >> 12) & 15;
            var s = (code >> 8) & 15;
            var m = (code >> 0) & 15;

            const ulong sign = 0x80000000;

            ulong rm = signExtend == 0 ? Get(m) : ((Get(m) ^ sign) - sign);
            ulong rs = signExtend == 0 ? Get(s) : ((Get(s) ^ sign) - sign);
            ulong rh = accumulate == 1 ? Get(h) : 0;
            ulong rl = accumulate == 1 ? Get(l) : 0;

            ulong rd = (rm * rs) + (rh << 32) + rl;

            Set(h, (uint)(rd >> 32));
            Set(l, (uint)(rd));

            var save = (code >> 20) & 1;
            if (save == 1)
            {
                cpsr.n = (uint)(rd >> 63);
                cpsr.z = (uint)(rd == 0 ? 1 : 0);
            }
        }

        private void OpSwap()
        {
            var m = (code >> 0) & 15;
            var d = (code >> 12) & 15;
            var n = (code >> 16) & 15;

            var rm = Get(m);
            var rn = Get(n);

            uint tmp;

            switch ((code >> 22) & 1)
            {
            case 0:
                tmp = ReadWord(rn);
                Write(2, rn, rm);
                Set(d, tmp);
                break;

            case 1:
                tmp = ReadByte(rn);
                Write(0, rn, rm);
                Set(d, tmp);
                break;
            }
        }

        private void OpSWI()
        {
            Isr(Mode.SVC, Vector.SWI);
        }

        private void OpUND()
        {
            Isr(Mode.UND, Vector.UND);
        }

        #endregion

        private uint GetOperand2ConstantShift()
        {
            var shift = (code >> 7) & 31;
            var value = Get(code & 15);

            switch ((code >> 5) & 3)
            {
            case 0: return shift != 0 ? LSL(value, shift) : LSL(value,  0);
            case 1: return shift != 0 ? LSR(value, shift) : LSR(value, 32);
            case 2: return shift != 0 ? ASR(value, shift) : ASR(value, 32);
            case 3: return shift != 0 ? ROR(value, shift) : RRX(value);
            }

            throw new CompilerPleasingException();
        }

        private uint GetOperand2RegisterShift()
        {
            var rs = (code >> 8) & 15;
            var rm = (code >> 0) & 15;
            var shift = Get(rs) & 255;
            var value = Get(rm, 4);

            cycles++;

            switch ((code >> 5) & 3)
            {
            case 0: return LSL(value, shift);
            case 1: return LSR(value, shift);
            case 2: return ASR(value, shift);
            case 3: return ROR(value, shift);
            }

            throw new CompilerPleasingException();
        }
    }
}
