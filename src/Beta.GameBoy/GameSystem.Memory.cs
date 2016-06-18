using System.IO;
using Beta.Platform.Core;

namespace Beta.GameBoy
{
    public partial class GameSystem
    {
        private Peek[] peeks = new Peek[1 << 16];
        private Poke[] pokes = new Poke[1 << 16];
        private byte[] bios;
        private byte[] hram = new byte[0x007f];
        private byte[] wram = new byte[0x2000];

        private byte PeekBios(uint address)
        {
            return bios[address & 0x00ffu];
        }

        private byte PeekHRam(uint address)
        {
            return hram[address & 0x007fu];
        }

        private byte PeekWRam(uint address)
        {
            return wram[address & 0x1fffu];
        }

        private void PokeHRam(uint address, byte data)
        {
            hram[address & 0x007fu] = data;
        }

        private void PokeWRam(uint address, byte data)
        {
            wram[address & 0x1fffu] = data;
        }

        private void InitializeMemory()
        {
            bios = File.ReadAllBytes("systems/gb.sys/boot.rom");

            Hook(0x0000, 0x00ff, PeekBios);
            Hook(0xc000, 0xfdff, PeekWRam, PokeWRam);
            Hook(0xff80, 0xfffe, PeekHRam, PokeHRam);
        }

        public void Hook(uint address, Peek peek)
        {
            peeks[address] = peek;
        }

        public void Hook(uint address, Poke poke)
        {
            pokes[address] = poke;
        }

        public void Hook(uint address, Peek peek, Poke poke)
        {
            Hook(address, peek);
            Hook(address, poke);
        }

        public void Hook(uint address, uint last, Peek peek)
        {
            for (; address <= last; address++)
                Hook(address, peek);
        }

        public void Hook(uint address, uint last, Poke poke)
        {
            for (; address <= last; address++)
                Hook(address, poke);
        }

        public void Hook(uint address, uint last, Peek peek, Poke poke)
        {
            for (; address <= last; address++)
            {
                Hook(address, peek);
                Hook(address, poke);
            }
        }

        public void Dispatch()
        {
            if (cpu.interrupt.ff2 == 1)
            {
                cpu.interrupt.ff2 = 0;
                cpu.interrupt.ff1 = 1;
            }

            ppu.Update(cpu.Single);
            apu.Update(cpu.Single);
            tma.Update();
        }

        public byte PeekByte(uint address)
        {
            Dispatch();

            return peeks[address](address);
        }

        public void PokeByte(uint address, byte data)
        {
            Dispatch();

            pokes[address](address, data);
        }

        public byte PeekByteFree(uint address)
        {
            return peeks[address](address);
        }
    }
}
