using Beta.Famicom.Formats;

namespace Beta.Famicom.Boards.Discreet
{
    [BoardName("NES-AOROM")]
    public class Aorom : Board
    {
        private int mirroring;
        private int prgPage;

        public Aorom(CartridgeImage image)
            : base(image)
        {
        }

        protected override int DecodePrg(ushort address)
        {
            return (address & 0x7fff) | prgPage;
        }

        protected override void WritePrg(ushort address, byte data)
        {
            mirroring = (data >> 4) & 1;
            prgPage = (data & 0x07) << 15;
        }

        public override int VRamA10(ushort address)
        {
            return mirroring;
        }
    }
}
