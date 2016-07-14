using System.Runtime.InteropServices;

namespace Beta.Platform.Processors.RP65816
{
    [StructLayout(LayoutKind.Explicit)]
    public struct Registers
    {
        [FieldOffset(0x00)] public byte al;
        [FieldOffset(0x01)] public byte ah;

        [FieldOffset(0x02)] public byte xl;
        [FieldOffset(0x03)] public byte xh;

        [FieldOffset(0x04)] public byte yl;
        [FieldOffset(0x05)] public byte yh;

        [FieldOffset(0x06)] public byte dpl;
        [FieldOffset(0x07)] public byte dph;

        [FieldOffset(0x08)] public byte spl;
        [FieldOffset(0x09)] public byte sph;

        [FieldOffset(0x0a)] public byte rdl;
        [FieldOffset(0x0b)] public byte rdh;

        [FieldOffset(0x0c)] public byte pcl;
        [FieldOffset(0x0d)] public byte pch;
        [FieldOffset(0x0e)] public byte pcb;
        [FieldOffset(0x0f)] public byte pc_;

        [FieldOffset(0x10)] public byte aal;
        [FieldOffset(0x11)] public byte aah;
        [FieldOffset(0x12)] public byte aab;
        [FieldOffset(0x13)] public byte aa_;

        [FieldOffset(0x14)] public byte ial;
        [FieldOffset(0x15)] public byte iah;
        [FieldOffset(0x16)] public byte iab;
        [FieldOffset(0x17)] public byte ia_;

        [FieldOffset(0x00)] public ushort a;
        [FieldOffset(0x02)] public ushort x;
        [FieldOffset(0x04)] public ushort y;
        [FieldOffset(0x06)] public ushort dp;
        [FieldOffset(0x08)] public ushort sp;
        [FieldOffset(0x0a)] public ushort rd;
        [FieldOffset(0x0c)] public ushort pc;
        [FieldOffset(0x10)] public ushort aa;
        [FieldOffset(0x14)] public ushort ia;

        [FieldOffset(0x0c)] public int pc24;
        [FieldOffset(0x10)] public int aa24;
        [FieldOffset(0x14)] public int ia24;
    }
}
