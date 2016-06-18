using System.IO;
using System.Linq;
using Beta.Famicom.Database;
using Beta.Famicom.Memory;

namespace Beta.Famicom.Formats
{
    public sealed class CartridgeFactory : ICartridgeFactory
    {
        private readonly IDatabase database;
        private readonly IMemoryFactory factory;

        public CartridgeFactory(IDatabase database, IMemoryFactory factory)
        {
            this.database = database;
            this.factory = factory;
        }

        public CartridgeImage Create(byte[] binary)
        {
            var board = database.Find(binary);

            var stream = new MemoryStream(binary);
            var reader = new BinaryReader(stream);

            // skip iNES header
            stream.Seek(16L, SeekOrigin.Begin);

            var prgRoms = board.Prg.Select(e => factory.CreateRom(reader.ReadBytes(e.Size)));
            var chrRoms = board.Chr.Select(e => factory.CreateRom(reader.ReadBytes(e.Size)));
            var chrRams = board.Vram.Select(e => factory.CreateRam(e.Size));

            var wram = board.Wram.Select(e => factory.CreateRam(e.Size));
            var vram = board.Vram.Select(e => factory.CreateRam(e.Size));

            return new CartridgeImage
            {
                PRG = prgRoms.ToArray(),
                CHR = chrRoms.Concat(chrRams).ToArray(),
                WRAM = wram.ToArray(),
                VRAM = vram.ToArray(),
                Chips = board.Chip.ToArray(),
                Mapper = board.Type,
                H = board.SolderPad?.H ?? 0,
                V = board.SolderPad?.V ?? 0
            };
        }
    }
}
