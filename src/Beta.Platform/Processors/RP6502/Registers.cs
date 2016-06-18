using System.Runtime.InteropServices;

namespace Beta.Platform.Processors.RP6502
{
    [StructLayout(LayoutKind.Explicit)]
    public struct Registers
    {
        [FieldOffset(0)]
        public byte EAL;

        [FieldOffset(1)]
        public byte EAH;

        [FieldOffset(2)]
        public byte PCL;

        [FieldOffset(3)]
        public byte PCH;

        [FieldOffset(4)]
        public byte SPL;

        [FieldOffset(5)]
        public byte SPH;

        [FieldOffset(6)]
        public byte A;

        [FieldOffset(7)]
        public byte X;

        [FieldOffset(8)]
        public byte Y;

        [FieldOffset(0)]
        public ushort EA;

        [FieldOffset(2)]
        public ushort PC;

        [FieldOffset(4)]
        public ushort SP;
    }
}
