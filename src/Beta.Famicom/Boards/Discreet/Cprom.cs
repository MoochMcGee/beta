using Beta.Famicom.Memory;
using Beta.Platform.Exceptions;
using Beta.Famicom.Formats;

namespace Beta.Famicom.Boards.Discreet
{
    [BoardName("NES-CPROM")]
    public class Cprom : Board
    {
        private int[] chrPages;
        private IMemory chr0;
        private IMemory chr1;

        public Cprom(CartridgeImage image)
            : base(image)
        {
            chrPages = new int[2];
        }

        protected override int DecodeChr(ushort address)
        {
            return (address & 0xfff) | chrPages[(address >> 12) & 1];
        }

        protected override void PeekChr(ushort address, ref byte data)
        {
            var addr = DecodeChr(address);

            switch (addr & 0x2000)
            {
            case 0x0000: chr0.Peek(addr, ref data); break;
            case 0x2000: chr1.Peek(addr, ref data); break;
            }
        }

        protected override void PokeChr(ushort address, ref byte data)
        {
            var addr = DecodeChr(address);

            switch (addr & 0x2000)
            {
            case 0x0000: chr0.Poke(addr, ref data); break;
            case 0x2000: chr1.Poke(addr, ref data); break;
            }

            throw new CompilerPleasingException();
        }

        protected override void PokePrg(ushort address, ref byte data)
        {
            chrPages[1] = (data & 0x03) << 12;
        }

        public override void Initialize()
        {
            base.Initialize();

            SelectChr(0);
            chr0 = Chr;

            SelectChr(1);
            chr1 = Chr;
        }
    }
}
