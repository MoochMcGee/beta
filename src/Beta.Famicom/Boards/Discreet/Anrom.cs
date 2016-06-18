using Beta.Famicom.Formats;

namespace Beta.Famicom.Boards.Discreet
{
    [BoardName("NES-ANROM")]
    public class Anrom : Board
    {
        private int mirroring;
        private int prgPage;

        public Anrom(CartridgeImage image)
            : base(image)
        {
        }

        protected override int DecodePrg(ushort address)
        {
            return (address & 0x7fff) | prgPage;
        }

        protected override void PokePrg(ushort address, ref byte data)
        {
            mirroring = (data >> 4) & 1;
            prgPage = (data & 0x03) << 15;
        }

        public override int VRamA10(ushort address)
        {
            return mirroring;
        }
    }
}
