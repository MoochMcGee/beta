using Beta.Famicom.Boards.Discrete;
using Beta.Famicom.Boards.Konami;
using Beta.Famicom.Boards.Nintendo;
using Beta.Famicom.Formats;

namespace Beta.Famicom.Boards
{
    public static class BoardFactory
    {
        public static IBoard getBoard(byte[] binary)
        {
            var info = CartridgeFactory.Create(binary);
            var type = getBoardType(info.mapper);

            type.applyImage(info);

            return type;
        }

        static IBoard getBoardType(string boardType)
        {
            switch (boardType)
            {
            case "HVC-NROM-128": return new NROM();
            case "HVC-NROM-256": return new NROM();
            case "NES-NROM-128": return new NROM();
            case "NES-NROM-256": return new NROM();

            case "UNROM": return new UxROM();
            case "UOROM": return new UxROM();

            case "KONAMI-VRC-1": return new VRC1();
            case "KONAMI-VRC-2": return new VRC2();

            case "NES-S(.+)ROM": return new SxROM();
            case "NES-T(.+)ROM": return new TxROM(null, null);
            }
            
            return null;
        }
    }
}
