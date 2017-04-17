using System;
using Beta.Platform.Exceptions;

namespace Beta.Platform.Processors.RP6502
{
    public static class R6502
    {
        static readonly Action<R6502State>[] modes =
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
            AmImm_a, AmIny_r, AmImp_a, AmIny_w, AmZpx_a, AmZpx_a, AmZpx_a, AmZpx_a, AmImp_a, AmAby_r, AmImp_a, AmAby_w, AmAbx_r, AmAbx_r, AmAbx_w, AmAbx_w  // f
        };

        static readonly Action<R6502State>[] codes =
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
            OpBeq_m, OpSbc_m, OpJam_i, OpIsc_m, OpDop_i, OpSbc_m, OpInc_m, OpIsc_m, OpSed_i, OpSbc_m, OpNop_i, OpIsc_m, OpTop_i, OpSbc_m, OpInc_m, OpIsc_m  // f
        };

        static void branch(R6502State e, bool flag)
        {
            read(e, e.regs.ea, true);
            var data = e.data;

            if (flag)
            {
                read(e, e.regs.pc);
                e.regs.pcl = ALU.add(e.regs.pcl, data);

                switch (data >> 7)
                {
                case 0: if (ALU.c == 1) { read(e, e.regs.pc, true); e.regs.pch += 0x01; } break; // unsigned, pcl+data carried
                case 1: if (ALU.c == 0) { read(e, e.regs.pc, true); e.regs.pch += 0xff; } break; //   signed, pcl-data borrowed
                }
            }
        }

        #region ALU

        static byte mov(R6502State e, byte data)
        {
            e.flags.n = (data >> 7);
            e.flags.z = (data == 0) ? 1 : 0;
            return data;
        }

        static void adc(R6502State e, byte data)
        {
            var temp = (byte)((e.regs.a + data) + e.flags.c);
            var bits = (byte)((e.regs.a ^ temp) & ~(e.regs.a ^ data));

            e.flags.v = (bits) >> 7;
            e.flags.c = (bits ^ e.regs.a ^ data ^ temp) >> 7;

            e.regs.a = mov(e, temp);
        }

        static void sbc(R6502State e, byte data)
        {
            adc(e, (byte)(~data));
        }

        static byte cmp(R6502State e, byte left, byte data)
        {
            var temp = (left - data);

            e.flags.c = (~temp >> 8) & 1;

            return mov(e, (byte)(temp));
        }

        static void and(R6502State e, byte data)
        {
            mov(e, e.regs.a &= data);
        }

        static void eor(R6502State e, byte data)
        {
            mov(e, e.regs.a ^= data);
        }

        static void ora(R6502State e, byte data)
        {
            mov(e, e.regs.a |= data);
        }

        static byte shl(R6502State e, byte data, int carry)
        {
            e.flags.c = (data >> 7);

            data = (byte)((data << 1) | (carry << 0));

            return mov(e, data);
        }

        static byte shr(R6502State e, byte data, int carry)
        {
            e.flags.c = (data & 1);

            data = (byte)((data >> 1) | (carry << 7));

            return mov(e, data);
        }

        #endregion

        #region Codes

        static void OpAsl_a(R6502State e)
        {
            e.regs.a = shl(e, e.regs.a, 0);
        }

        static void OpLsr_a(R6502State e)
        {
            e.regs.a = shr(e, e.regs.a, 0);
        }

        static void OpRol_a(R6502State e)
        {
            e.regs.a = shl(e, e.regs.a, e.flags.c);
        }

        static void OpRor_a(R6502State e)
        {
            e.regs.a = shr(e, e.regs.a, e.flags.c);
        }

        static void OpAdc_m(R6502State e)
        {
            read(e, e.regs.ea, true);

            adc(e, e.data);
        }

        static void OpAnd_m(R6502State e)
        {
            read(e, e.regs.ea, true);

            and(e, e.data);
        }

        static void OpAsl_m(R6502State e)
        {
            read(e, e.regs.ea);

            var data = e.data;
            write(e, e.regs.ea, data);
            write(e, e.regs.ea, shl(e, data, 0), true);
        }

        static void OpBcc_m(R6502State e)
        {
            branch(e, e.flags.c == 0);
        }

        static void OpBcs_m(R6502State e)
        {
            branch(e, e.flags.c != 0);
        }

        static void OpBeq_m(R6502State e)
        {
            branch(e, e.flags.z != 0);
        }

        static void OpBit_m(R6502State e)
        {
            read(e, e.regs.ea, true);

            var data = e.data;

            e.flags.n = (data >> 7) & 1;
            e.flags.v = (data >> 6) & 1;
            e.flags.z = (data & e.regs.a) == 0 ? 1 : 0;
        }

        static void OpBmi_m(R6502State e)
        {
            branch(e, e.flags.n != 0);
        }

        static void OpBne_m(R6502State e)
        {
            branch(e, e.flags.z == 0);
        }

        static void OpBpl_m(R6502State e)
        {
            branch(e, e.flags.n == 0);
        }

        static void OpBrk_i(R6502State e)
        {
            const ushort dec = 0xffff;
            const ushort inc = 0x0001;

            read(e, e.regs.pc);
            e.regs.pc += (e.ints.int_available == 1) ? dec : inc;

            if (e.ints.res == 1)
            {
                read(e, e.regs.sp); e.regs.spl--;
                read(e, e.regs.sp); e.regs.spl--;
                read(e, e.regs.sp); e.regs.spl--;
            }
            else
            {
                var flag = (byte)(Flag.packFlags(e.flags) & ~(e.ints.int_available << 4));

                write(e, e.regs.sp, e.regs.pch); e.regs.spl--;
                write(e, e.regs.sp, e.regs.pcl); e.regs.spl--;
                write(e, e.regs.sp, flag); e.regs.spl--;
            }

            var vector = Interrupt.getVector(e.ints);

            e.flags.i = 1;

            read(e, (ushort)(vector + 0));
            e.regs.pcl = e.data;

            read(e, (ushort)(vector + 1), true);
            e.regs.pch = e.data;
        }

        static void OpBvc_m(R6502State e)
        {
            branch(e, e.flags.v == 0);
        }

        static void OpBvs_m(R6502State e)
        {
            branch(e, e.flags.v != 0);
        }

        static void OpClc_i(R6502State e)
        {
            e.flags.c = 0;
        }

        static void OpCld_i(R6502State e)
        {
            e.flags.d = 0;
        }

        static void OpCli_i(R6502State e)
        {
            e.flags.i = 0;
        }

        static void OpClv_i(R6502State e)
        {
            e.flags.v = 0;
        }

        static void OpCmp_m(R6502State e)
        {
            read(e, e.regs.ea, true);

            cmp(e, e.regs.a, e.data);
        }

        static void OpCpx_m(R6502State e)
        {
            read(e, e.regs.ea, true);

            cmp(e, e.regs.x, e.data);
        }

        static void OpCpy_m(R6502State e)
        {
            read(e, e.regs.ea, true);

            cmp(e, e.regs.y, e.data);
        }

        static void OpDec_m(R6502State e)
        {
            read(e, e.regs.ea);
            var data = e.data;

            write(e, e.regs.ea, data); mov(e, --data);
            write(e, e.regs.ea, data, true);
        }

        static void OpDex_i(R6502State e)
        {
            mov(e, --e.regs.x);
        }

        static void OpDey_i(R6502State e)
        {
            mov(e, --e.regs.y);
        }

        static void OpEor_m(R6502State e)
        {
            read(e, e.regs.ea, true);

            eor(e, e.data);
        }

        static void OpInc_m(R6502State e)
        {
            read(e, e.regs.ea);
            var data = e.data;

            write(e, e.regs.ea, data); mov(e, ++data);
            write(e, e.regs.ea, data, true);
        }

        static void OpInx_i(R6502State e)
        {
            mov(e, ++e.regs.x);
        }

        static void OpIny_i(R6502State e)
        {
            mov(e, ++e.regs.y);
        }

        static void OpJmi_m(R6502State e)
        {
            read(e, e.regs.ea); e.regs.eal++; // Emulate the JMP ($nnnn) bug
            e.regs.pcl = e.data;

            read(e, e.regs.ea, true);
            e.regs.pch = e.data;
        }

        static void OpJmp_m(R6502State e)
        {
            read(e, e.regs.ea++);
            e.regs.pcl = e.data;

            read(e, e.regs.ea++, true);
            e.regs.pch = e.data;
        }

        static void OpJsr_m(R6502State e)
        {
            read(e, e.regs.pc++);
            e.regs.eal = e.data;

            read(e, e.regs.sp);
            write(e, e.regs.sp, e.regs.pch); e.regs.spl--;
            write(e, e.regs.sp, e.regs.pcl); e.regs.spl--;

            read(e, e.regs.pc++, true);
            e.regs.pch = e.data;
            e.regs.pcl = e.regs.eal;
        }

        static void OpLda_m(R6502State e)
        {
            read(e, e.regs.ea, true);
            e.regs.a = mov(e, e.data);
        }

        static void OpLdx_m(R6502State e)
        {
            read(e, e.regs.ea, true);
            e.regs.x = mov(e, e.data);
        }

        static void OpLdy_m(R6502State e)
        {
            read(e, e.regs.ea, true);
            e.regs.y = mov(e, e.data);
        }

        static void OpLsr_m(R6502State e)
        {
            read(e, e.regs.ea);
            var data = e.data;

            write(e, e.regs.ea, data); data = shr(e, data, 0);
            write(e, e.regs.ea, data, true);
        }

        static void OpOra_m(R6502State e)
        {
            read(e, e.regs.ea, true);
            ora(e, e.data);
        }

        static void OpNop_i(R6502State e) { }

        static void OpPha_i(R6502State e)
        {
            write(e, e.regs.sp, e.regs.a, true);
            e.regs.spl--;
        }

        static void OpPhp_i(R6502State e)
        {
            write(e, e.regs.sp, Flag.packFlags(e.flags), true);
            e.regs.spl--;
        }

        static void OpPla_i(R6502State e)
        {
            read(e, e.regs.sp);
            e.regs.spl++;

            read(e, e.regs.sp, true);
            e.regs.a = mov(e, e.data);
        }

        static void OpPlp_i(R6502State e)
        {
            read(e, e.regs.sp);
            e.regs.spl++;

            read(e, e.regs.sp, true);
            Flag.unpackFlags(e.flags, e.data);
        }

        static void OpRol_m(R6502State e)
        {
            read(e, e.regs.ea);
            var data = e.data;

            write(e, e.regs.ea, data);
            write(e, e.regs.ea, shl(e, data, e.flags.c), true);
        }

        static void OpRor_m(R6502State e)
        {
            read(e, e.regs.ea);
            var data = e.data;

            write(e, e.regs.ea, data);
            write(e, e.regs.ea, shr(e, data, e.flags.c), true);
        }

        static void OpRti_i(R6502State e)
        {
            read(e, e.regs.sp); e.regs.spl++;

            read(e, e.regs.sp);
            Flag.unpackFlags(e.flags, e.data); e.regs.spl++;

            read(e, e.regs.sp); e.regs.spl++;
            e.regs.pcl = e.data;

            read(e, e.regs.sp, true);
            e.regs.pch = e.data;
        }

        static void OpRts_i(R6502State e)
        {
            read(e, e.regs.sp); e.regs.spl++;
            read(e, e.regs.sp); e.regs.spl++;
            e.regs.pcl = e.data;

            read(e, e.regs.sp);
            e.regs.pch = e.data;

            read(e, e.regs.pc++, true);
        }

        static void OpSbc_m(R6502State e)
        {
            read(e, e.regs.ea, true);
            sbc(e, e.data);
        }

        static void OpSec_i(R6502State e)
        {
            e.flags.c = 1;
        }

        static void OpSed_i(R6502State e)
        {
            e.flags.d = 1;
        }

        static void OpSei_i(R6502State e)
        {
            e.flags.i = 1;
        }

        static void OpSta_m(R6502State e)
        {
            write(e, e.regs.ea, e.regs.a, true);
        }

        static void OpStx_m(R6502State e)
        {
            write(e, e.regs.ea, e.regs.x, true);
        }

        static void OpSty_m(R6502State e)
        {
            write(e, e.regs.ea, e.regs.y, true);
        }

        static void OpTax_i(R6502State e)
        {
            mov(e, e.regs.x = e.regs.a);
        }

        static void OpTay_i(R6502State e)
        {
            mov(e, e.regs.y = e.regs.a);
        }

        static void OpTsx_i(R6502State e)
        {
            mov(e, e.regs.x = e.regs.spl);
        }

        static void OpTxa_i(R6502State e)
        {
            mov(e, e.regs.a = e.regs.x);
        }

        static void OpTxs_i(R6502State e)
        {
            e.regs.spl = e.regs.x;
        }

        static void OpTya_i(R6502State e)
        {
            mov(e, e.regs.a = e.regs.y);
        }

        // Unofficial codes
        static void OpAax_m(R6502State e)
        {
            write(e, e.regs.ea, (byte)(e.regs.a & e.regs.x), true);
        }

        static void OpAnc_m(R6502State e)
        {
            read(e, e.regs.ea, true);

            and(e, e.data);
            e.flags.c = (e.regs.a >> 7);
        }

        static void OpArr_m(R6502State e)
        {
            read(e, e.regs.ea, true);

            and(e, e.data);
            e.regs.a = shr(e, e.regs.a, e.flags.c);

            e.flags.c = (e.regs.a >> 6) & 1;
            e.flags.v = (e.regs.a >> 5 ^ e.flags.c) & 1;
        }

        static void OpAsr_m(R6502State e)
        {
            read(e, e.regs.ea, true);

            and(e, e.data);
            e.regs.a = shr(e, e.regs.a, 0);
        }

        static void OpAxa_m(R6502State e)
        {
            write(e, e.regs.ea, (byte)(e.regs.a & e.regs.x & 7), true);
        }

        static void OpAxs_m(R6502State e)
        {
            read(e, e.regs.ea, true);

            e.regs.x = cmp(e, (byte)(e.regs.a & e.regs.x), e.data);
        }

        static void OpDcp_m(R6502State e)
        {
            read(e, e.regs.ea);
            var data = e.data;

            write(e, e.regs.ea, data); mov(e, --data);
            write(e, e.regs.ea, data, true);

            cmp(e, e.regs.a, data);
        }

        static void OpDop_i(R6502State e)
        {
            read(e, e.regs.ea, true);
        }

        static void OpIsc_m(R6502State e)
        {
            read(e, e.regs.ea);
            var data = e.data;

            write(e, e.regs.ea, data); mov(e, ++data);
            write(e, e.regs.ea, data, true);

            sbc(e, data);
        }

        static void OpJam_i(R6502State e)
        {
            throw new ProcessorJammedException("Keep on jammin'!");
        }

        static void OpLar_m(R6502State e)
        {
            read(e, e.regs.ea, true);

            e.regs.a = e.regs.x = mov(e, e.regs.spl &= e.data);
        }

        static void OpLax_m(R6502State e)
        {
            read(e, e.regs.ea, true);

            e.regs.a = e.regs.x = mov(e, e.data);
        }

        static void OpRla_m(R6502State e)
        {
            read(e, e.regs.ea);
            var data = e.data;

            write(e, e.regs.ea, data); and(e, data = shl(e, data, e.flags.c));
            write(e, e.regs.ea, data, true);
        }

        static void OpRra_m(R6502State e)
        {
            read(e, e.regs.ea);
            var data = e.data;

            write(e, e.regs.ea, data); adc(e, data = shr(e, data, e.flags.c));
            write(e, e.regs.ea, data, true);
        }

        static void OpSlo_m(R6502State e)
        {
            read(e, e.regs.ea);
            var data = e.data;

            write(e, e.regs.ea, data); ora(e, data = shl(e, data, 0));
            write(e, e.regs.ea, data, true);
        }

        static void OpSre_m(R6502State e)
        {
            read(e, e.regs.ea);
            var data = e.data;

            write(e, e.regs.ea, data); eor(e, data = shr(e, data, 0));
            write(e, e.regs.ea, data, true);
        }

        static void OpSxa_m(R6502State e)
        {
            var data = (byte)(e.regs.x & (e.regs.eah + 1));

            e.regs.eal += e.regs.y;
            read(e, e.regs.ea);

            if (e.regs.eal < e.regs.y)
            {
                e.regs.eah = data;
            }

            write(e, e.regs.ea, data, true);
        }

        static void OpSya_m(R6502State e)
        {
            var data = (byte)(e.regs.y & (e.regs.eah + 1));

            e.regs.eal += e.regs.x;
            read(e, e.regs.ea);

            if (e.regs.eal < e.regs.x)
            {
                e.regs.eah = data;
            }

            write(e, e.regs.ea, data, true);
        }

        static void OpTop_i(R6502State e)
        {
            read(e, e.regs.ea, true);
        }

        static void OpXaa_m(R6502State e)
        {
            read(e, e.regs.ea, true);

            e.regs.a = mov(e, (byte)(e.regs.x & e.data));
        }

        static void OpXas_m(R6502State e)
        {
            e.regs.spl = (byte)(e.regs.a & e.regs.x);

            write(e, e.regs.ea, (byte)(e.regs.spl & (e.regs.eah + 1)), true);
        }

        #endregion

        #region Modes

        static void Am____a(R6502State e) { }

        static void AmAbs_a(R6502State e)
        {
            read(e, e.regs.pc++);
            e.regs.eal = e.data;

            read(e, e.regs.pc++);
            e.regs.eah = e.data;
        }

        static void AmAbx_r(R6502State e)
        {
            read(e, e.regs.pc++);
            e.regs.eal = e.data;

            read(e, e.regs.pc++);
            e.regs.eah = e.data;

            e.regs.eal += e.regs.x;

            if (e.regs.eal < e.regs.x)
            {
                read(e, e.regs.ea);
                e.regs.eah++;
            }
        }

        static void AmAbx_w(R6502State e)
        {
            read(e, e.regs.pc++);
            e.regs.eal = e.data;

            read(e, e.regs.pc++);
            e.regs.eah = e.data;

            e.regs.eal += e.regs.x;

            read(e, e.regs.ea);

            if (e.regs.eal < e.regs.x)
            {
                e.regs.eah++;
            }
        }

        static void AmAby_r(R6502State e)
        {
            read(e, e.regs.pc++);
            e.regs.eal = e.data;

            read(e, e.regs.pc++);
            e.regs.eah = e.data;

            e.regs.eal += e.regs.y;

            if (e.regs.eal < e.regs.y)
            {
                read(e, e.regs.ea);
                e.regs.eah++;
            }
        }

        static void AmAby_w(R6502State e)
        {
            read(e, e.regs.pc++);
            e.regs.eal = e.data;

            read(e, e.regs.pc++);
            e.regs.eah = e.data;

            e.regs.eal += e.regs.y;

            read(e, e.regs.ea);

            if (e.regs.eal < e.regs.y)
            {
                e.regs.eah++;
            }
        }

        static void AmImm_a(R6502State e)
        {
            e.regs.ea = e.regs.pc++;
        }

        static void AmImp_a(R6502State e)
        {
            read(e, e.regs.pc, true);
        }

        static void AmInx_a(R6502State e)
        {
            read(e, e.regs.pc++);
            var pointer = e.data;

            read(e, e.regs.pc);
            pointer += e.regs.x;

            read(e, pointer++);
            e.regs.eal = e.data;

            read(e, pointer);
            e.regs.eah = e.data;
        }

        static void AmIny_r(R6502State e)
        {
            read(e, e.regs.pc++);
            var pointer = e.data;

            read(e, pointer++);
            e.regs.eal = e.data;

            read(e, pointer);
            e.regs.eah = e.data;

            e.regs.eal += e.regs.y;

            if (e.regs.eal < e.regs.y)
            {
                read(e, e.regs.ea);
                e.regs.eah++;
            }
        }

        static void AmIny_w(R6502State e)
        {
            read(e, e.regs.pc++);
            var pointer = e.data;

            read(e, pointer++);
            e.regs.eal = e.data;

            read(e, pointer);
            e.regs.eah = e.data;

            e.regs.eal += e.regs.y;

            read(e, e.regs.ea);

            if (e.regs.eal < e.regs.y)
            {
                e.regs.eah++;
            }
        }

        static void AmZpg_a(R6502State e)
        {
            read(e, e.regs.pc++);
            e.regs.eal = e.data;
            e.regs.eah = 0;
        }

        static void AmZpx_a(R6502State e)
        {
            read(e, e.regs.pc++);
            e.regs.eal = e.data;
            e.regs.eah = 0;

            read(e, e.regs.ea);
            e.regs.eal += e.regs.x;
        }

        static void AmZpy_a(R6502State e)
        {
            read(e, e.regs.pc++);
            e.regs.eal = e.data;
            e.regs.eah = 0;

            read(e, e.regs.ea);
            e.regs.eal += e.regs.y;
        }

        #endregion

        public static void resetHard(R6502State e)
        {
            e.regs.ea = 0x0000;
            e.regs.pc = 0x0000;
            e.regs.sp = 0x0100;

            e.regs.a = 0;
            e.regs.x = 0;
            e.regs.y = 0;
            Flag.unpackFlags(e.flags, 0);

            e.ints.res = 1;
            Interrupt.poll(e.ints, e.flags.i);
        }

        public static void resetSoft(R6502State e)
        {
            e.regs.ea = 0x0000;

            e.ints.res = 1;
            Interrupt.poll(e.ints, e.flags.i);
        }

        public static void update(R6502State e)
        {
            read(e, e.regs.pc++);
            e.code = e.data;

            if (e.ints.int_available == 1)
            {
                e.code = 0;
            }

            modes[e.code](e);
            codes[e.code](e);
        }

        public static void read(R6502State e, ushort address, bool last = false)
        {
            e.address = address;
            e.read = true;

            if (last)
            {
                Interrupt.poll(e.ints, e.flags.i);
            }
        }

        public static void write(R6502State e, ushort address, byte data, bool last = false)
        {
            e.address = address;
            e.data = data;
            e.read = false;

            if (last)
            {
                Interrupt.poll(e.ints, e.flags.i);
            }
        }
    }
}
