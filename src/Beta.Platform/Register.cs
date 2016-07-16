using System.Runtime.InteropServices;

namespace Beta.Platform
{
    [StructLayout(LayoutKind.Explicit)]
    public struct Register16
    {
        [FieldOffset(0)] public byte l;
        [FieldOffset(1)] public byte h;
        [FieldOffset(0)] public ushort w;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct Register32
    {
        // unsigned
        [FieldOffset(0)] public byte ub0;
        [FieldOffset(1)] public byte ub1;
        [FieldOffset(2)] public byte ub2;
        [FieldOffset(3)] public byte ub3;

        [FieldOffset(0)] public ushort uw0;
        [FieldOffset(2)] public ushort uw1;

        [FieldOffset(0)] public uint ud0;

        // signed
        [FieldOffset(0)] public int sd0;
    }
}
