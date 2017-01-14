namespace Beta.Platform.Processors.ARM7
{
    public partial class Core
    {
        public uint ReadWord(uint address)
        {
            var value = Read(2, address & ~3U) & 0xffffffff;
            var shift = (int)((address & 3) * 8);

            return (value >> shift) | (value << (32 - shift));
        }

        public uint ReadHalf(uint address)
        {
            var value = Read(1, address & ~1U) & 0x0000ffff;
            var shift = (int)((address & 1) * 8);

            return (value >> shift) | (value << (32 - shift));
        }

        public uint ReadHalfSignExtended(uint address)
        {
            var value = Read(1, address & ~1U);

            if ((address & 1) == 1)
            {
                return (uint)(sbyte)(value >> 8);
            }
            else
            {
                return (uint)(short)(value);
            }
        }

        public uint ReadByte(uint address)
        {
            var value = Read(0, address & ~0U) & 0x000000ff;

            return value;
        }

        public uint ReadByteSignExtended(uint address)
        {
            var value = Read(0, address);

            return (uint)(sbyte)(value);
        }
    }
}
