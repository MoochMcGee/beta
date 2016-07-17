using Beta.Famicom.Formats;

namespace Beta.Famicom.Boards.Discreet
{
    [BoardName("NES-UNROM")]
    public class Unrom : Board
    {
        private int[] prgPage;

        public Unrom(CartridgeImage image)
            : base(image)
        {
            prgPage = new int[2];
            prgPage[0] = 0 << 14;
            prgPage[1] = 7 << 14;
        }

        protected override int DecodePrg(ushort address)
        {
            return (address & 0x3fff) | prgPage[(address >> 14) & 1];
        }

        protected override void WritePrg(ushort address, byte data)
        {
            prgPage[0] = (data & 0x07) << 14;
        }
    }
}
