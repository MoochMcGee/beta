using System;
using Beta.Platform.Exceptions;

namespace Beta.Platform.Processors.RP6502
{
    public abstract class Core
    {
        private Action[] codes;
        private Action[] modes;
        private Status p;
        private Interrupts interrupts;
        private Registers registers;
        private byte ir;
        private byte open;
        private int rw, rwOld;

        public bool Edge
        {
            get { return rw != rwOld; }
        }

        protected Core()
        {
            modes = new Action[]
            {
                //    0        1        2        3        4        5        6        7        8        9        a        b        c        d        e        f
                Am____a, AmInx_a, AmImp_a, AmInx_a, AmZpg_a, AmZpg_a, AmZpg_a, AmZpg_a, AmImp_a, AmImm_a, AmImp_a, AmImm_a, AmAbs_a, AmAbs_a, AmAbs_a, AmAbs_a, // 0
                AmImm_a, AmIny_r, AmImp_a, AmIny_w, AmZpx_a, AmZpx_a, AmZpx_a, AmZpx_a, AmImp_a, AmAby_r, AmImp_a, AmAby_w, AmAbx_r, AmAbx_r, AmAbx_w, AmAbx_w, // 1
                Am____a, AmInx_a, AmImp_a, AmInx_a, AmZpg_a, AmZpg_a, AmZpg_a, AmZpg_a, AmImp_a, AmImm_a, AmImp_a, AmImm_a, AmAbs_a, AmAbs_a, AmAbs_a, AmAbs_a, // 2
                AmImm_a, AmIny_r, AmImp_a, AmIny_w, AmZpx_a, AmZpx_a, AmZpx_a, AmZpx_a, AmImp_a, AmAby_r, AmImp_a, AmAby_w, AmAbx_r, AmAbx_r, AmAbx_w, AmAbx_w, // 3
                AmImp_a, AmInx_a, AmImp_a, AmInx_a, AmZpg_a, AmZpg_a, AmZpg_a, AmZpg_a, AmImp_a, AmImm_a, AmImp_a, AmImm_a, AmImm_a, AmAbs_a, AmAbs_a, AmAbs_a, // 4
                AmImm_a, AmIny_r, AmImp_a, AmIny_w, AmZpx_a, AmZpx_a, AmZpx_a, AmZpx_a, AmImp_a, AmAby_r, AmImp_a, AmAby_w, AmAbx_r, AmAbx_r, AmAbx_w, AmAbx_w, // 5
                AmImp_a, AmInx_a, AmImp_a, AmInx_a, AmZpg_a, AmZpg_a, AmZpg_a, AmZpg_a, AmImp_a, AmImm_a, AmImp_a, AmImm_a, AmAbs_a, AmAbs_a, AmAbs_a, AmAbs_a, // 6
                AmImm_a, AmIny_r, AmImp_a, AmIny_w, AmZpx_a, AmZpx_a, AmZpx_a, AmZpx_a, AmImp_a, AmAby_r, AmImp_a, AmAby_w, AmAbx_r, AmAbx_r, AmAbx_w, AmAbx_w, // 7
                AmImm_a, AmInx_a, AmImm_a, AmInx_a, AmZpg_a, AmZpg_a, AmZpg_a, AmZpg_a, AmImp_a, AmImm_a, AmImp_a, AmImm_a, AmAbs_a, AmAbs_a, AmAbs_a, AmAbs_a, // 8
                AmImm_a, AmIny_w, AmImp_a, AmIny_w, AmZpx_a, AmZpx_a, AmZpy_a, AmZpy_a, AmImp_a, AmAby_w, AmImp_a, AmAby_w, AmAbs_a, AmAbx_w, AmAbs_a, AmAby_w, // 9
                AmImm_a, AmInx_a, AmImm_a, AmInx_a, AmZpg_a, AmZpg_a, AmZpg_a, AmZpg_a, AmImp_a, AmImm_a, AmImp_a, AmImm_a, AmAbs_a, AmAbs_a, AmAbs_a, AmAbs_a, // a
                AmImm_a, AmIny_r, AmImp_a, AmIny_r, AmZpx_a, AmZpx_a, AmZpy_a, AmZpy_a, AmImp_a, AmAby_r, AmImp_a, AmAby_r, AmAbx_r, AmAbx_r, AmAby_r, AmAby_r, // b
                AmImm_a, AmInx_a, AmImm_a, AmInx_a, AmZpg_a, AmZpg_a, AmZpg_a, AmZpg_a, AmImp_a, AmImm_a, AmImp_a, AmImm_a, AmAbs_a, AmAbs_a, AmAbs_a, AmAbs_a, // c
                AmImm_a, AmIny_r, AmImp_a, AmIny_w, AmZpx_a, AmZpx_a, AmZpx_a, AmZpx_a, AmImp_a, AmAby_r, AmImp_a, AmAby_w, AmAbx_r, AmAbx_r, AmAbx_w, AmAbx_w, // d
                AmImm_a, AmInx_a, AmImm_a, AmInx_a, AmZpg_a, AmZpg_a, AmZpg_a, AmZpg_a, AmImp_a, AmImm_a, AmImp_a, AmImm_a, AmAbs_a, AmAbs_a, AmAbs_a, AmAbs_a, // e
                AmImm_a, AmIny_r, AmImp_a, AmIny_w, AmZpx_a, AmZpx_a, AmZpx_a, AmZpx_a, AmImp_a, AmAby_r, AmImp_a, AmAby_w, AmAbx_r, AmAbx_r, AmAbx_w, AmAbx_w // f
            };

            codes = new Action[]
            {
                //    0        1        2        3        4        5        6        7        8        9        a        b        c        d        e        f
                OpBrk_i, OpOra_m, OpJam_i, OpSlo_m, OpDop_i, OpOra_m, OpAsl_m, OpSlo_m, OpPhp_i, OpOra_m, OpAsl_a, OpAnc_m, OpTop_i, OpOra_m, OpAsl_m, OpSlo_m, // 0
                OpBpl_m, OpOra_m, OpJam_i, OpSlo_m, OpDop_i, OpOra_m, OpAsl_m, OpSlo_m, OpClc_i, OpOra_m, OpNop_i, OpSlo_m, OpTop_i, OpOra_m, OpAsl_m, OpSlo_m, // 1
                OpJsr_m, OpAnd_m, OpJam_i, OpRla_m, OpBit_m, OpAnd_m, OpRol_m, OpRla_m, OpPlp_i, OpAnd_m, OpRol_a, OpAnc_m, OpBit_m, OpAnd_m, OpRol_m, OpRla_m, // 2
                OpBmi_m, OpAnd_m, OpJam_i, OpRla_m, OpDop_i, OpAnd_m, OpRol_m, OpRla_m, OpSec_i, OpAnd_m, OpNop_i, OpRla_m, OpTop_i, OpAnd_m, OpRol_m, OpRla_m, // 3
                OpRti_i, OpEor_m, OpJam_i, OpSre_m, OpDop_i, OpEor_m, OpLsr_m, OpSre_m, OpPha_i, OpEor_m, OpLsr_a, OpAsr_m, OpJmp_m, OpEor_m, OpLsr_m, OpSre_m, // 4
                OpBvc_m, OpEor_m, OpJam_i, OpSre_m, OpDop_i, OpEor_m, OpLsr_m, OpSre_m, OpCli_i, OpEor_m, OpNop_i, OpSre_m, OpTop_i, OpEor_m, OpLsr_m, OpSre_m, // 5
                OpRts_i, OpAdc_m, OpJam_i, OpRra_m, OpDop_i, OpAdc_m, OpRor_m, OpRra_m, OpPla_i, OpAdc_m, OpRor_a, OpArr_m, OpJmi_m, OpAdc_m, OpRor_m, OpRra_m, // 6
                OpBvs_m, OpAdc_m, OpJam_i, OpRra_m, OpDop_i, OpAdc_m, OpRor_m, OpRra_m, OpSei_i, OpAdc_m, OpNop_i, OpRra_m, OpTop_i, OpAdc_m, OpRor_m, OpRra_m, // 7
                OpDop_i, OpSta_m, OpDop_i, OpAax_m, OpSty_m, OpSta_m, OpStx_m, OpAax_m, OpDey_i, OpDop_i, OpTxa_i, OpXaa_m, OpSty_m, OpSta_m, OpStx_m, OpAax_m, // 8
                OpBcc_m, OpSta_m, OpJam_i, OpAxa_m, OpSty_m, OpSta_m, OpStx_m, OpAax_m, OpTya_i, OpSta_m, OpTxs_i, OpXas_m, OpSya_m, OpSta_m, OpSxa_m, OpAxa_m, // 9
                OpLdy_m, OpLda_m, OpLdx_m, OpLax_m, OpLdy_m, OpLda_m, OpLdx_m, OpLax_m, OpTay_i, OpLda_m, OpTax_i, OpLax_m, OpLdy_m, OpLda_m, OpLdx_m, OpLax_m, // a
                OpBcs_m, OpLda_m, OpJam_i, OpLax_m, OpLdy_m, OpLda_m, OpLdx_m, OpLax_m, OpClv_i, OpLda_m, OpTsx_i, OpLar_m, OpLdy_m, OpLda_m, OpLdx_m, OpLax_m, // b
                OpCpy_m, OpCmp_m, OpDop_i, OpDcp_m, OpCpy_m, OpCmp_m, OpDec_m, OpDcp_m, OpIny_i, OpCmp_m, OpDex_i, OpAxs_m, OpCpy_m, OpCmp_m, OpDec_m, OpDcp_m, // c
                OpBne_m, OpCmp_m, OpJam_i, OpDcp_m, OpDop_i, OpCmp_m, OpDec_m, OpDcp_m, OpCld_i, OpCmp_m, OpNop_i, OpDcp_m, OpTop_i, OpCmp_m, OpDec_m, OpDcp_m, // d
                OpCpx_m, OpSbc_m, OpDop_i, OpIsc_m, OpCpx_m, OpSbc_m, OpInc_m, OpIsc_m, OpInx_i, OpSbc_m, OpNop_i, OpSbc_m, OpCpx_m, OpSbc_m, OpInc_m, OpIsc_m, // e
                OpBeq_m, OpSbc_m, OpJam_i, OpIsc_m, OpDop_i, OpSbc_m, OpInc_m, OpIsc_m, OpSed_i, OpSbc_m, OpNop_i, OpIsc_m, OpTop_i, OpSbc_m, OpInc_m, OpIsc_m // f
            };
        }

