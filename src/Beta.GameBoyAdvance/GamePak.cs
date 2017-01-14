using System.IO;
using Beta.GameBoyAdvance.Messaging;
using Beta.Platform;
using Beta.Platform.Exceptions;
using Beta.Platform.Messaging;
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

        private readonly IProducer<AddClockSignal> clock;
        private readonly byte[] binary;

        private half[] buffer;
        private word counter;
        private word mask;
        private int ramAccess;
        private int[] romAccess1 = new int[3];
        private int[] romAccess2 = new int[3];

        public GamePak(byte[] binary, IProducer<AddClockSignal> clock)
        {
            this.binary = binary;
            this.clock = clock;

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

        private byte ReadRomByte(word address)
        {
            switch (address & 1)
            {
            case 0: return (byte)(ReadRomHalf(address));
            case 1: return (byte)(ReadRomHalf(address) >> 8);
            }

            throw new CompilerPleasingException();
        }

        private half ReadRomHalf(word address)
        {
            var region = (address >> 25) & 3u;

            clock.Produce(new AddClockSignal(romAccess2[region]));
            counter++;

            return buffer[(address >> 1) & mask];
        }

        private word ReadRomWord(word address)
        {
            var lower = ReadRomHalf(address & ~2u);
            var upper = ReadRomHalf(address | 2u);

            return (word)((upper << 16) | lower);
        }

        public word ReadRam(int size, word address)
        {
            clock.Produce(new AddClockSignal(ramAccess));
            return 0;
        }

        public word ReadRom(int size, word address)
        {
            if (size == 2) return ReadRomWord(address);
            if (size == 1) return ReadRomHalf(address);
            if (size == 0) return ReadRomByte(address);

            throw new CompilerPleasingException();
        }

        public void WriteRam(int size, word address, word data)
        {
            clock.Produce(new AddClockSignal(ramAccess));
        }

        public void WriteRom(int size, word address, word data)
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
