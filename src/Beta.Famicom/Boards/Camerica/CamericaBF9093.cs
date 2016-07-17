using Beta.Famicom.Abstractions;
using Beta.Famicom.Formats;

namespace Beta.Famicom.Boards.Camerica
{
    [BoardName("CAMERICA-BF9093")]
    public class CamericaBF9093 : Board
    {
        private int[] prgPages;
        private int mirroring;

        public CamericaBF9093(CartridgeImage image)
            : base(image)
        {
            prgPages = new int[2];
        }

        private void Poke8000(ushort address, byte data)
        {
            mirroring = (data >> 4) & 1;
        }

        private void PokeC000(ushort address, byte data)
        {
            prgPages[0] = data << 14;
        }

        protected override int DecodePrg(ushort address)
        {
            return prgPages[(address >> 14) & 1] | (address & 0x3fff);
        }

        public override void Initialize()
        {
            base.Initialize();

            prgPages[0] = +0 << 14;
            prgPages[1] = -1 << 14;
        }

        public override void MapToCpu(IBus bus)
        {
            base.MapToCpu(bus);

            bus.Map("10-- ---- ---- ----", writer: Poke8000);
            bus.Map("11-- ---- ---- ----", writer: PokeC000);
        }

        public override int VRamA10(ushort address)
        {
            return mirroring;
        }
    }
}