        private void Branch(bool flag)
        {
            var data = Read(registers.EA, true);

            if (flag)
            {
                Read(registers.PC);
                registers.PCL = Alu.Add(registers.PCL, data);

                switch (data >> 7)
                {
                case 0: if (Alu.C == 1) { Read(registers.PC, true); registers.PCH += 0x01; } break; // unsigned, pcl+data carried
                case 1: if (Alu.C == 0) { Read(registers.PC, true); registers.PCH += 0xff; } break; //   signed, pcl-data borrowed
                }
            }
        }

        #region ALU

        private byte Mov(byte data)
        {
            p.N = (data >> 7);
            p.Z = (data == 0) ? 1 : 0;
            return data;
        }

        private void Adc(byte data)
        {
            var temp = (byte)((registers.A + data) + p.C);
            var bits = (byte)((registers.A ^ temp) & ~(registers.A ^ data));

            p.V = (bits) >> 7;
            p.C = (bits ^ registers.A ^ data ^ temp) >> 7;

            registers.A = Mov(temp);
        }

        private void Sbc(byte data)
        {
            Adc((byte)(~data));
        }

        private byte Cmp(byte left, byte data)
        {
            var temp = (left - data);

            p.C = (~temp >> 8) & 1;

            return Mov((byte)(temp));
        }

