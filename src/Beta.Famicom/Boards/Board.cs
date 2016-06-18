using System.Linq;
using Beta.Famicom.Abstractions;
using Beta.Famicom.CPU;
using Beta.Famicom.Memory;
using Beta.Famicom.Formats;

namespace Beta.Famicom.Boards
{
    public abstract class Board : IBoard
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

        protected virtual void PeekChr(ushort address, ref byte data)
        {
            Chr.Peek(DecodeChr(address), ref data);
        }

        protected virtual void PeekPrg(ushort address, ref byte data)
        {
            Prg.Peek(DecodePrg(address), ref data);
        }

        protected virtual void PeekRam(ushort address, ref byte data)
        {
            Ram?.Peek(address, ref data);
        }

        protected virtual void PokeChr(ushort address, ref byte data)
        {
            Chr.Poke(DecodeChr(address), ref data);
        }

        protected virtual void PokePrg(ushort address, ref byte data)
        {
        }

        protected virtual void PokeRam(ushort address, ref byte data)
        {
            Ram?.Poke(address, ref data);
        }

        public virtual void Clock()
        {
        }

        public virtual void CpuAddressUpdate(ushort address)
        {
        }

        public virtual void PpuAddressUpdate(ushort address)
        {
        }

        public virtual void MapToCpu(IBus bus)
        {
            bus.Decode("011- ---- ---- ----").Peek(PeekRam).Poke(PokeRam);
            bus.Decode("1--- ---- ---- ----").Peek(PeekPrg).Poke(PokePrg);
        }

        public virtual void MapToPpu(IBus bus)
        {
            bus.Decode("000- ---- ---- ----").Peek(PeekChr).Poke(PokeChr);
        }

        public virtual int VRamA10(ushort address)
        {
            var x = (address >> 10) & h;
            var y = (address >> 11) & v;

            return x | y;
        }

        public virtual void Initialize() { }

        public virtual void ResetSoft() { }

        public virtual void ResetHard() { }
    }
}
