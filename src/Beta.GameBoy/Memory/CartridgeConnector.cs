using Beta.GameBoy.Boards;

namespace Beta.GameBoy.Memory
{
    public sealed class CartridgeConnector
    {
        private readonly BoardFactory factory;
        private readonly BIOS boot;
        private readonly Registers regs;

        private Board cart;

        public CartridgeConnector(BoardFactory factory, BIOS boot, Registers regs)
        {
            this.factory = factory;
            this.boot = boot;
            this.regs = regs;
        }

        public void InsertCartridge(byte[] cartridgeImage)
        {
            this.cart = factory.Create(cartridgeImage);
        }

        public byte Read(ushort address)
        {
            return (regs.boot_rom_enabled && address <= 0x00ff)
                ? boot.Read(address)
                : cart.Read(address)
                ;
        }

        public void Write(ushort address, byte data)
        {
            cart.Write(address, data);
        }
    }
}