        private void And(byte data)
        {
            Mov(registers.A &= data);
        }

        private void Eor(byte data)
        {
            Mov(registers.A ^= data);
        }

        private void Ora(byte data)
        {
            Mov(registers.A |= data);
        }

        private byte Shl(byte data, int carry)
        {
            p.C = (data >> 7);

            data = (byte)((data << 1) | (carry << 0));

            return Mov(data);
        }

        private byte Shr(byte data, int carry)
        {
            p.C = (data & 1);

            data = (byte)((data >> 1) | (carry << 7));

            return Mov(data);
        }

        #endregion

        #region Codes

        private void OpAsl_a()
        {
            registers.A = Shl(registers.A, 0);
        }

        private void OpLsr_a()
        {
            registers.A = Shr(registers.A, 0);
        }

        private void OpRol_a()
        {
            registers.A = Shl(registers.A, p.C);
        }

        private void OpRor_a()
        {
            registers.A = Shr(registers.A, p.C);
        }

        private void OpAdc_m()
        {
            Adc(Read(registers.EA, true));
        }

        private void OpAnd_m()
        {
            And(Read(registers.EA, true));
        }

        private void OpAsl_m()
        {
            var data = Read(registers.EA);
            Write(registers.EA, data);
            Write(registers.EA, Shl(data, 0), true);
        }

