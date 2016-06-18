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
                result = a.l + rd.l + (p.c ? 1 : 0);
            }
            else
            {
                result = (a.l & 0x0f) + (rd.l & 0x0f) + (p.c ? 0x01 : 0);
                if (result > 0x09) result += 0x06;
                p.c = result > 0x0f;
                result = (a.l & 0xf0) + (rd.l & 0xf0) + (p.c ? 0x10 : 0) + (result & 0x0f);
            }

            p.v = (~(a.l ^ rd.l) & (a.l ^ result) & 0x80) != 0;
            if (p.d && result > 0x9f) result += 0x60;
            p.c = result > 0xff;
            p.n = (byte)result >= 0x80;
            p.z = (byte)result == 0;

            a.l = (byte)result;
        }

        void op_adc_w()
        {
            int result;

            if (!p.d)
            {
                result = a.w + rd.w + (p.c ? 1 : 0);
            }
            else
            {
                result = (a.w & 0x000f) + (rd.w & 0x000f) + (p.c ? 0x0001 : 0);
                if (result > 0x0009) result += 0x0006;
                p.c = result > 0x000f;
                result = (a.w & 0x00f0) + (rd.w & 0x00f0) + (p.c ? 0x0010 : 0) + (result & 0x000f);
                if (result > 0x009f) result += 0x0060;
                p.c = result > 0x00ff;
                result = (a.w & 0x0f00) + (rd.w & 0x0f00) + (p.c ? 0x0100 : 0) + (result & 0x00ff);
                if (result > 0x09ff) result += 0x0600;
                p.c = result > 0x0fff;
                result = (a.w & 0xf000) + (rd.w & 0xf000) + (p.c ? 0x1000 : 0) + (result & 0x0fff);
            }

            p.v = (~(a.w ^ rd.w) & (a.w ^ result) & 0x8000) != 0;
            if (p.d && result > 0x9fff) result += 0x6000;
            p.c = result > 0xffff;
            p.n = (word)result >= 0x8000;
            p.z = (word)result == 0x0000;

            a.w = (word)result;
        }

        void op_and_b()
        {
            a.l &= rd.l;
            p.n = a.l >= 0x80;
            p.z = a.l == 0;
        }

        void op_and_w()
        {
            a.w &= rd.w;
            p.n = a.w >= 0x8000;
            p.z = a.w == 0;
        }

        void op_bit_b()
        {
            p.n = (rd.l & 0x80) != 0;
            p.v = (rd.l & 0x40) != 0;
            p.z = (rd.l & a.l) == 0;
        }

        void op_bit_w()
        {
            p.n = (rd.w & 0x8000) != 0;
            p.v = (rd.w & 0x4000) != 0;
            p.z = (rd.w & a.w) == 0;
        }

        void op_cmp_b()
        {
            int r = a.l - rd.l;
            p.n = (r & 0x80) != 0;
            p.z = (byte)r == 0;
            p.c = r >= 0;
        }

        void op_cmp_w()
        {
            int r = a.w - rd.w;
            p.n = (r & 0x8000) != 0;
            p.z = (word)r == 0;
            p.c = r >= 0;
        }

        void op_cpx_b()
        {
            int r = x.l - rd.l;
            p.n = (r & 0x80) != 0;
            p.z = (byte)r == 0;
            p.c = r >= 0;
        }

        void op_cpx_w()
        {
            int r = x.w - rd.w;
            p.n = (r & 0x8000) != 0;
            p.z = (word)r == 0;
            p.c = r >= 0;
        }

        void op_cpy_b()
        {
            int r = y.l - rd.l;
            p.n = (r & 0x80) != 0;
            p.z = (byte)r == 0;
            p.c = r >= 0;
        }

        void op_cpy_w()
        {
            int r = y.w - rd.w;
            p.n = (r & 0x8000) != 0;
            p.z = (word)r == 0;
            p.c = r >= 0;
        }

        void op_eor_b()
        {
            a.l ^= rd.l;
            p.n = a.l >= 0x80;
            p.z = a.l == 0;
        }

        void op_eor_w()
        {
            a.w ^= rd.w;
            p.n = a.w >= 0x8000;
            p.z = a.w == 0;
        }

        void op_lda_b()
        {
            a.l = rd.l;
            p.n = a.l >= 0x80;
            p.z = a.l == 0;
        }

        void op_lda_w()
        {
            a.w = rd.w;
            p.n = a.w >= 0x8000;
            p.z = a.w == 0;
        }

        void op_ldx_b()
        {
            x.l = rd.l;
            p.n = x.l >= 0x80;
            p.z = x.l == 0;
        }

        void op_ldx_w()
        {
            x.w = rd.w;
            p.n = x.w >= 0x8000;
            p.z = x.w == 0;
        }

        void op_ldy_b()
        {
            y.l = rd.l;
            p.n = y.l >= 0x80;
            p.z = y.l == 0;
        }

        void op_ldy_w()
        {
            y.w = rd.w;
            p.n = y.w >= 0x8000;
            p.z = y.w == 0;
        }

        void op_ora_b()
        {
            a.l |= rd.l;
            p.n = a.l >= 0x80;
            p.z = a.l == 0;
        }

        void op_ora_w()
        {
            a.w |= rd.w;
            p.n = a.w >= 0x8000;
            p.z = a.w == 0;
        }

        void op_sbc_b()
        {
            int result;
            rd.l ^= 0xff;

            if (!p.d)
            {
                result = a.l + rd.l + (p.c ? 1 : 0);
            }
            else
            {
                result = (a.l & 0x0f) + (rd.l & 0x0f) + (p.c ? 0x01 : 0);
                if (result <= 0x0f) result -= 0x06;
                p.c = result > 0x0f;
                result = (a.l & 0xf0) + (rd.l & 0xf0) + (p.c ? 0x10 : 0) + (result & 0x0f);
            }

            p.v = (~(a.l ^ rd.l) & (a.l ^ result) & 0x80) != 0;
            if (p.d && result <= 0xff) result -= 0x60;
            p.c = result > 0xff;
            p.n = (byte)result >= 0x80;
            p.z = (byte)result == 0;

            a.l = (byte)result;
        }

        void op_sbc_w()
        {
            int result;
            rd.w ^= 0xffff;

            if (!p.d)
            {
                result = a.w + rd.w + (p.c ? 1 : 0);
            }
            else
            {
                result = (a.w & 0x000f) + (rd.w & 0x000f) + (p.c ? 0x0001 : 0);                     if (result <= 0x000f) result -= 0x0006; p.c = result > 0x000f;
                result = (a.w & 0x00f0) + (rd.w & 0x00f0) + (p.c ? 0x0010 : 0) + (result & 0x000f); if (result <= 0x00ff) result -= 0x0060; p.c = result > 0x00ff;
                result = (a.w & 0x0f00) + (rd.w & 0x0f00) + (p.c ? 0x0100 : 0) + (result & 0x00ff); if (result <= 0x0fff) result -= 0x0600; p.c = result > 0x0fff;
                result = (a.w & 0xf000) + (rd.w & 0xf000) + (p.c ? 0x1000 : 0) + (result & 0x0fff);
            }

            p.v = (~(a.w ^ rd.w) & (a.w ^ result) & 0x8000) != 0;
            if (p.d && result <= 0xffff) result -= 0x6000;
            p.c = result > 0xffff;
            p.n = (word)result >= 0x8000;
            p.z = (word)result == 0;

            a.w = (word)result;
        }

        void op_inc_b()
        {
            rd.l++;
            p.n = rd.l >= 0x80;
            p.z = rd.l == 0;
        }

        void op_inc_w()
        {
            rd.w++;
            p.n = rd.w >= 0x8000;
            p.z = rd.w == 0;
        }

        void op_dec_b()
        {
            rd.l--;
            p.n = rd.l >= 0x80;
            p.z = rd.l == 0;
        }

        void op_dec_w()
        {
            rd.w--;
            p.n = rd.w >= 0x8000;
            p.z = rd.w == 0;
        }

        void op_asl_b()
        {
            p.c = (rd.l & 0x80) != 0;
            rd.l <<= 1;
            p.n = rd.l >= 0x80;
            p.z = rd.l == 0;
        }

        void op_asl_w()
        {
            p.c = (rd.w & 0x8000) != 0;
            rd.w <<= 1;
            p.n = rd.w >= 0x8000;
            p.z = rd.w == 0;
        }

        void op_lsr_b()
        {
            p.c = (rd.l & 1) != 0;
            rd.l >>= 1;
            p.n = rd.l >= 0x80;
            p.z = rd.l == 0;
        }

        void op_lsr_w()
        {
            p.c = (rd.w & 1) != 0;
            rd.w >>= 1;
            p.n = rd.w >= 0x8000;
            p.z = rd.w == 0;
        }

        void op_rol_b()
        {
            var carry = (byte)(p.c ? 0x01 : 0);
            p.c = (rd.l & 0x80) != 0;
            rd.l <<= 1;
            rd.l |= carry;
            p.n = rd.l >= 0x80;
            p.z = rd.l == 0;
        }

        void op_rol_w()
        {
            var carry = (word)(p.c ? 0x0001 : 0);
            p.c = (rd.w & 0x8000) != 0;
            rd.w <<= 1;
            rd.w |= carry;
            p.n = rd.w >= 0x8000;
            p.z = rd.w == 0;
        }

        void op_ror_b()
        {
            var carry = (byte)(p.c ? 0x80 : 0);
            p.c = (rd.l & 0x01) != 0;
            rd.l >>= 1;
            rd.l |= carry;
            p.n = rd.l >= 0x80;
            p.z = rd.l == 0;
        }

        void op_ror_w()
        {
            var carry = (word)(p.c ? 0x8000 : 0);
            p.c = (rd.w & 0x0001) != 0;
            rd.w >>= 1;
            rd.w |= carry;
            p.n = rd.w >= 0x8000;
            p.z = rd.w == 0;
        }

        void op_sta_b() { rd.l = a.l; }

        void op_sta_w() { rd.w = a.w; }

        void op_stx_b() { rd.l = x.l; }

        void op_stx_w() { rd.w = x.w; }

        void op_sty_b() { rd.l = y.l; }

        void op_sty_w() { rd.w = y.w; }

        void op_stz_b() { rd.l = 0; }

        void op_stz_w() { rd.w = 0; }

        void op_trb_b()
        {
            p.z = (rd.l & a.l) == 0;
            rd.l &= (byte) ~a.l;
        }

        void op_trb_w()
        {
            p.z = (rd.w & a.w) == 0;
            rd.w &= (word) ~a.w;
        }

        void op_tsb_b()
        {
            p.z = (rd.l & a.l) == 0;
            rd.l |= a.l;
        }

        void op_tsb_w()
        {
            p.z = (rd.w & a.w) == 0;
            rd.w |= a.w;
        }
    }
}
