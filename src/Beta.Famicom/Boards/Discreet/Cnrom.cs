using Beta.Famicom.Formats;

namespace Beta.Famicom.Boards.Discreet
{
    [BoardName("NES-CNROM")]
    public class Cnrom : Board
    {
        private int chrPage;

        public Cnrom(CartridgeImage image)
            : base(image)
        {
        }

        protected override int DecodeChr(ushort address)
        {
            return (address & 0x1fff) | chrPage;
        }

        protected override void PokePrg(ushort address, ref byte data)
        {
            chrPage = data << 13;
        }
    }
}