        private void OpBcc_m()
        {
            Branch(p.C == 0);
        }

        private void OpBcs_m()
        {
            Branch(p.C != 0);
        }

        private void OpBeq_m()
        {
            Branch(p.Z != 0);
        }

        private void OpBit_m()
        {
            var data = Read(registers.EA, true);

            p.N = (data >> 7) & 1;
            p.V = (data >> 6) & 1;
            p.Z = (data & registers.A) == 0 ? 1 : 0;
        }

        private void OpBmi_m()
        {
            Branch(p.N != 0);
        }

        private void OpBne_m()
        {
            Branch(p.Z == 0);
        }

        private void OpBpl_m()
        {
            Branch(p.N == 0);
        }

        private void OpBrk_i()
        {
            const ushort dec = 0xffff;
            const ushort inc = 0x0001;

            Read(registers.PC);
            registers.PC += (interrupts.Available == 1) ? dec : inc;

            if (interrupts.Res == 1)
            {
                Read(registers.SP); registers.SPL--;
                Read(registers.SP); registers.SPL--;
                Read(registers.SP); registers.SPL--;
            }
            else
            {
                var flag = (byte)(p.Pack() & ~(interrupts.Available << 4));

                Write(registers.SP, registers.PCH); registers.SPL--;
                Write(registers.SP, registers.PCL); registers.SPL--;
                Write(registers.SP, flag); registers.SPL--;
            }

            var vector = interrupts.GetVector();

            p.I = 1;
            registers.PCL = Read((ushort)(vector + 0));
            registers.PCH = Read((ushort)(vector + 1), true);
        }

        private void OpBvc_m()
        {
            Branch(p.V == 0);
        }

        private void OpBvs_m()
        {
            Branch(p.V != 0);
        }

        private void OpClc_i()
        {
            p.C = 0;
        }

        private void OpCld_i()
        {
            p.D = 0;
        }

        private void OpCli_i()
        {
            p.I = 0;
        }

        private void OpClv_i()
        {
            p.V = 0;
        }

        private void OpCmp_m()
        {
            Cmp(registers.A, Read(registers.EA, true));
        }

        private void OpCpx_m()
        {
            Cmp(registers.X, Read(registers.EA, true));
        }

        private void OpCpy_m()
        {
            Cmp(registers.Y, Read(registers.EA, true));
        }

        private void OpDec_m()
        {
            var data = Read(registers.EA);
            Write(registers.EA, data); Mov(--data);
            Write(registers.EA, data, true);
        }

        private void OpDex_i()
        {
            Mov(--registers.X);
        }

        private void OpDey_i()
        {
            Mov(--registers.Y);
        }

