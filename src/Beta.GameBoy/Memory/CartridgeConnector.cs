using Beta.GameBoy.Boards;

namespace Beta.GameBoy.Memory
{
    public sealed class CartridgeConnector : ICartridgeConnector
    {
        private readonly IBoardFactory boardFactory;
        private readonly Bios boot;
        private readonly Registers regs;

        private Board cart;

        public CartridgeConnector(IBoardFactory boardFactory, Bios boot, Registers regs)
        {
            this.boardFactory = boardFactory;
            this.boot = boot;
            this.regs = regs;
        }

        public void InsertCartridge(byte[] cartridgeImage)
        {
            this.cart = boardFactory.Create(cartridgeImage);
        }

        public byte Read(ushort address)
        {
            if (regs.boot_rom_enabled && address <= 0x00ff)
            {
                return boot.Read(address);
            }
            else
            {
                return cart.Read(address);
            }
        }

        public void Write(ushort address, byte data)
        {
            cart.Write(address, data);
        }
    }
}
