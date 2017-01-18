namespace Beta.Platform.Processors.ARM7
{
    public partial class Core
    {
        private static int MultiplierCycles(uint value)
        {
            if ((value & 0xffffff00) == 0 || (value & 0xffffff00) == 0xffffff00) return 1;
            if ((value & 0xffff0000) == 0 || (value & 0xffff0000) == 0xffff0000) return 2;
            if ((value & 0xff000000) == 0 || (value & 0xff000000) == 0xff000000) return 3;

            return 4;
        }

        private uint carry;

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
            cycles += MultiplierCycles(b);

            var result = (a * b) + c;

            if (cpsr.t != 0U || (code & (1 << 20)) != 0)
            {
                cpsr.n = result >> 31;
                cpsr.z = result == 0U ? 1U : 0U;
            }

            return result;
        }

        private uint Mov(uint value)
        {
            if (cpsr.t != 0U || (code & (1U << 20)) != 0)
            {
                cpsr.n = value >> 31;
                cpsr.z = value == 0U ? 1U : 0U;
                cpsr.c = carry;
            }

            return value;
        }

        private uint LSL(uint value, uint shift)
        {
            if (shift == 0)
            {
                carry = cpsr.c;
            }
            else
            {
                var s = ((int)shift);

                carry = shift > 32 ? 0 : (value >> (32 - s)) & 1U;
                value = shift > 31 ? 0 : (value << s);
            }

            return value;
        }

        private uint LSR(uint value, uint shift)
        {
            if (shift == 0)
            {
                carry = cpsr.c;
            }
            else
            {
                var s = ((int)shift);

                carry = shift > 32 ? 0 : (value >> (s - 1)) & 1U;
                value = shift > 31 ? 0 : (value >> s);
            }

            return value;
        }

        private uint ASR(uint value, uint shift)
        {
            if (shift == 0)
            {
                carry = cpsr.c;
            }
            else
            {
                var s = ((int)shift);
                var v = ((int)value);

                carry = (uint)(shift > 32 ? (v >> 31) : (v >> (s - 1))) & 1U;
                value = (uint)(shift > 31 ? (v >> 31) : (v >> s));
            }

            return value;
        }

        private uint ROR(uint value, uint shift)
        {
            if (shift == 0)
            {
                carry = cpsr.c;
            }
            else
            {
                var s = ((int)shift) & 31;

                carry = (value >> (s - 1)) & 1;
                value = (value >> s) | (value << (32 - s));
            }

            return value;
        }

        private uint RRX(uint value)
        {
            carry = value & 1U;
            return (value >> 1) | (cpsr.c << 31);
        }
    }
}
