using word = System.UInt16;

namespace Beta.Platform.Processors.RP65816
{
    public abstract partial class Core
    {
        void op_adc_b()
        {
            int result;

            if (!p.d)
            {
                result = regs.al + regs.rdl + (p.c ? 1 : 0);
            }
            else
            {
                result = (regs.al & 0x0f) + (regs.rdl & 0x0f) + (p.c ? 0x01 : 0);
                if (result > 0x09) result += 0x06;
                p.c = result > 0x0f;
                result = (regs.al & 0xf0) + (regs.rdl & 0xf0) + (p.c ? 0x10 : 0) + (result & 0x0f);
            }

            p.v = (~(regs.al ^ regs.rdl) & (regs.al ^ result) & 0x80) != 0;
            if (p.d && result > 0x9f) result += 0x60;
            p.c = result > 0xff;
            p.n = (byte)result >= 0x80;
            p.z = (byte)result == 0;

            regs.al = (byte)result;
        }

        void op_adc_w()
        {
            int result;

            if (!p.d)
            {
                result = regs.a + regs.rd + (p.c ? 1 : 0);
            }
            else
            {
                result = (regs.a & 0x000f) + (regs.rd & 0x000f) + (p.c ? 0x0001 : 0);
                if (result > 0x0009) result += 0x0006;
                p.c = result > 0x000f;
                result = (regs.a & 0x00f0) + (regs.rd & 0x00f0) + (p.c ? 0x0010 : 0) + (result & 0x000f);
                if (result > 0x009f) result += 0x0060;
                p.c = result > 0x00ff;
                result = (regs.a & 0x0f00) + (regs.rd & 0x0f00) + (p.c ? 0x0100 : 0) + (result & 0x00ff);
                if (result > 0x09ff) result += 0x0600;
                p.c = result > 0x0fff;
                result = (regs.a & 0xf000) + (regs.rd & 0xf000) + (p.c ? 0x1000 : 0) + (result & 0x0fff);
            }

            p.v = (~(regs.a ^ regs.rd) & (regs.a ^ result) & 0x8000) != 0;
            if (p.d && result > 0x9fff) result += 0x6000;
            p.c = result > 0xffff;
            p.n = (word)result >= 0x8000;
            p.z = (word)result == 0x0000;

            regs.a = (word)result;
        }

        void op_and_b()
        {
            regs.al &= regs.rdl;
            p.n = regs.al >= 0x80;
            p.z = regs.al == 0;
        }

        void op_and_w()
        {
            regs.a &= regs.rd;
            p.n = regs.a >= 0x8000;
            p.z = regs.a == 0;
        }

        void op_bit_b()
        {
            p.n = (regs.rdl & 0x80) != 0;
            p.v = (regs.rdl & 0x40) != 0;
            p.z = (regs.rdl & regs.al) == 0;
        }

        void op_bit_w()
        {
            p.n = (regs.rd & 0x8000) != 0;
            p.v = (regs.rd & 0x4000) != 0;
            p.z = (regs.rd & regs.a) == 0;
        }

        void op_cmp_b()
        {
            int r = regs.al - regs.rdl;
            p.n = (r & 0x80) != 0;
            p.z = (byte)r == 0;
            p.c = r >= 0;
        }

        void op_cmp_w()
        {
            int r = regs.a - regs.rd;
            p.n = (r & 0x8000) != 0;
            p.z = (word)r == 0;
            p.c = r >= 0;
        }

        void op_cpx_b()
        {
            int r = regs.xl - regs.rdl;
            p.n = (r & 0x80) != 0;
            p.z = (byte)r == 0;
            p.c = r >= 0;
        }

        void op_cpx_w()
        {
            int r = regs.x - regs.rd;
            p.n = (r & 0x8000) != 0;
            p.z = (word)r == 0;
            p.c = r >= 0;
        }

        void op_cpy_b()
        {
            int r = regs.yl - regs.rdl;
            p.n = (r & 0x80) != 0;
            p.z = (byte)r == 0;
            p.c = r >= 0;
        }

