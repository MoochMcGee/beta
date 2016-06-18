using Beta.Famicom.Formats;

namespace Beta.Famicom.Boards.Discreet
{
    [BoardName("HVC-NROM-256")]
    [BoardName("NES-NROM-256")]
    public class Nrom256 : Board
    {
        public Nrom256(CartridgeImage image)
            : base(image)
        {
        }

        protected override int DecodePrg(ushort address)
        {
            return (address & 0x7fff);
        }
    }
}
