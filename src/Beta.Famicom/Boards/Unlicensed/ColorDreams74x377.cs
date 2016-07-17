using Beta.Famicom.Formats;

namespace Beta.Famicom.Boards.Unlicensed
{
    [BoardName("COLORDREAMS-74*377")]
    public class ColorDreams74x377 : Board
    {
        private int[] chrPage;
        private int[] prgPage;

        public ColorDreams74x377(CartridgeImage image)
            : base(image)
        {
            chrPage = new int[1];
            prgPage = new int[1];
        }

        protected override int DecodeChr(ushort address)
        {
            return (address & 0x1fff) | chrPage[0];
        }

        protected override int DecodePrg(ushort address)
        {
            return (address & 0x7fff) | prgPage[0];
        }

        protected override void WritePrg(ushort address, byte data)
        {
            chrPage[0] = (data & 0xf0) << 0x9;
            prgPage[0] = (data & 0x03) << 0xf;
        }
    }
}