        void op_cpy_w()
        {
            int r = regs.y - regs.rd;
            p.n = (r & 0x8000) != 0;
            p.z = (word)r == 0;
            p.c = r >= 0;
        }

        void op_eor_b()
        {
            regs.al ^= regs.rdl;
            p.n = regs.al >= 0x80;
            p.z = regs.al == 0;
        }

        void op_eor_w()
        {
            regs.a ^= regs.rd;
            p.n = regs.a >= 0x8000;
            p.z = regs.a == 0;
        }

        void op_lda_b()
        {
            regs.al = regs.rdl;
            p.n = regs.al >= 0x80;
            p.z = regs.al == 0;
        }

        void op_lda_w()
        {
            regs.a = regs.rd;
            p.n = regs.a >= 0x8000;
            p.z = regs.a == 0;
        }

        void op_ldx_b()
        {
            regs.xl = regs.rdl;
            p.n = regs.xl >= 0x80;
            p.z = regs.xl == 0;
        }

        void op_ldx_w()
        {
            regs.x = regs.rd;
            p.n = regs.x >= 0x8000;
            p.z = regs.x == 0;
        }

        void op_ldy_b()
        {
            regs.yl = regs.rdl;
            p.n = regs.yl >= 0x80;
            p.z = regs.yl == 0;
        }

        void op_ldy_w()
        {
            regs.y = regs.rd;
            p.n = regs.y >= 0x8000;
            p.z = regs.y == 0;
        }

        void op_ora_b()
        {
            regs.al |= regs.rdl;
            p.n = regs.al >= 0x80;
            p.z = regs.al == 0;
        }

        void op_ora_w()
        {
            regs.a |= regs.rd;
            p.n = regs.a >= 0x8000;
            p.z = regs.a == 0;
        }

        void op_sbc_b()
        {
            int result;
            regs.rdl ^= 0xff;

            if (!p.d)
            {
                result = regs.al + regs.rdl + (p.c ? 1 : 0);
            }
            else
            {
                result = (regs.al & 0x0f) + (regs.rdl & 0x0f) + (p.c ? 0x01 : 0);
                if (result <= 0x0f) result -= 0x06;
                p.c = result > 0x0f;
                result = (regs.al & 0xf0) + (regs.rdl & 0xf0) + (p.c ? 0x10 : 0) + (result & 0x0f);
            }

            p.v = (~(regs.al ^ regs.rdl) & (regs.al ^ result) & 0x80) != 0;
            if (p.d && result <= 0xff) result -= 0x60;
            p.c = result > 0xff;
            p.n = (byte)result >= 0x80;
            p.z = (byte)result == 0;

            regs.al = (byte)result;
        }

        void op_sbc_w()
        {
            int result;
            regs.rd ^= 0xffff;

            if (!p.d)
            {
                result = regs.a + regs.rd + (p.c ? 1 : 0);
            }
            else
            {
                result = (regs.a & 0x000f) + (regs.rd & 0x000f) + (p.c ? 0x0001 : 0);                     if (result <= 0x000f) result -= 0x0006; p.c = result > 0x000f;
                result = (regs.a & 0x00f0) + (regs.rd & 0x00f0) + (p.c ? 0x0010 : 0) + (result & 0x000f); if (result <= 0x00ff) result -= 0x0060; p.c = result > 0x00ff;
                result = (regs.a & 0x0f00) + (regs.rd & 0x0f00) + (p.c ? 0x0100 : 0) + (result & 0x00ff); if (result <= 0x0fff) result -= 0x0600; p.c = result > 0x0fff;
                result = (regs.a & 0xf000) + (regs.rd & 0xf000) + (p.c ? 0x1000 : 0) + (result & 0x0fff);
            }

            p.v = (~(regs.a ^ regs.rd) & (regs.a ^ result) & 0x8000) != 0;
            if (p.d && result <= 0xffff) result -= 0x6000;
            p.c = result > 0xffff;
            p.n = (word)result >= 0x8000;
            p.z = (word)result == 0;

            regs.a = (word)result;
        }

