using Beta.Famicom.Formats;

namespace Beta.Famicom.Boards.Discreet
{
    [BoardName("HVC-NROM-128")]
    [BoardName("NES-NROM-128")]
    public class Nrom128 : Board
    {
        public Nrom128(CartridgeImage image)
            : base(image)
        {
        }

        protected override int DecodePrg(ushort address)
        {
            return (address & 0x3fff);
        }
    }
}
