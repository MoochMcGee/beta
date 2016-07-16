using System;

namespace Beta.Platform.Processors.RP65816
{
    public partial class Core
    {
        private void op_r(bool flag, Action codeB, Action codeW)
        {
            if (flag || p.e)
            {
                LastCycle();
                regs.rdl = Read(regs.aab, regs.aa);
                codeB();
            }
            else
            {
                regs.rdl = Read(regs.aab, regs.aa); regs.aa24++;
                LastCycle();
                regs.rdh = Read(regs.aab, regs.aa);
                codeW();
            }
        }

        private void op_m(bool flag, Action codeB, Action codeW)
        {
            if (p.m || p.e)
            {
                regs.rdl = Read(regs.aab, regs.aa);

                InternalOperation();
                codeB();

                LastCycle();
                Write(regs.aab, regs.aa, regs.rdl);
            }
            else
            {
                regs.rdl = Read(regs.aab, regs.aa); regs.aa24++;
                regs.rdh = Read(regs.aab, regs.aa);

                InternalOperation();
                codeW();

                Write(regs.aab, regs.aa, regs.rdh); regs.aa24--;
                LastCycle();
                Write(regs.aab, regs.aa, regs.rdl);
            }
        }

        private void op_w(bool flag, Action codeB, Action codeW)
        {
            if (flag || p.e)
            {
                codeB();
                LastCycle();
                Write(regs.aab, regs.aa, regs.rdl);
            }
            else
            {
                codeW();
                Write(regs.aab, regs.aa, regs.rdl); regs.aa24++;
                LastCycle();
                Write(regs.aab, regs.aa, regs.rdh);
            }
        }

        private void op_adc_m() => op_r(p.m, op_adc_b, op_adc_w);
        private void op_and_m() => op_r(p.m, op_and_b, op_and_w);
        private void op_asl_m() => op_m(p.m, op_asl_b, op_asl_w);
        private void op_bit_m() => op_r(p.m, op_bit_b, op_bit_w);
        private void op_cmp_m() => op_r(p.m, op_cmp_b, op_cmp_w);
        private void op_cpx_x() => op_r(p.x, op_cpx_b, op_cpx_w);
        private void op_cpy_x() => op_r(p.x, op_cpy_b, op_cpy_w);
        private void op_dec_m() => op_m(p.m, op_dec_b, op_dec_w);
        private void op_eor_m() => op_r(p.m, op_eor_b, op_eor_w);
        private void op_inc_m() => op_m(p.m, op_inc_b, op_inc_w);
        private void op_lda_m() => op_r(p.m, op_lda_b, op_lda_w);
        private void op_ldx_x() => op_r(p.x, op_ldx_b, op_ldx_w);
        private void op_ldy_x() => op_r(p.x, op_ldy_b, op_ldy_w);
        private void op_lsr_m() => op_m(p.m, op_lsr_b, op_lsr_w);
        private void op_ora_m() => op_r(p.m, op_ora_b, op_ora_w);
        private void op_rol_m() => op_m(p.m, op_rol_b, op_rol_w);
        private void op_ror_m() => op_m(p.m, op_ror_b, op_ror_w);
        private void op_sbc_m() => op_r(p.m, op_sbc_b, op_sbc_w);
        private void op_sta_m() => op_w(p.m, op_sta_b, op_sta_w);
        private void op_stx_x() => op_w(p.x, op_stx_b, op_stx_w);
        private void op_sty_x() => op_w(p.x, op_sty_b, op_sty_w);
        private void op_stz_m() => op_w(p.m, op_stz_b, op_stz_w);
        private void op_trb_m() => op_m(p.m, op_trb_b, op_trb_w);
        private void op_tsb_m() => op_m(p.m, op_tsb_b, op_tsb_w);

