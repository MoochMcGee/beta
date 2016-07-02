using System.IO;
using Beta.GameBoyAdvance.CPU;
using Beta.Platform;
using Beta.Platform.Exceptions;
using half = System.UInt16;
using word = System.UInt32;

namespace Beta.GameBoyAdvance
{
    public sealed class GamePak
    {
        public const int WAIT_STATE_0 = 0;
        public const int WAIT_STATE_1 = 1;
        public const int WAIT_STATE_2 = 2;

        private static int[] access1Table = new[]
        {
            4, 3, 2, 8
        };

        private static int[][] access2Table = new[]
        {
            new[] { 2, 4, 8 },
            new[] { 1, 1, 1 }
        };

        private Cpu cpu;
        private Register32 latch;
        private half[] buffer;
        private word counter;
        private word mask;
        private byte[] binary;
        private int ramAccess;
        private int[] romAccess1 = new int[3];
        private int[] romAccess2 = new int[3];

        public GamePak(Driver gameSystem, byte[] binary)
        {
            cpu = gameSystem.Cpu;
            this.binary = binary;

            SetRamAccessTiming(0);
            Set1stAccessTiming(0, WAIT_STATE_0);
            Set2ndAccessTiming(0, WAIT_STATE_0);
            Set1stAccessTiming(0, WAIT_STATE_1);
            Set2ndAccessTiming(0, WAIT_STATE_1);
            Set1stAccessTiming(0, WAIT_STATE_2);
            Set2ndAccessTiming(0, WAIT_STATE_2);
        }

        public void Initialize()
        {
            var length = MathHelper.NextPowerOfTwo((uint)binary.Length) >> 1;
            var stream = new MemoryStream(binary);
            var reader = new BinaryReader(stream);

            buffer = new half[length];

            for (var i = 0; i < binary.Length / 2; i++)
            {
                buffer[i] = reader.ReadUInt16();
            }

            mask = (length - 1U);
        }

        private byte PeekRomByte(word address)
        {
            switch (address & 1)
            {
            case 0: return (byte)(PeekRomHalf(address));
            case 1: return (byte)(PeekRomHalf(address) >> 8);
            }

            throw new CompilerPleasingException();
        }

        private half PeekRomHalf(word address)
        {
            var region = (address >> 25) & 3u;
            var compare = (address >>= 1) & 0xffffu;

            if (counter != compare)
            {
                cpu.Cycles += romAccess1[region];
                counter = compare;
            }

            cpu.Cycles += romAccess2[region];
            counter++;

            return buffer[address & mask];
        }

        private word PeekRomWord(word address)
        {
            latch.uw0 = PeekRomHalf(address & ~2u);
            latch.uw1 = PeekRomHalf(address | 2u);

            return latch.ud0;
        }

        public word PeekRam(int size, word address)
        {
            cpu.Cycles += ramAccess;
            return 0;
        }

        public word PeekRom(int size, word address)
        {
            if (size == 2) return PeekRomWord(address);
            if (size == 1) return PeekRomHalf(address);
            if (size == 0) return PeekRomByte(address);

            throw new CompilerPleasingException();
        }

        public void PokeRam(int size, word address, word data)
        {
            cpu.Cycles += ramAccess;
        }

        public void PokeRom(int size, word address, word data)
        {
        }

        public void SetRamAccessTiming(int speedSpecifier)
        {
            ramAccess = access1Table[speedSpecifier];
        }

        public void Set1stAccessTiming(int speedSpecifier, int region)
        {
            romAccess1[region] = access1Table[speedSpecifier];
        }

        public void Set2ndAccessTiming(int speedSpecifier, int region)
        {
            romAccess2[region] = access2Table[speedSpecifier][region];
        }
    }
}
