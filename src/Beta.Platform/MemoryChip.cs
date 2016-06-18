using System.Runtime.InteropServices;
using half = System.UInt16;
using word = System.UInt32;

namespace Beta.Platform
{
    [StructLayout(LayoutKind.Explicit)]
    public class MemoryChip
    {
        [field: FieldOffset(0)]
        public byte[] b;

        [field: FieldOffset(0)]
        public half[] h;

        [field: FieldOffset(0)]
        public word[] w;

        [field: FieldOffset(8)]
        public word mask;

        [field: FieldOffset(12)]
        public bool writable;

        public MemoryChip(byte[] buffer)
        {
            w = null;
            h = null;
            b = buffer.Clone() as byte[];

            writable = false;
            mask = (word)(buffer.Length - 1);
        }

        public MemoryChip(int capacity)
        {
            w = null;
            h = null;
            b = new byte[capacity];

            writable = true;
            mask = (uint)(capacity - 1);
        }
    }
}
