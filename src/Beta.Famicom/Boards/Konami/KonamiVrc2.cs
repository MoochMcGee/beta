using Beta.Platform.Exceptions;
using Beta.Famicom.Abstractions;
using Beta.Famicom.Formats;

namespace Beta.Famicom.Boards.Konami
{
    [BoardName("KONAMI-VRC-2")]
    public class KonamiVrc2 : Board
    {
        private int[] chrPages;
        private int[] prgPages;
        private int latch;
        private int mirroring;

        public KonamiVrc2(CartridgeImage image)
            : base(image)
        {
            chrPages = new int[8];
            prgPages = new int[4];
            prgPages[0] = +0 << 13;
            prgPages[1] = +0 << 13;
            prgPages[2] = -2 << 13;
            prgPages[3] = -1 << 13;
        }

        private void Peek6000(ushort address, ref byte data)
        {
            data &= 0xfe;
            data |= (byte)(latch & 1);
        }

        private void Poke6000(ushort address, byte data)
        {
            latch = data;
        }

        private void Poke8000(ushort address, byte data)
        {
            prgPages[0] = (data & 0x1f) << 13;
        }

        private void Poke9000(ushort address, byte data)
        {
            mirroring = (data & 0x03);
        }

        private void PokeA000(ushort address, byte data)
        {
            prgPages[1] = (data & 0x1f) << 13;
        }

        private void PokeB000(ushort address, byte data)
        {
            chrPages[0] = (chrPages[0] & ~0x03c00) | ((data & 0x0f) << 10);
        }

        private void PokeB001(ushort address, byte data)
        {
            chrPages[0] = (chrPages[0] & ~0x3c000) | ((data & 0x0f) << 14);
        }

        private void PokeB002(ushort address, byte data)
        {
            chrPages[1] = (chrPages[1] & ~0x03c00) | ((data & 0x0f) << 10);
        }

        private void PokeB003(ushort address, byte data)
        {
            chrPages[1] = (chrPages[1] & ~0x3c000) | ((data & 0x0f) << 14);
        }

        private void PokeC000(ushort address, byte data)
        {
            chrPages[2] = (chrPages[2] & ~0x03c00) | ((data & 0x0f) << 10);
        }

        private void PokeC001(ushort address, byte data)
        {
            chrPages[2] = (chrPages[2] & ~0x3c000) | ((data & 0x0f) << 14);
        }

        private void PokeC002(ushort address, byte data)
        {
            chrPages[3] = (chrPages[3] & ~0x03c00) | ((data & 0x0f) << 10);
        }

        private void PokeC003(ushort address, byte data)
        {
            chrPages[3] = (chrPages[3] & ~0x3c000) | ((data & 0x0f) << 14);
        }

        private void PokeD000(ushort address, byte data)
        {
            chrPages[4] = (chrPages[4] & ~0x03c00) | ((data & 0x0f) << 10);
        }

        private void PokeD001(ushort address, byte data)
        {
            chrPages[4] = (chrPages[4] & ~0x3c000) | ((data & 0x0f) << 14);
        }

        private void PokeD002(ushort address, byte data)
        {
            chrPages[5] = (chrPages[5] & ~0x03c00) | ((data & 0x0f) << 10);
        }

        private void PokeD003(ushort address, byte data)
        {
            chrPages[5] = (chrPages[5] & ~0x3c000) | ((data & 0x0f) << 14);
        }

        private void PokeE000(ushort address, byte data)
        {
            chrPages[6] = (chrPages[6] & ~0x03c00) | ((data & 0x0f) << 10);
        }

        private void PokeE001(ushort address, byte data)
        {
            chrPages[6] = (chrPages[6] & ~0x3c000) | ((data & 0x0f) << 14);
        }

        private void PokeE002(ushort address, byte data)
        {
            chrPages[7] = (chrPages[7] & ~0x03c00) | ((data & 0x0f) << 10);
        }

        private void PokeE003(ushort address, byte data)
        {
            chrPages[7] = (chrPages[7] & ~0x3c000) | ((data & 0x0f) << 14);
        }

        protected override int DecodeChr(ushort address)
        {
            return (address & 0x3ff) | chrPages[(address >> 10) & 7];
        }

        protected override int DecodePrg(ushort address)
        {
            return (address & 0x1fff) | prgPages[(address >> 13) & 3];
        }

        public override void MapToCpu(IBus bus)
        {
            base.MapToCpu(bus);

            bus.Map("0110 ---- ---- ----", reader: Peek6000, writer: Poke6000);

            var pin3 = 1 << int.Parse(GetPin("VRC2", 3).Replace("PRG A", ""));
            var pin4 = 1 << int.Parse(GetPin("VRC2", 4).Replace("PRG A", ""));

            bus.Map("1000 ---- ---- ----", writer: Poke8000);
            bus.Map("1001 ---- ---- ----", writer: Poke9000);
            bus.Map("1010 ---- ---- ----", writer: PokeA000);
            bus.Map("1011 ---- ---- --00", writer: PokeB000);
            bus.Map("1100 ---- ---- --00", writer: PokeC000);
            bus.Map("1101 ---- ---- --00", writer: PokeD000);
            bus.Map("1110 ---- ---- --00", writer: PokeE000);

            if (pin4 == 1)
            {
                bus.Map("1011 ---- ---- --01", writer: PokeB001);
                bus.Map("1100 ---- ---- --01", writer: PokeC001);
                bus.Map("1101 ---- ---- --01", writer: PokeD001);
                bus.Map("1110 ---- ---- --01", writer: PokeE001);
            }
            else
            {
                bus.Map("1011 ---- ---- --01", writer: PokeB002);
                bus.Map("1100 ---- ---- --01", writer: PokeC002);
                bus.Map("1101 ---- ---- --01", writer: PokeD002);
                bus.Map("1110 ---- ---- --01", writer: PokeE002);
            }

            if (pin3 == 2)
            {
                bus.Map("1011 ---- ---- --10", writer: PokeB002);
                bus.Map("1100 ---- ---- --10", writer: PokeC002);
                bus.Map("1101 ---- ---- --10", writer: PokeD002);
                bus.Map("1110 ---- ---- --10", writer: PokeE002);
            }
            else
            {
                bus.Map("1011 ---- ---- --10", writer: PokeB001);
                bus.Map("1100 ---- ---- --10", writer: PokeC001);
                bus.Map("1101 ---- ---- --10", writer: PokeD001);
                bus.Map("1110 ---- ---- --10", writer: PokeE001);
            }

            bus.Map("1011 ---- ---- --11", writer: PokeB003);
            bus.Map("1100 ---- ---- --11", writer: PokeC003);
            bus.Map("1101 ---- ---- --11", writer: PokeD003);
            bus.Map("1110 ---- ---- --11", writer: PokeE003);
        }

        public override int VRamA10(ushort address)
        {
            var x = (address >> 10) & 1;
            var y = (address >> 11) & 1;

            switch (mirroring)
            {
            case 0: return x;
            case 1: return y;
            case 2: return 0;
            case 3: return 1;
            }

            throw new CompilerPleasingException();
        }
    }
}
