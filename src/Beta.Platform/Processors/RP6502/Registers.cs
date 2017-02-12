using System.Runtime.InteropServices;

namespace Beta.Platform.Processors.RP6502
{
    [StructLayout(LayoutKind.Explicit)]
    public struct Registers
    {
        [FieldOffset(0)] public byte eal;
        [FieldOffset(1)] public byte eah;

        [FieldOffset(2)] public byte pcl;
        [FieldOffset(3)] public byte pch;

        [FieldOffset(4)] public byte spl;
        [FieldOffset(5)] public byte sph;

        [FieldOffset(6)] public byte a;
        [FieldOffset(7)] public byte x;
        [FieldOffset(8)] public byte y;

        [FieldOffset(0)] public ushort ea;
        [FieldOffset(2)] public ushort pc;
        [FieldOffset(4)] public ushort sp;
    }
}