        private void OpEor_m()
        {
            Eor(Read(registers.EA, true));
        }

        private void OpInc_m()
        {
            var data = Read(registers.EA);
            Write(registers.EA, data); Mov(++data);
            Write(registers.EA, data, true);
        }

        private void OpInx_i()
        {
            Mov(++registers.X);
        }

        private void OpIny_i()
        {
            Mov(++registers.Y);
        }

        private void OpJmi_m()
        {
            registers.PCL = Read(registers.EA); registers.EAL++; // Emulate the JMP ($nnnn) bug
            registers.PCH = Read(registers.EA, true);
        }

        private void OpJmp_m()
        {
            registers.PCL = Read(registers.EA++);
            registers.PCH = Read(registers.EA++, true);
        }

        private void OpJsr_m()
        {
            registers.EAL = Read(registers.PC++);

            Read(registers.SP);
            Write(registers.SP, registers.PCH); registers.SPL--;
            Write(registers.SP, registers.PCL); registers.SPL--;

            registers.PCH = Read(registers.PC++, true);
            registers.PCL = registers.EAL;
        }

        private void OpLda_m()
        {
            registers.A = Mov(Read(registers.EA, true));
        }

        private void OpLdx_m()
        {
            registers.X = Mov(Read(registers.EA, true));
        }

        private void OpLdy_m()
        {
            registers.Y = Mov(Read(registers.EA, true));
        }

        private void OpLsr_m()
        {
            var data = Read(registers.EA);
            Write(registers.EA, data); data = Shr(data, 0);
            Write(registers.EA, data, true);
        }

        private void OpOra_m()
        {
            Ora(Read(registers.EA, true));
        }

        private void OpNop_i()
        {
        }

        private void OpPha_i()
        {
            Write(registers.SP, registers.A, true);
            registers.SPL--;
        }

        private void OpPhp_i()
        {
            Write(registers.SP, p.Pack(), true);
            registers.SPL--;
        }

        private void OpPla_i()
        {
            Read(registers.SP);
            registers.SPL++;

            registers.A = Mov(Read(registers.SP, true));
        }

        private void OpPlp_i()
        {
            Read(registers.SP);
            registers.SPL++;

            p.Unpack(Read(registers.SP, true));
        }

        private void OpRol_m()
        {
            var data = Read(registers.EA);
            Write(registers.EA, data);
            Write(registers.EA, Shl(data, p.C), true);
        }

        private void OpRor_m()
        {
            var data = Read(registers.EA);
            Write(registers.EA, data);
            Write(registers.EA, Shr(data, p.C), true);
        }

        private void OpRti_i()
        {
            Read(registers.SP); registers.SPL++;
            p.Unpack(Read(registers.SP)); registers.SPL++;
            registers.PCL = Read(registers.SP); registers.SPL++;
            registers.PCH = Read(registers.SP, true);
        }

        private void OpRts_i()
        {
            Read(registers.SP); registers.SPL++;
            registers.PCL = Read(registers.SP); registers.SPL++;
            registers.PCH = Read(registers.SP);
            Read(registers.PC++, true);
        }

        private void OpSbc_m()
        {
            Sbc(Read(registers.EA, true));
        }

        private void OpSec_i()
        {
            p.C = 1;
        }

        private void OpSed_i()
        {
            p.D = 1;
        }

        private void OpSei_i()
        {
            p.I = 1;
        }

        private void OpSta_m()
        {
            Write(registers.EA, registers.A, true);
        }

        private void OpStx_m()
        {
            Write(registers.EA, registers.X, true);
        }

        private void OpSty_m()
        {
            Write(registers.EA, registers.Y, true);
        }

        private void OpTax_i()
        {
            Mov(registers.X = registers.A);
        }

        private void OpTay_i()
        {
            Mov(registers.Y = registers.A);
        }

        private void OpTsx_i()
        {
            Mov(registers.X = registers.SPL);
        }

        private void OpTxa_i()
        {
            Mov(registers.A = registers.X);
        }

        private void OpTxs_i()
        {
            registers.SPL = registers.X;
        }

