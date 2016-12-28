using Beta.Platform.Core;
using Beta.Platform.Exceptions;
using half = System.UInt16;
using word = System.UInt32;

namespace Beta.GameBoyAdvance.Memory
{
    public sealed class MMIO : IMemory
    {
        private const uint SIZE = (1u << 10);
        private const uint MASK = (SIZE - 1u);

        private readonly Registers regs;

        private Reader[] readers = new Reader[SIZE];
        private Writer[] writers = new Writer[SIZE];

        public MMIO(Registers regs)
        {
            this.regs = regs;

            for (int i = 0; i < 1024; i++)
            {
                readers[i] = ReadOpenBus;
                writers[i] = WriteOpenBus;
            }

            Map(0x130, Read130);
            Map(0x131, Read131);
            Map(0x132, Read132, Write132);
            Map(0x133, Read133, Write133);

            Map(0x200, Read200, Write200);
            Map(0x201, Read201, Write201);
            Map(0x202, Read202, Write202);
            Map(0x203, Read203, Write203);
            Map(0x208, Read208, Write208);
            Map(0x209, Read209, Write209);
        }

        private byte ReadOpenBus(word address)
        {
            return 0;
        }

        private void WriteOpenBus(word address, byte data)
        {
        }

        public uint Read(int size, uint address)
        {
            if (address > 0x040003ff)
            {
                return 0;
            }

            address = address & MASK;

            var b0 = readers[address | 0](address | 0); if (size == 0) return b0;
            var b1 = readers[address | 1](address | 1); if (size == 1) return (half)((b1 << 8) | b0);
            var b2 = readers[address | 2](address | 2);
            var b3 = readers[address | 3](address | 3); if (size == 2) return (word)((b3 << 24) | (b2 << 16) | (b1 << 8) | b0);

            throw new CompilerPleasingException();
        }

        public void Write(int size, uint address, uint data)
        {
            if (address > 0x040003ff)
            {
                return;
            }

            address = address & MASK;

            writers[address | 0](address | 0, (byte)(data >> 0)); if (size == 0) return;
            writers[address | 1](address | 1, (byte)(data >> 8)); if (size == 1) return;
            writers[address | 2](address | 2, (byte)(data >> 16));
            writers[address | 3](address | 3, (byte)(data >> 24)); if (size == 2) return;

            throw new CompilerPleasingException();
        }

        public void Map(uint address, Reader reader)
        {
            readers[address] = reader;
        }

        public void Map(uint address, Writer writer)
        {
            writers[address] = writer;
        }

        public void Map(uint address, Reader reader, Writer writer)
        {
            Map(address, reader);
            Map(address, writer);
        }

        public void Map(uint address, uint last, Reader reader, Writer writer)
        {
            for (; address <= last; address++)
            {
                Map(address, reader, writer);
            }
        }

        #region Registers

        private byte Read130(uint address)
        {
            return (byte)(regs.pad.data >> 0);
        }

        private byte Read131(uint address)
        {
            return (byte)(regs.pad.data >> 8);
        }

        private byte Read132(uint address)
        {
            return (byte)(regs.pad.mask >> 0);
        }

        private byte Read133(uint address)
        {
            return (byte)(regs.pad.mask >> 8);
        }

        private void Write132(uint address, byte data)
        {
            regs.pad.mask &= 0xff00;
            regs.pad.mask |= data;
        }

        private void Write133(uint address, byte data)
        {
            regs.pad.mask &= 0x00ff;
            regs.pad.mask |= (half)(data << 8);
        }

        private byte Read200(uint address)
        {
            return (byte)(regs.cpu.ief >> 0);
        }

        private byte Read201(uint address)
        {
            return (byte)(regs.cpu.ief >> 8);
        }

        private byte Read202(uint address)
        {
            return (byte)(regs.cpu.irf >> 0);
        }

        private byte Read203(uint address)
        {
            return (byte)(regs.cpu.irf >> 8);
        }

        private void Write200(uint address, byte data)
        {
            regs.cpu.ief &= 0xff00;
            regs.cpu.ief |= data;
        }

        private void Write201(uint address, byte data)
        {
            regs.cpu.ief &= 0x00ff;
            regs.cpu.ief |= (half)(data << 8);
        }

        private void Write202(uint address, byte data)
        {
            regs.cpu.irf &= (half)~(data << 0);
        }

        private void Write203(uint address, byte data)
        {
            regs.cpu.irf &= (half)~(data << 8);
        }

        private byte Read208(uint address)
        {
            return (byte)(regs.cpu.ime ? 1 : 0);
        }

        private byte Read209(uint address)
        {
            return 0;
        }

        private void Write208(uint address, byte data)
        {
            regs.cpu.ime = (data & 1) != 0;
        }

        private void Write209(uint address, byte data)
        {
        }

        #endregion
    }
}
