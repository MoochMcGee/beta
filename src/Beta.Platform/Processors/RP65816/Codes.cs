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
                rd.l = Read(aa.b, aa.w);
                codeB();
            }
            else
            {
                rd.l = Read(aa.b, aa.w); aa.d++;
                LastCycle();
                rd.h = Read(aa.b, aa.w);
                codeW();
            }
        }

        private void op_m(bool flag, Action codeB, Action codeW)
        {
            if (p.m || p.e)
            {
                rd.l = Read(aa.b, aa.w);

                InternalOperation();
                codeB();

                LastCycle();
                Write(aa.b, aa.w, rd.l);
            }
            else
            {
                rd.l = Read(aa.b, aa.w); aa.d++;
                rd.h = Read(aa.b, aa.w);

                InternalOperation();
                codeW();

                Write(aa.b, aa.w, rd.h); aa.d--;
                LastCycle();
                Write(aa.b, aa.w, rd.l);
            }
        }

        private void op_w(bool flag, Action codeB, Action codeW)
        {
            if (flag || p.e)
            {
                codeB();
                LastCycle();
                Write(aa.b, aa.w, rd.l);
            }
            else
            {
                codeW();
                Write(aa.b, aa.w, rd.l); aa.d++;
                LastCycle();
                Write(aa.b, aa.w, rd.h);
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
            var dp = Read(pc.b, pc.w++);
            var sp = Read(pc.b, pc.w++);
            db = dp;
            rd.l = Read(sp, x.w);
            Write(dp, y.w, rd.l);
            InternalOperation();

            if (p.x || p.e)
            {
                x.l += (byte)adjust;
                y.l += (byte)adjust;
            }
            else
            {
                x.w += (ushort)adjust;
                y.w += (ushort)adjust;
            }

            LastCycle();
            InternalOperation();
            if (a.w != 0) pc.w -= 3;
            a.w--;
        }

        private void op_asl_a()
        {
            if (p.m || p.e)
            {
                rd.l = a.l;
                op_asl_b();
                a.l = rd.l;
            }
            else
            {
                rd.w = a.w;
                op_asl_w();
                a.w = rd.w;
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
                a.l--;
                p.n = a.l >= 0x80;
                p.z = a.l == 0x00;
            }
            else
            {
                a.w--;
                p.n = a.w >= 0x8000;
                p.z = a.w == 0x0000;
            }
        }

        private void op_dex_i()
        {
            if (p.x || p.e)
            {
                x.l--;
                p.n = x.l >= 0x80;
                p.z = x.l == 0x00;
            }
            else
            {
                x.w--;
                p.n = x.w >= 0x8000;
                p.z = x.w == 0x0000;
            }
        }

        private void op_dey_i()
        {
            if (p.x || p.e)
            {
                y.l--;
                p.n = y.l >= 0x80;
                p.z = y.l == 0x00;
            }
            else
            {
                y.w--;
                p.n = y.w >= 0x8000;
                p.z = y.w == 0x0000;
            }
        }

        private void op_inc_a()
        {
            if (p.m || p.e)
            {
                a.l++;
                p.n = a.l >= 0x80;
                p.z = a.l == 0x00;
            }
            else
            {
                a.w++;
                p.n = a.w >= 0x8000;
                p.z = a.w == 0x0000;
            }
        }

        private void op_inx_i()
        {
            if (p.x || p.e)
            {
                x.l++;
                p.n = x.l >= 0x80;
                p.z = x.l == 0x00;
            }
            else
            {
                x.w++;
                p.n = x.w >= 0x8000;
                p.z = x.w == 0x0000;
            }
        }

        private void op_iny_i()
        {
            if (p.x || p.e)
            {
                y.l++;
                p.n = y.l >= 0x80;
                p.z = y.l == 0x00;
            }
            else
            {
                y.w++;
                p.n = y.w >= 0x8000;
                p.z = y.w == 0x0000;
            }
        }

        private void op_lsr_a()
        {
            if (p.m || p.e)
            {
                rd.l = a.l;
                op_lsr_b();
                a.l = rd.l;
            }
            else
            {
                rd.w = a.w;
                op_lsr_w();
                a.w = rd.w;
            }
        }

        private void op_pea_i()
        {
            am_abs_w();

            Write(0, sp.w--, aa.h);
            LastCycle();
            Write(0, sp.w--, aa.l);

            if (p.e)
            {
                sp.h = 1;
            }
        }

        private void op_pei_i()
        {
            am_ind_w();
            Write(0, sp.w--, aa.h);
            Write(0, sp.w--, aa.l);

            if (p.e)
            {
                sp.h = 1;
            }
        }

        private void op_per_i()
        {
            rd.l = Read(pc.b, pc.w++);
            rd.h = Read(pc.b, pc.w++);
            InternalOperation();
            aa.w = (ushort)(pc.w + rd.w);
            Write(0, sp.w--, aa.h);
            Write(0, sp.w--, aa.l);

            if (p.e)
            {
                sp.h = 1;
            }
        }

        private void op_pha_i()
        {
            InternalOperation();

            if (p.m || p.e)
            {
                LastCycle();
                Write(0, sp.w--, a.l); if (p.e) { sp.h = 1; }
            }
            else
            {
                Write(0, sp.w--, a.h);
                LastCycle();
                Write(0, sp.w--, a.l);
            }
        }

        private void op_phb_i()
        {
            InternalOperation();
            LastCycle();
            Write(0, sp.w--, db);

            if (p.e)
            {
                sp.h = 1;
            }
        }

        private void op_phd_i()
        {
            InternalOperation();
            Write(0, sp.w--, dp.h);
            LastCycle();
            Write(0, sp.w--, dp.l);

            if (p.e)
            {
                sp.h = 1;
            }
        }

        private void op_phk_i()
        {
            InternalOperation();
            LastCycle();
            Write(0, sp.w--, pc.b);

            if (p.e)
            {
                sp.h = 1;
            }
        }

        private void op_php_i()
        {
            InternalOperation();
            LastCycle();
            Write(0, sp.w--, p.Pack());

            if (p.e)
            {
                sp.h = 1;
            }
        }

        private void op_phx_i()
        {
            InternalOperation();

            if (p.x || p.e)
            {
                LastCycle();
                Write(0, sp.w--, x.l);
            }
            else
            {
                Write(0, sp.w--, x.h);
                LastCycle();
                Write(0, sp.w--, x.l);
            }

            if (p.e)
            {
                sp.h = 1;
            }
        }

        private void op_phy_i()
        {
            InternalOperation();

            if (p.x || p.e)
            {
                LastCycle();
                Write(0, sp.w--, y.l);
            }
            else
            {
                Write(0, sp.w--, y.h);
                LastCycle();
                Write(0, sp.w--, y.l);
            }

            if (p.e)
            {
                sp.h = 1;
            }
        }

        private void op_pla_i()
        {
            InternalOperation();
            InternalOperation();

            if (p.m || p.e)
            {
                LastCycle();
                a.l = Read(0, ++sp.w); if (p.e) sp.h = 1;
                p.n = a.l >= 0x80;
                p.z = a.l == 0x00;
            }
            else
            {
                a.l = Read(0, ++sp.w);
                LastCycle();
                a.h = Read(0, ++sp.w);
                p.n = a.w >= 0x8000;
                p.z = a.w == 0x0000;
            }
        }

        private void op_plb_i()
        {
            InternalOperation();
            InternalOperation();

            LastCycle();
            db = Read(0, ++sp.w);
            p.n = db >= 0x80;
            p.z = db == 0x00;

            if (p.e)
            {
                sp.h = 1;
            }
        }

        private void op_pld_i()
        {
            InternalOperation();
            InternalOperation();
            dp.l = Read(0, ++sp.w);
            LastCycle();
            dp.h = Read(0, ++sp.w);
            p.n = dp.w >= 0x8000;
            p.z = dp.w == 0x0000;

            if (p.e)
            {
                sp.h = 1;
            }
        }

        private void op_plp_i()
        {
            InternalOperation();
            InternalOperation();
            LastCycle();

            if (p.e)
            {
                sp.l++; p.Unpack(Read(0, sp.w));
            }
            else
            {
                sp.w++; p.Unpack(Read(0, sp.w));
            }

            if (p.x || p.e)
            {
                x.h = 0;
                y.h = 0;
            }
        }

        private void op_plx_i()
        {
            InternalOperation();
            InternalOperation();

            if (p.x || p.e)
            {
                LastCycle();
                x.l = Read(0, ++sp.w);
                p.n = x.l >= 0x80;
                p.z = x.l == 0x00;
            }
            else
            {
                x.l = Read(0, ++sp.w);
                LastCycle();
                x.h = Read(0, ++sp.w);
                p.n = x.w >= 0x8000;
                p.z = x.w == 0x0000;
            }

            if (p.e)
            {
                sp.h = 1;
            }
        }

        private void op_ply_i()
        {
            InternalOperation();
            InternalOperation();

            if (p.x || p.e)
            {
                LastCycle();
                y.l = Read(0, ++sp.w);
                p.n = y.l >= 0x80;
                p.z = y.l == 0x00;
            }
            else
            {
                y.l = Read(0, ++sp.w);
                LastCycle();
                y.h = Read(0, ++sp.w);
                p.n = y.w >= 0x8000;
                p.z = y.w == 0x0000;
            }

            if (p.e)
            {
                sp.h = 1;
            }
        }

        private void op_rep_i()
        {
            rd.l = Read(pc.b, pc.w++);

            LastCycle();
            InternalOperation();

            if ((rd.l & 0x80) != 0) { p.n = false; }
            if ((rd.l & 0x40) != 0) { p.v = false; }
            if ((rd.l & 0x20) != 0) { p.m = false; }
            if ((rd.l & 0x10) != 0) { p.x = false; }
            if ((rd.l & 0x08) != 0) { p.d = false; }
            if ((rd.l & 0x04) != 0) { p.i = false; }
            if ((rd.l & 0x02) != 0) { p.z = false; }
            if ((rd.l & 0x01) != 0) { p.c = false; }
        }

        private void op_rol_a()
        {
            if (p.m || p.e)
            {
                rd.l = a.l;
                op_rol_b();
                a.l = rd.l;
            }
            else
            {
                rd.w = a.w;
                op_rol_w();
                a.w = rd.w;
            }
        }

        private void op_ror_a()
        {
            if (p.m || p.e)
            {
                rd.l = a.l;
                op_ror_b();
                a.l = rd.l;
            }
            else
            {
                rd.w = a.w;
                op_ror_w();
                a.w = rd.w;
            }
        }

        private void op_rti_i()
        {
            InternalOperation();
            InternalOperation();

            if (p.e)
            {
                sp.l++; p.Unpack(Read(0, sp.w));
                sp.l++; pc.l = (Read(0, sp.w));
                LastCycle();
                sp.l++; pc.h = (Read(0, sp.w));
            }
            else
            {
                sp.w++; p.Unpack(Read(0, sp.w));
                sp.w++; pc.l = (Read(0, sp.w));
                sp.w++; pc.h = (Read(0, sp.w));
                LastCycle();
                sp.w++; pc.b = (Read(0, sp.w));
            }
        }

        private void op_rtl_i()
        {
            InternalOperation();
            InternalOperation();
            pc.l = Read(0, ++sp.w);
            pc.h = Read(0, ++sp.w);
            LastCycle();
            pc.b = Read(0, ++sp.w);
            pc.w++;

            if (p.e)
            {
                sp.h = 1;
            }
        }

        private void op_rts_i()
        {
            InternalOperation();
            InternalOperation();
            pc.l = Read(0, ++sp.w); if (p.e) { sp.h = 1; }
            pc.h = Read(0, ++sp.w); if (p.e) { sp.h = 1; }
            LastCycle();
            InternalOperation();
            pc.w++;
        }

        private void op_sec_i() { p.c = true; }
        private void op_sed_i() { p.d = true; }
        private void op_sei_i() { p.i = true; }

        private void op_sep_i()
        {
            rd.l = Read(pc.b, pc.w++);

            LastCycle();
            InternalOperation();

            if ((rd.l & 0x80) != 0) { p.n = true; }
            if ((rd.l & 0x40) != 0) { p.v = true; }
            if ((rd.l & 0x20) != 0) { p.m = true; }
            if ((rd.l & 0x10) != 0) { p.x = true; x.h = 0; y.h = 0; }
            if ((rd.l & 0x08) != 0) { p.d = true; }
            if ((rd.l & 0x04) != 0) { p.i = true; }
            if ((rd.l & 0x02) != 0) { p.z = true; }
            if ((rd.l & 0x01) != 0) { p.c = true; }
        }

        private void op_tax_i()
        {
            if (p.x || p.e)
            {
                x.l = a.l;
                p.n = x.l >= 0x80;
                p.z = x.l == 0x00;
            }
            else
            {
                x.w = a.w;
                p.n = x.w >= 0x8000;
                p.z = x.w == 0x0000;
            }
        }

        private void op_tay_i()
        {
            if (p.x || p.e)
            {
                y.l = a.l;
                p.n = y.l >= 0x80;
                p.z = y.l == 0x00;
            }
            else
            {
                y.w = a.w;
                p.n = y.w >= 0x8000;
                p.z = y.w == 0x0000;
            }
        }

        private void op_tcd_i()
        {
            dp.w = a.w;
            p.n = dp.w >= 0x8000;
            p.z = dp.w == 0x0000;
        }

        private void op_tcs_i()
        {
            sp.w = a.w;

            if (p.e)
            {
                sp.h = 1;
            }
        }

        private void op_tdc_i()
        {
            a.w = dp.w;
            p.n = a.w >= 0x8000;
            p.z = a.w == 0x0000;
        }

        private void op_tsc_i()
        {
            a.w = sp.w;
            p.n = a.w >= 0x8000;
            p.z = a.w == 0x0000;
        }

        private void op_tsx_i()
        {
            if (p.x || p.e)
            {
                x.l = sp.l;
                p.n = x.l >= 0x80;
                p.z = x.l == 0x00;
            }
            else
            {
                x.w = sp.w;
                p.n = x.w >= 0x8000;
                p.z = x.w == 0x0000;
            }
        }

        private void op_txa_i()
        {
            if (p.m || p.e)
            {
                a.l = x.l;
                p.n = a.l >= 0x80;
                p.z = a.l == 0x00;
            }
            else
            {
                a.w = x.w;
                p.n = a.w >= 0x8000;
                p.z = a.w == 0x0000;
            }
        }

        private void op_txs_i()
        {
            if (p.e)
            {
                sp.l = x.l;
            }
            else
            {
                sp.w = x.w;
            }
        }

        private void op_txy_i()
        {
            if (p.x || p.e)
            {
                y.l = x.l;
                p.n = y.l >= 0x80;
                p.z = y.l == 0x00;
            }
            else
            {
                y.w = x.w;
                p.n = y.w >= 0x8000;
                p.z = y.w == 0x0000;
            }
        }

        private void op_tya_i()
        {
            if (p.m || p.e)
            {
                a.l = y.l;
                p.n = a.l >= 0x80;
                p.z = a.l == 0x00;
            }
            else
            {
                a.w = y.w;
                p.n = a.w >= 0x8000;
                p.z = a.w == 0x0000;
            }
        }

        private void op_tyx_i()
        {
            if (p.x || p.e)
            {
                x.l = y.l;
                p.n = x.l >= 0x80;
                p.z = x.l == 0x00;
            }
            else
            {
                x.w = y.w;
                p.n = x.w >= 0x8000;
                p.z = x.w == 0x0000;
            }
        }

        private void op_xba_i()
        {
            InternalOperation();
            LastCycle();
            InternalOperation();

            a.l ^= a.h;
            a.h ^= a.l;
            a.l ^= a.h;
            p.n = a.l >= 0x80;
            p.z = a.l == 0x00;
        }

        private void op_xce_i()
        {
            p.e ^= p.c;
            p.c ^= p.e;
            p.e ^= p.c;
        }
    }
}
