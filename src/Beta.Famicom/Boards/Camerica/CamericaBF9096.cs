using Beta.Famicom.Abstractions;
using Beta.Famicom.Formats;

namespace Beta.Famicom.Boards.Camerica
{
    [BoardName("CAMERICA-BF9096")]
    public class CamericaBF9096 : Board
    {
        private int[] prgPages;

        public CamericaBF9096(CartridgeImage image)
            : base(image)
        {
            prgPages = new int[2];
        }

        private void Poke8000(ushort address, byte data)
        {
            prgPages[0] = (prgPages[0] & ~0x30000) | ((data & 0x18) << 13);
            prgPages[1] = (prgPages[1] & ~0x30000) | ((data & 0x18) << 13);
        }

        private void PokeC000(ushort address, byte data)
        {
            prgPages[0] = (prgPages[0] & ~0x0c000) | ((data & 0x03) << 14);
        }

        protected override int DecodePrg(ushort address)
        {
            return prgPages[(address >> 14) & 1] | (address & 0x3fff);
        }

        public override void Initialize()
        {
            base.Initialize();

            prgPages[0] = 0 << 14;
            prgPages[1] = 3 << 14;
        }

        public override void MapToCpu(IBus bus)
        {
            base.MapToCpu(bus);

            bus.Map("10-- ---- ---- ----", writer: Poke8000);
            bus.Map("11-- ---- ---- ----", writer: PokeC000);
        }
    }
}