        private void OpTya_i()
        {
            Mov(registers.A = registers.Y);
        }

        // Unofficial codes
        private void OpAax_m()
        {
            Write(registers.EA, (byte)(registers.A & registers.X), true);
        }

        private void OpAnc_m()
        {
            And(Read(registers.EA, true));
            p.C = (registers.A >> 7);
        }

        private void OpArr_m()
        {
            And(Read(registers.EA, true));
            registers.A = Shr(registers.A, p.C);

            p.C = (registers.A >> 6) & 1;
            p.V = (registers.A >> 5 ^ p.C) & 1;
        }

        private void OpAsr_m()
        {
            And(Read(registers.EA, true));
            registers.A = Shr(registers.A, 0);
        }

        private void OpAxa_m()
        {
            Write(registers.EA, (byte)(registers.A & registers.X & 7), true);
        }

        private void OpAxs_m()
        {
            registers.X = Cmp((byte)(registers.A & registers.X), Read(registers.EA, true));
        }

        private void OpDcp_m()
        {
            var data = Read(registers.EA);
            Write(registers.EA, data); Mov(--data);
            Write(registers.EA, data, true);

            Cmp(registers.A, data);
        }

        private void OpDop_i()
        {
            Read(registers.EA, true);
        }

        private void OpIsc_m()
        {
            var data = Read(registers.EA);
            Write(registers.EA, data); Mov(++data);
            Write(registers.EA, data, true);

            Sbc(data);
        }

        private void OpJam_i()
        {
            throw new ProcessorJammedException("Keep on jammin'!");
        }

        private void OpLar_m()
        {
            registers.A = registers.X = Mov(registers.SPL &= Read(registers.EA, true));
        }

        private void OpLax_m()
        {
            registers.A = registers.X = Mov(Read(registers.EA, true));
        }

        private void OpRla_m()
        {
            var data = Read(registers.EA);
            Write(registers.EA, data); And(data = Shl(data, p.C));
            Write(registers.EA, data, true);
        }

        private void OpRra_m()
        {
            var data = Read(registers.EA);
            Write(registers.EA, data); Adc(data = Shr(data, p.C));
            Write(registers.EA, data, true);
        }

        private void OpSlo_m()
        {
            var data = Read(registers.EA);
            Write(registers.EA, data); Ora(data = Shl(data, 0));
            Write(registers.EA, data, true);
        }

        private void OpSre_m()
        {
            var data = Read(registers.EA);
            Write(registers.EA, data); Eor(data = Shr(data, 0));
            Write(registers.EA, data, true);
        }

        private void OpSxa_m()
        {
            var data = (byte)(registers.X & (registers.EAH + 1));

            registers.EAL += registers.Y;
            Read(registers.EA);

            if (registers.EAL < registers.Y)
            {
                registers.EAH = data;
            }

            Write(registers.EA, data, true);
        }

        private void OpSya_m()
        {
            var data = (byte)(registers.Y & (registers.EAH + 1));

            registers.EAL += registers.X;
            Read(registers.EA);

            if (registers.EAL < registers.X)
            {
                registers.EAH = data;
            }

            Write(registers.EA, data, true);
        }

        private void OpTop_i()
        {
            Read(registers.EA, true);
        }

        private void OpXaa_m()
        {
            registers.A = Mov((byte)(registers.X & Read(registers.EA, true)));
        }

        private void OpXas_m()
        {
            registers.SPL = (byte)(registers.A & registers.X);

            Write(registers.EA, (byte)(registers.SPL & (registers.EAH + 1)), true);
        }

        #endregion

        #region Modes

        private void Am____a()
        {
        }

        private void AmAbs_a()
        {
            registers.EAL = Read(registers.PC++);
            registers.EAH = Read(registers.PC++);
        }

        private void AmAbx_r()
        {
            registers.EAL = Read(registers.PC++);
            registers.EAH = Read(registers.PC++);
            registers.EAL += registers.X;

            if (registers.EAL < registers.X)
            {
                Read(registers.EA);
                registers.EAH++;
            }
        }