        private void op_move(int adjust)
        {
            var dp = Read(regs.pcb, regs.pc++);
            var sp = Read(regs.pcb, regs.pc++);
            db = dp;
            regs.rdl = Read(sp, regs.x);
            Write(dp, regs.y, regs.rdl);
            InternalOperation();

            if (p.x || p.e)
            {
                regs.xl += (byte)adjust;
                regs.yl += (byte)adjust;
            }
            else
            {
                regs.x += (ushort)adjust;
                regs.y += (ushort)adjust;
            }

            LastCycle();
            InternalOperation();
            if (regs.a != 0) regs.pc -= 3;
            regs.a--;
        }

        private void op_asl_a()
        {
            if (p.m || p.e)
            {
                regs.rdl = regs.al;
                op_asl_b();
                regs.al = regs.rdl;
            }
            else
            {
                regs.rd = regs.a;
                op_asl_w();
                regs.a = regs.rd;
            }
        }

        private void op_clc_i() { p.c = false; }
        private void op_cld_i() { p.d = false; }
        private void op_cli_i() { p.i = false; }
        private void op_clv_i() { p.v = false; }

        private void op_dec_a()
        {
            if (p.m || p.e)
            {
                regs.al--;
                p.n = regs.al >= 0x80;
                p.z = regs.al == 0x00;
            }
            else
            {
                regs.a--;
                p.n = regs.a >= 0x8000;
                p.z = regs.a == 0x0000;
            }
        }

        private void op_dex_i()
        {
            if (p.x || p.e)
            {
                regs.xl--;
                p.n = regs.xl >= 0x80;
                p.z = regs.xl == 0x00;
            }
            else
            {
                regs.x--;
                p.n = regs.x >= 0x8000;
                p.z = regs.x == 0x0000;
            }
        }

        private void op_dey_i()
        {
            if (p.x || p.e)
            {
                regs.yl--;
                p.n = regs.yl >= 0x80;
                p.z = regs.yl == 0x00;
            }
            else
            {
                regs.y--;
                p.n = regs.y >= 0x8000;
                p.z = regs.y == 0x0000;
            }
        }

        private void op_inc_a()
        {
            if (p.m || p.e)
            {
                regs.al++;
                p.n = regs.al >= 0x80;
                p.z = regs.al == 0x00;
            }
            else
            {
                regs.a++;
                p.n = regs.a >= 0x8000;
                p.z = regs.a == 0x0000;
            }
        }

        private void op_inx_i()
        {
            if (p.x || p.e)
            {
                regs.xl++;
                p.n = regs.xl >= 0x80;
                p.z = regs.xl == 0x00;
            }
            else
            {
                regs.x++;
                p.n = regs.x >= 0x8000;
                p.z = regs.x == 0x0000;
            }
        }

        private void op_iny_i()
        {
            if (p.x || p.e)
            {
                regs.yl++;
                p.n = regs.yl >= 0x80;
                p.z = regs.yl == 0x00;
            }
            else
            {
                regs.y++;
                p.n = regs.y >= 0x8000;
                p.z = regs.y == 0x0000;
            }
        }

        private void op_lsr_a()
        {
            if (p.m || p.e)
            {
                regs.rdl = regs.al;
                op_lsr_b();
                regs.al = regs.rdl;
            }
            else
            {
                regs.rd = regs.a;
                op_lsr_w();
                regs.a = regs.rd;
            }
        }

        private void op_pea_i()
        {
            am_abs_w();

            Write(0, regs.sp--, regs.aah); 
            LastCycle();
            Write(0, regs.sp--, regs.aal);

            if (p.e)
            {
                regs.sph = 1;
            }
        }

        private void op_pei_i()
        {
            am_ind_w();
            Write(0, regs.sp--, regs.aah);
            Write(0, regs.sp--, regs.aal);

            if (p.e)
            {
                regs.sph = 1;
            }
        }

        private void op_per_i()
        {
            regs.rdl = Read(regs.pcb, regs.pc++);
            regs.rdh = Read(regs.pcb, regs.pc++);
            InternalOperation();
            regs.aa = (ushort)(regs.pc + regs.rd);
            Write(0, regs.sp--, regs.aah);
            Write(0, regs.sp--, regs.aal);

            if (p.e)
            {
                regs.sph = 1;
            }
        }

