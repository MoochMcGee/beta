using Beta.GameBoy.APU;
using Beta.GameBoy.Boards;
using Beta.GameBoy.CPU;
using Beta.GameBoy.Memory;
using Beta.GameBoy.PPU;
using Beta.Platform.Core;
using Beta.Platform.Messaging;
using SimpleInjector;

namespace Beta.GameBoy
{
    public sealed class DriverFactory : IDriverFactory
    {
        private readonly Container container;
        private readonly IBoardFactory boardFactory;
        private readonly ISubscriptionBroker broker;

        public DriverFactory(Container container, IBoardFactory boardFactory, ISubscriptionBroker broker)
        {
            this.container = container;
            this.boardFactory = boardFactory;
            this.broker = broker;
        }

        public IDriver Create(byte[] binary)
        {
            var result = new Driver();

            result.Board = boardFactory.Create(binary);
            result.Apu = container.GetInstance<Apu>();
            result.Cpu = container.GetInstance<Cpu>();
            result.Ppu = container.GetInstance<Ppu>();
            result.Pad = container.GetInstance<Pad>();
            result.Tma = container.GetInstance<Tma>();

            broker.Subscribe(result.Apu);
            broker.Subscribe(result.Cpu);
            broker.Subscribe(result.Pad);
            broker.Subscribe(result.Ppu);
            broker.Subscribe(result.Tma);

            var addressSpace = container.GetInstance<IAddressSpace>();
            var bios = container.GetInstance<Bios>();
            var wram = container.GetInstance<Wram>();
            var hram = container.GetInstance<Hram>();

            result.Board.Initialize();

            addressSpace.Map(0x0000, 0x00ff, bios.Read);
            addressSpace.Map(0xc000, 0xfdff, wram.Read, wram.Write);
            addressSpace.Map(0xff80, 0xfffe, hram.Read, hram.Write);

            return result;
        }
    }
}