        void op_inc_b()
        {
            regs.rdl++;
            p.n = regs.rdl >= 0x80;
            p.z = regs.rdl == 0;
        }

        void op_inc_w()
        {
            regs.rd++;
            p.n = regs.rd >= 0x8000;
            p.z = regs.rd == 0;
        }

        void op_dec_b()
        {
            regs.rdl--;
            p.n = regs.rdl >= 0x80;
            p.z = regs.rdl == 0;
        }

        void op_dec_w()
        {
            regs.rd--;
            p.n = regs.rd >= 0x8000;
            p.z = regs.rd == 0;
        }

        void op_asl_b()
        {
            p.c = (regs.rdl & 0x80) != 0;
            regs.rdl <<= 1;
            p.n = regs.rdl >= 0x80;
            p.z = regs.rdl == 0;
        }

        void op_asl_w()
        {
            p.c = (regs.rd & 0x8000) != 0;
            regs.rd <<= 1;
            p.n = regs.rd >= 0x8000;
            p.z = regs.rd == 0;
        }

        void op_lsr_b()
        {
            p.c = (regs.rdl & 1) != 0;
            regs.rdl >>= 1;
            p.n = regs.rdl >= 0x80;
            p.z = regs.rdl == 0;
        }

        void op_lsr_w()
        {
            p.c = (regs.rd & 1) != 0;
            regs.rd >>= 1;
            p.n = regs.rd >= 0x8000;
            p.z = regs.rd == 0;
        }

        void op_rol_b()
        {
            var carry = (byte)(p.c ? 0x01 : 0);
            p.c = (regs.rdl & 0x80) != 0;
            regs.rdl <<= 1;
            regs.rdl |= carry;
            p.n = regs.rdl >= 0x80;
            p.z = regs.rdl == 0;
        }

        void op_rol_w()
        {
            var carry = (word)(p.c ? 0x0001 : 0);
            p.c = (regs.rd & 0x8000) != 0;
            regs.rd <<= 1;
            regs.rd |= carry;
            p.n = regs.rd >= 0x8000;
            p.z = regs.rd == 0;
        }

        void op_ror_b()
        {
            var carry = (byte)(p.c ? 0x80 : 0);
            p.c = (regs.rdl & 0x01) != 0;
            regs.rdl >>= 1;
            regs.rdl |= carry;
            p.n = regs.rdl >= 0x80;
            p.z = regs.rdl == 0;
        }

        void op_ror_w()
        {
            var carry = (word)(p.c ? 0x8000 : 0);
            p.c = (regs.rd & 0x0001) != 0;
            regs.rd >>= 1;
            regs.rd |= carry;
            p.n = regs.rd >= 0x8000;
            p.z = regs.rd == 0;
        }

        void op_sta_b() { regs.rdl = regs.al; }

        void op_sta_w() { regs.rd = regs.a; }

        void op_stx_b() { regs.rdl = regs.xl; }

        void op_stx_w() { regs.rd = regs.x; }

        void op_sty_b() { regs.rdl = regs.yl; }

        void op_sty_w() { regs.rd = regs.y; }

        void op_stz_b() { regs.rdl = 0; }

        void op_stz_w() { regs.rd = 0; }

        void op_trb_b()
        {
            p.z = (regs.rdl & regs.al) == 0;
            regs.rdl &= (byte) ~regs.al;
        }

        void op_trb_w()
        {
            p.z = (regs.rd & regs.a) == 0;
            regs.rd &= (word) ~regs.a;
        }

        void op_tsb_b()
        {
            p.z = (regs.rdl & regs.al) == 0;
            regs.rdl |= regs.al;
        }

        void op_tsb_w()
        {
            p.z = (regs.rd & regs.a) == 0;
            regs.rd |= regs.a;
        }
    }
}
