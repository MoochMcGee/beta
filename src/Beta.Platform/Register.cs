using System.Runtime.InteropServices;

namespace Beta.Platform
{
    [StructLayout(LayoutKind.Explicit)]
    public struct Register16
    {
        [field: FieldOffset(0)]
        public byte l;

        [field: FieldOffset(1)]
        public byte h;

        [field: FieldOffset(0)]
        public ushort w;

        public override string ToString() => $"0x{w:x4}";
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct Register24
    {
        [field: FieldOffset(0)]
        public byte l;

        [field: FieldOffset(1)]
        public byte h;

        [field: FieldOffset(2)]
        public byte b;

        [field: FieldOffset(0)]
        public ushort w;

        [field: FieldOffset(0)]
        public uint d;

        public override string ToString() => $"0x{d:x6}";
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct Register32
    {
        // unsigned
        [field: FieldOffset(0)]
        public byte ub0;

        [field: FieldOffset(1)]
        public byte ub1;

        [field: FieldOffset(2)]
        public byte ub2;

        [field: FieldOffset(3)]
        public byte ub3;

        [field: FieldOffset(0)]
        public ushort uw0;

        [field: FieldOffset(2)]
        public ushort uw1;

        [field: FieldOffset(0)]
        public uint ud0;

        // signed
        [field: FieldOffset(0)]
        public int sd0;

        public override string ToString() => $"0x{ud0:x8}";
    }
}
