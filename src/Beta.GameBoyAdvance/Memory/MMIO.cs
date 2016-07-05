using Beta.Platform;
using Beta.Platform.Core;
using Beta.Platform.Exceptions;
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
        private Register32 latch;
        private byte[] ioMemory = new byte[1024];

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
            return ioMemory[address & 0x3ff];
        }

        private void WriteOpenBus(word address, byte data)
        {
            ioMemory[address & 0x3ff] = data;
        }

        public uint Read(int size, uint address)
        {
            if (address > 0x040003ff)
            {
                return 0;
            }

            address = (address & 0x3ff);

            latch.ub0 = readers[address | 0](address | 0); if (size == 0) return latch.ub0;
            latch.ub1 = readers[address | 1](address | 1); if (size == 1) return latch.uw0;
            latch.ub2 = readers[address | 2](address | 2);
            latch.ub3 = readers[address | 3](address | 3); if (size == 2) return latch.ud0;

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
            return regs.pad.data.l;
        }

        private byte Read131(uint address)
        {
            return regs.pad.data.h;
        }

        private byte Read132(uint address)
        {
            return regs.pad.mask.l;
        }

        private byte Read133(uint address)
        {
            return regs.pad.mask.h;
        }

        private void Write132(uint address, byte data)
        {
            regs.pad.mask.l = data;
        }

        private void Write133(uint address, byte data)
        {
            regs.pad.mask.h = data;
        }

        private byte Read200(uint address)
        {
            return regs.cpu.ief.l;
        }

        private byte Read201(uint address)
        {
            return regs.cpu.ief.h;
        }

        private byte Read202(uint address)
        {
            return regs.cpu.irf.l;
        }

        private byte Read203(uint address)
        {
            return regs.cpu.irf.h;
        }

        private void Write200(uint address, byte data)
        {
            regs.cpu.ief.l = data;
        }

        private void Write201(uint address, byte data)
        {
            regs.cpu.ief.h = data;
        }

        private void Write202(uint address, byte data)
        {
            regs.cpu.irf.l &= (byte)~data;
        }

        private void Write203(uint address, byte data)
        {
            regs.cpu.irf.h &= (byte)~data;
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