        private void AmAbx_w()
        {
            registers.EAL = Read(registers.PC++);
            registers.EAH = Read(registers.PC++);
            registers.EAL += registers.X;

            Read(registers.EA);

            if (registers.EAL < registers.X)
            {
                registers.EAH++;
            }
        }

        private void AmAby_r()
        {
            registers.EAL = Read(registers.PC++);
            registers.EAH = Read(registers.PC++);
            registers.EAL += registers.Y;

            if (registers.EAL < registers.Y)
            {
                Read(registers.EA);
                registers.EAH++;
            }
        }

        private void AmAby_w()
        {
            registers.EAL = Read(registers.PC++);
            registers.EAH = Read(registers.PC++);
            registers.EAL += registers.Y;

            Read(registers.EA);

            if (registers.EAL < registers.Y)
            {
                registers.EAH++;
            }
        }

        private void AmImm_a()
        {
            registers.EA = registers.PC++;
        }

        private void AmImp_a()
        {
            Read(registers.PC, true);
        }

        private void AmInx_a()
        {
            var pointer = Read(registers.PC++);

            Read(registers.PC);
            pointer += registers.X;

            registers.EAL = Read(pointer++);
            registers.EAH = Read(pointer);
        }

        private void AmIny_r()
        {
            var pointer = Read(registers.PC++);

            registers.EAL = Read(pointer++);
            registers.EAH = Read(pointer);
            registers.EAL += registers.Y;

            if (registers.EAL < registers.Y)
            {
                Read(registers.EA);
                registers.EAH++;
            }
        }

        private void AmIny_w()
        {
            var pointer = Read(registers.PC++);

            registers.EAL = Read(pointer++);
            registers.EAH = Read(pointer);
            registers.EAL += registers.Y;

            Read(registers.EA);

            if (registers.EAL < registers.Y)
            {
                registers.EAH++;
            }
        }

        private void AmZpg_a()
        {
            registers.EAL = Read(registers.PC++);
            registers.EAH = 0;
        }

        private void AmZpx_a()
        {
            registers.EAL = Read(registers.PC++);
            registers.EAH = 0;

            Read(registers.EA);
            registers.EAL += registers.X;
        }

        private void AmZpy_a()
        {
            registers.EAL = Read(registers.PC++);
            registers.EAH = 0;

            Read(registers.EA);
            registers.EAL += registers.Y;
        }

        #endregion

        protected abstract void Read(int address, ref byte data);

        protected abstract void Write(int address, byte data);

        public virtual void ResetHard()
        {
            registers.EA = 0x0000;
            registers.PC = 0x0000;
            registers.SP = 0x0100;

            registers.A = 0;
            registers.X = 0;
            registers.Y = 0;
            p.Unpack(0);

            interrupts.Res = 1;
            interrupts.Poll(p.I);
        }

        public virtual void ResetSoft()
        {
            registers.EA = 0x0000;

            interrupts.Res = 1;
            interrupts.Poll(p.I);
        }

        public virtual void Update()
        {
            ir = Read(registers.PC++);

            if (interrupts.Available == 1)
            {
                ir = 0;
            }

            modes[ir]();
            codes[ir]();
        }

        public void Irq(int value)
        {
            interrupts.Irq = value; // level sensitive
        }

        public void Nmi(int value)
        {
            if (interrupts.NmiLatch < value) // edge sensitive (0 -> 1)
            {
                interrupts.Nmi = 1;
            }

            interrupts.NmiLatch = value;
        }

        protected byte Read(int address, bool last = false)
        {
            rwOld = rw;
            rw = 1;

            if (last)
            {
                interrupts.Poll(p.I);
            }

            Read(address, ref open);

            return open;
        }

        protected void Write(int address, byte data, bool last = false)
        {
            rwOld = rw;
            rw = 0;

            if (last)
            {
                interrupts.Poll(p.I);
            }

            Write(address, open = data);
        }
    }
}
