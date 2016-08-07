using System.Linq;
using Beta.Famicom.CPU;
using Beta.Famicom.Formats;
using Beta.Famicom.Memory;
using Beta.Famicom.Messaging;
using Beta.Famicom.PPU;
using Beta.Platform.Messaging;

namespace Beta.Famicom.Boards
{
    public abstract class Board
        : IConsumer<ClockSignal>
        , IConsumer<PpuAddressSignal>
    {
        private Database.Chip[] chips;
        private IMemory[] prgChips;
        private IMemory[] chrChips;
        private IMemory[] ramChips;
        private int h;
        private int v;

        public R2A03 Cpu;

        protected IMemory Chr;
        protected IMemory Prg;
        protected IMemory Ram;
        protected string Type;

        protected Board(CartridgeImage image)
        {
            chips = image.Chips;
            prgChips = image.PRG;
            chrChips = image.CHR;
            ramChips = image.WRAM;

            Type = image.Mapper;

            h = image.H;
            v = image.V;

            SelectPrg(0);
            SelectChr(0);
            SelectRam(0);
        }

        protected void SelectChr(int chip)
        {
            Chr = (chip < chrChips.Length) ? chrChips[chip] : null;
        }

        protected void SelectPrg(int chip)
        {
            Prg = (chip < prgChips.Length) ? prgChips[chip] : null;
        }

        protected void SelectRam(int chip)
        {
            Ram = (chip < ramChips.Length) ? ramChips[chip] : null;
        }

        protected string GetPin(string type, int number)
        {
            var linq = from chip in chips
                       where chip.Type == type
                       from pin in chip.Pins
                       where pin.Number == number
                       select pin.Function;

            return linq.First();
        }

        protected virtual int DecodeChr(ushort address)
        {
            return address;
        }

        protected virtual int DecodePrg(ushort address)
        {
            return address;
        }

        protected virtual void ReadChr(ushort address, ref byte data)
        {
            Chr.Read(DecodeChr(address), ref data);
        }

        protected virtual void ReadPrg(ushort address, ref byte data)
        {
            Prg.Read(DecodePrg(address), ref data);
        }

        protected virtual void ReadRam(ushort address, ref byte data)
        {
            Ram?.Read(address, ref data);
        }

        protected virtual void WriteChr(ushort address, byte data)
        {
            Chr.Write(DecodeChr(address), data);
        }

        protected virtual void WritePrg(ushort address, byte data)
        {
        }

        protected virtual void WriteRam(ushort address, byte data)
        {
            Ram?.Write(address, data);
        }

        public virtual void R2C02Read(ushort address, ref byte data) { }

        public virtual void R2C02Write(ushort address, byte data) { }

        public virtual void R2A03Read(ushort address, ref byte data) { }

        public virtual void R2A03Write(ushort address, byte data) { }

        public virtual void MapToCpu(R2A03Bus bus)
        {
            bus.Map("011- ---- ---- ----", ReadRam, WriteRam);
            bus.Map("1--- ---- ---- ----", ReadPrg, WritePrg);
        }

        public virtual void MapToPpu(R2C02Bus bus)
        {
            bus.Map("000- ---- ---- ----", ReadChr, WriteChr);
        }

        public virtual bool VRAM(ushort address, out int a10)
        {
            var x = (address >> 10) & h;
            var y = (address >> 11) & v;

            a10 = x | y;

            return true;
        }

        public virtual void Initialize() { }

        public virtual void Consume(ClockSignal e) { }

        public virtual void Consume(PpuAddressSignal e) { }
    }
}
