using Beta.Famicom.Abstractions;
using Beta.Platform;

namespace Beta.Famicom
{
    public delegate void Access(ushort address, ref byte data);

    public class Bus : IBus
    {
        public readonly Access[] Peeks;
        public readonly Access[] Pokes;

        public Bus(int capacity)
        {
            Peeks = new Access[capacity];
            Pokes = new Access[capacity];
        }

        public IBusDecoder Decode(string pattern)
        {
            return new BusDecoder(this, pattern);
        }

        public void Peek(ushort address, ref byte data)
        {
            Peeks[address](address, ref data);
        }

        public void Poke(ushort address, ref byte data)
        {
            Pokes[address](address, ref data);
        }
    }

    public sealed class BusDecoder : IBusDecoder
    {
        private readonly Bus bus;
        private readonly uint min;
        private readonly uint max;
        private readonly uint mask;

        public BusDecoder(Bus bus, string pattern)
        {
            this.bus = bus;
            min = BitString.Min(pattern);
            max = BitString.Max(pattern);
            mask = BitString.Mask(pattern);
        }

        public IBusDecoder Peek(Access access)
        {
            if (access != null)
            {
                for (var address = min; address <= max; address++)
                {
                    if ((address & mask) == min)
                    {
                        bus.Peeks[address] = access;
                    }
                }
            }

            return this;
        }

        public IBusDecoder Poke(Access access)
        {
            if (access != null)
            {
                for (var address = min; address <= max; address++)
                {
                    if ((address & mask) == min)
                    {
                        bus.Pokes[address] = access;
                    }
                }
            }

            return this;
        }
    }
}
