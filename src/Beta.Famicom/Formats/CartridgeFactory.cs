using System.IO;
using System.Linq;
using Beta.Famicom.Database;
using Beta.Famicom.Memory;

namespace Beta.Famicom.Formats
{
    public static class CartridgeFactory
    {
        public static CartridgeImage Create(byte[] binary)
        {
            var board = DatabaseService.Find(binary);

            var stream = new MemoryStream(binary);
            var reader = new BinaryReader(stream);

            // skip iNES header
            stream.Seek(16L, SeekOrigin.Begin);

            var prgRoms = board.Prg.Select(e => MemoryFactory.CreateRom(reader.ReadBytes(e.Size)));
            var chrRoms = board.Chr.Select(e => MemoryFactory.CreateRom(reader.ReadBytes(e.Size)));
            var chrRams = board.Vram.Select(e => MemoryFactory.CreateRam(e.Size));

            var wram = board.Wram.Select(e => MemoryFactory.CreateRam(e.Size));
            var vram = board.Vram.Select(e => MemoryFactory.CreateRam(e.Size));

            return new CartridgeImage
            {
                prg = prgRoms.First(),
                chr = chrRoms.Concat(chrRams).First(),
                wram = wram.FirstOrDefault(),
                vram = vram.FirstOrDefault(),
                mapper = board.Type,
                h = board.SolderPad?.H ?? 0,
                v = board.SolderPad?.V ?? 0
            };
        }
    }
}