        private void op_pha_i()
        {
            InternalOperation();

            if (p.m || p.e)
            {
                LastCycle();
                Write(0, regs.sp--, regs.al); if (p.e) { regs.sph = 1; }
            }
            else
            {
                Write(0, regs.sp--, regs.ah);
                LastCycle();
                Write(0, regs.sp--, regs.al);
            }
        }

        private void op_phb_i()
        {
            InternalOperation();
            LastCycle();
            Write(0, regs.sp--, db);

            if (p.e)
            {
                regs.sph = 1;
            }
        }

        private void op_phd_i()
        {
            InternalOperation();
            Write(0, regs.sp--, regs.dph);
            LastCycle();
            Write(0, regs.sp--, regs.dpl);

            if (p.e)
            {
                regs.sph = 1;
            }
        }

        private void op_phk_i()
        {
            InternalOperation();
            LastCycle();
            Write(0, regs.sp--, regs.pcb);

            if (p.e)
            {
                regs.sph = 1;
            }
        }

        private void op_php_i()
        {
            InternalOperation();
            LastCycle();
            Write(0, regs.sp--, p.Pack());

            if (p.e)
            {
                regs.sph = 1;
            }
        }

        private void op_phx_i()
        {
            InternalOperation();

            if (p.x || p.e)
            {
                LastCycle();
                Write(0, regs.sp--, regs.xl);
            }
            else
            {
                Write(0, regs.sp--, regs.xh);
                LastCycle();
                Write(0, regs.sp--, regs.xl);
            }

            if (p.e)
            {
                regs.sph = 1;
            }
        }

        private void op_phy_i()
        {
            InternalOperation();

            if (p.x || p.e)
            {
                LastCycle();
                Write(0, regs.sp--, regs.yl);
            }
            else
            {
                Write(0, regs.sp--, regs.yh);
                LastCycle();
                Write(0, regs.sp--, regs.yl);
            }

            if (p.e)
            {
                regs.sph = 1;
            }
        }

        private void op_pla_i()
        {
            InternalOperation();
            InternalOperation();

            if (p.m || p.e)
            {
                LastCycle();
                regs.al = Read(0, ++regs.sp); if (p.e) regs.sph = 1;
                p.n = regs.al >= 0x80;
                p.z = regs.al == 0x00;
            }
            else
            {
                regs.al = Read(0, ++regs.sp);
                LastCycle();
                regs.ah = Read(0, ++regs.sp);
                p.n = regs.a >= 0x8000;
                p.z = regs.a == 0x0000;
            }
        }

        private void op_plb_i()
        {
            InternalOperation();
            InternalOperation();

            LastCycle();
            db = Read(0, ++regs.sp);
            p.n = db >= 0x80;
            p.z = db == 0x00;

            if (p.e)
            {
                regs.sph = 1;
            }
        }

        private void op_pld_i()
        {
            InternalOperation();
            InternalOperation();
            regs.dpl = Read(0, ++regs.sp);
            LastCycle();
            regs.dph = Read(0, ++regs.sp);
            p.n = regs.dp >= 0x8000;
            p.z = regs.dp == 0x0000;

            if (p.e)
            {
                regs.sph = 1;
            }
        }

        private void op_plp_i()
        {
            InternalOperation();
            InternalOperation();
            LastCycle();

            if (p.e)
            {
                regs.spl++; p.Unpack(Read(0, regs.sp));
            }
            else
            {
                regs.sp++; p.Unpack(Read(0, regs.sp));
            }

            if (p.x || p.e)
            {
                regs.xh = 0;
                regs.yh = 0;
            }
        }

        private void op_plx_i()
        {
            InternalOperation();
            InternalOperation();

            if (p.x || p.e)
            {
                LastCycle();
                regs.xl = Read(0, ++regs.sp);
                p.n = regs.xl >= 0x80;
                p.z = regs.xl == 0x00;
            }
            else
            {
                regs.xl = Read(0, ++regs.sp);
                LastCycle();
                regs.xh = Read(0, ++regs.sp);
                p.n = regs.x >= 0x8000;
                p.z = regs.x == 0x0000;
            }

            if (p.e)
            {
                regs.sph = 1;
            }
        }

