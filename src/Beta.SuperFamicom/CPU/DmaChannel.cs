using Beta.Platform;

namespace Beta.SuperFamicom.CPU
{
    public sealed class DmaChannel
    {
        public Register16 Count;
        public Register24 AddressA;
        public byte AddressB;
        public byte Control;
    }
}
