using System.IO;
using System.Linq;
using Beta.Famicom.Database;
using Beta.Famicom.Memory;

namespace Beta.Famicom.Formats
{
    public static class CartridgeFactory
    {
        public static CartridgeImage create(byte[] binary)
        {
            var board = DatabaseService.find(binary);

            var stream = new MemoryStream(binary);
            var reader = new BinaryReader(stream);

            // skip iNES header
            stream.Seek(16L, SeekOrigin.Begin);

            var prgRoms = board.prg.Select(e => MemoryFactory.createRom(reader.ReadBytes(e.size)));
            var chrRoms = board.chr.Select(e => MemoryFactory.createRom(reader.ReadBytes(e.size)));
            var chrRams = board.vram.Select(e => MemoryFactory.createRam(e.size));

            var wram = board.wram.Select(e => MemoryFactory.createRam(e.size));
            var vram = board.vram.Select(e => MemoryFactory.createRam(e.size));

            return new CartridgeImage
            {
                prg = prgRoms.First(),
                chr = chrRoms.Concat(chrRams).First(),
                wram = wram.FirstOrDefault(),
                vram = vram.FirstOrDefault(),
                mapper = board.type,
                h = board.solderPad?.h ?? 0,
                v = board.solderPad?.v ?? 0
            };
        }
    }
}