        private void op_ply_i()
        {
            InternalOperation();
            InternalOperation();

            if (p.x || p.e)
            {
                LastCycle();
                regs.yl = Read(0, ++regs.sp);
                p.n = regs.yl >= 0x80;
                p.z = regs.yl == 0x00;
            }
            else
            {
                regs.yl = Read(0, ++regs.sp);
                LastCycle();
                regs.yh = Read(0, ++regs.sp);
                p.n = regs.y >= 0x8000;
                p.z = regs.y == 0x0000;
            }

            if (p.e)
            {
                regs.sph = 1;
            }
        }

        private void op_rep_i()
        {
            regs.rdl = Read(regs.pcb, regs.pc++);

            LastCycle();
            InternalOperation();

            if ((regs.rdl & 0x80) != 0) { p.n = false; }
            if ((regs.rdl & 0x40) != 0) { p.v = false; }
            if ((regs.rdl & 0x20) != 0) { p.m = false; }
            if ((regs.rdl & 0x10) != 0) { p.x = false; }
            if ((regs.rdl & 0x08) != 0) { p.d = false; }
            if ((regs.rdl & 0x04) != 0) { p.i = false; }
            if ((regs.rdl & 0x02) != 0) { p.z = false; }
            if ((regs.rdl & 0x01) != 0) { p.c = false; }
        }

        private void op_rol_a()
        {
            if (p.m || p.e)
            {
                regs.rdl = regs.al;
                op_rol_b();
                regs.al = regs.rdl;
            }
            else
            {
                regs.rd = regs.a;
                op_rol_w();
                regs.a = regs.rd;
            }
        }

        private void op_ror_a()
        {
            if (p.m || p.e)
            {
                regs.rdl = regs.al;
                op_ror_b();
                regs.al = regs.rdl;
            }
            else
            {
                regs.rd = regs.a;
                op_ror_w();
                regs.a = regs.rd;
            }
        }

        private void op_rti_i()
        {
            InternalOperation();
            InternalOperation();

            if (p.e)
            {
                regs.spl++; p.Unpack(Read(0, regs.sp));
                regs.spl++; regs.pcl = (Read(0, regs.sp));
                LastCycle();
                regs.spl++; regs.pch = (Read(0, regs.sp));
            }
            else
            {
                regs.sp++; p.Unpack(Read(0, regs.sp));
                regs.sp++; regs.pcl = (Read(0, regs.sp));
                regs.sp++; regs.pch = (Read(0, regs.sp));
                LastCycle();
                regs.sp++; regs.pcb = (Read(0, regs.sp));
            }
        }

        private void op_rtl_i()
        {
            InternalOperation();
            InternalOperation();
            regs.pcl = Read(0, ++regs.sp);
            regs.pch = Read(0, ++regs.sp);
            LastCycle();
            regs.pcb = Read(0, ++regs.sp);
            regs.pc++;

            if (p.e)
            {
                regs.sph = 1;
            }
        }

        private void op_rts_i()
        {
            InternalOperation();
            InternalOperation();
            regs.pcl = Read(0, ++regs.sp); if (p.e) { regs.sph = 1; }
            regs.pch = Read(0, ++regs.sp); if (p.e) { regs.sph = 1; }
            LastCycle();
            InternalOperation();
            regs.pc++;
        }

        private void op_sec_i() { p.c = true; }
        private void op_sed_i() { p.d = true; }
        private void op_sei_i() { p.i = true; }

        private void op_sep_i()
        {
            regs.rdl = Read(regs.pcb, regs.pc++);

            LastCycle();
            InternalOperation();

            if ((regs.rdl & 0x80) != 0) { p.n = true; }
            if ((regs.rdl & 0x40) != 0) { p.v = true; }
            if ((regs.rdl & 0x20) != 0) { p.m = true; }
            if ((regs.rdl & 0x10) != 0) { p.x = true; regs.xh = 0; regs.yh = 0; }
            if ((regs.rdl & 0x08) != 0) { p.d = true; }
            if ((regs.rdl & 0x04) != 0) { p.i = true; }
            if ((regs.rdl & 0x02) != 0) { p.z = true; }
            if ((regs.rdl & 0x01) != 0) { p.c = true; }
        }

