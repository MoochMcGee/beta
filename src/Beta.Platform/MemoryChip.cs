using System.Runtime.InteropServices;
using half = System.UInt16;
using word = System.UInt32;

namespace Beta.Platform
{
    [StructLayout(LayoutKind.Explicit)]
    public class MemoryChip
    {
        [FieldOffset(0)] public byte[] b;
        [FieldOffset(0)] public half[] h;
        [FieldOffset(0)] public word[] w;

        public MemoryChip(byte[] buffer)
        {
            w = null;
            h = null;
            b = buffer.Clone() as byte[];
        }

        public MemoryChip(int capacity)
        {
            w = null;
            h = null;
            b = new byte[capacity];
        }
    }
}