        private void op_tax_i()
        {
            if (p.x || p.e)
            {
                regs.xl = regs.al;
                p.n = regs.xl >= 0x80;
                p.z = regs.xl == 0x00;
            }
            else
            {
                regs.x = regs.a;
                p.n = regs.x >= 0x8000;
                p.z = regs.x == 0x0000;
            }
        }

        private void op_tay_i()
        {
            if (p.x || p.e)
            {
                regs.yl = regs.al;
                p.n = regs.yl >= 0x80;
                p.z = regs.yl == 0x00;
            }
            else
            {
                regs.y = regs.a;
                p.n = regs.y >= 0x8000;
                p.z = regs.y == 0x0000;
            }
        }

        private void op_tcd_i()
        {
            regs.dp = regs.a;
            p.n = regs.dp >= 0x8000;
            p.z = regs.dp == 0x0000;
        }

        private void op_tcs_i()
        {
            regs.sp = regs.a;

            if (p.e)
            {
                regs.sph = 1;
            }
        }

        private void op_tdc_i()
        {
            regs.a = regs.dp;
            p.n = regs.a >= 0x8000;
            p.z = regs.a == 0x0000;
        }

        private void op_tsc_i()
        {
            regs.a = regs.sp;
            p.n = regs.a >= 0x8000;
            p.z = regs.a == 0x0000;
        }

        private void op_tsx_i()
        {
            if (p.x || p.e)
            {
                regs.xl = regs.spl;
                p.n = regs.xl >= 0x80;
                p.z = regs.xl == 0x00;
            }
            else
            {
                regs.x = regs.sp;
                p.n = regs.x >= 0x8000;
                p.z = regs.x == 0x0000;
            }
        }

        private void op_txa_i()
        {
            if (p.m || p.e)
            {
                regs.al = regs.xl;
                p.n = regs.al >= 0x80;
                p.z = regs.al == 0x00;
            }
            else
            {
                regs.a = regs.x;
                p.n = regs.a >= 0x8000;
                p.z = regs.a == 0x0000;
            }
        }

        private void op_txs_i()
        {
            if (p.e)
            {
                regs.spl = regs.xl;
            }
            else
            {
                regs.sp = regs.x;
            }
        }

        private void op_txy_i()
        {
            if (p.x || p.e)
            {
                regs.yl = regs.xl;
                p.n = regs.yl >= 0x80;
                p.z = regs.yl == 0x00;
            }
            else
            {
                regs.y = regs.x;
                p.n = regs.y >= 0x8000;
                p.z = regs.y == 0x0000;
            }
        }

        private void op_tya_i()
        {
            if (p.m || p.e)
            {
                regs.al = regs.yl;
                p.n = regs.al >= 0x80;
                p.z = regs.al == 0x00;
            }
            else
            {
                regs.a = regs.y;
                p.n = regs.a >= 0x8000;
                p.z = regs.a == 0x0000;
            }
        }

        private void op_tyx_i()
        {
            if (p.x || p.e)
            {
                regs.xl = regs.yl;
                p.n = regs.xl >= 0x80;
                p.z = regs.xl == 0x00;
            }
            else
            {
                regs.x = regs.y;
                p.n = regs.x >= 0x8000;
                p.z = regs.x == 0x0000;
            }
        }

        private void op_xba_i()
        {
            InternalOperation();
            LastCycle();
            InternalOperation();

            regs.al ^= regs.ah;
            regs.ah ^= regs.al;
            regs.al ^= regs.ah;
            p.n = regs.al >= 0x80;
            p.z = regs.al == 0x00;
        }

        private void op_xce_i()
        {
            p.e ^= p.c;
            p.c ^= p.e;
            p.e ^= p.c;
        }
    }
}
