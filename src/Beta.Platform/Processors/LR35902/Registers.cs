using System.Runtime.InteropServices;

namespace Beta.Platform.Processors.LR35902
{
    [StructLayout(LayoutKind.Explicit)]
    public struct Registers
    {
        [FieldOffset(1)]  public byte b;
        [FieldOffset(0)]  public byte c;
        [FieldOffset(3)]  public byte d;
        [FieldOffset(2)]  public byte e;
        [FieldOffset(5)]  public byte h;
        [FieldOffset(4)]  public byte l;
        [FieldOffset(7)]  public byte a;
        [FieldOffset(6)]  public byte f;
        [FieldOffset(8)]  public byte spl;
        [FieldOffset(9)]  public byte sph;
        [FieldOffset(10)] public byte pcl;
        [FieldOffset(11)] public byte pch;
        [FieldOffset(12)] public byte aal;
        [FieldOffset(13)] public byte aah;

        [FieldOffset(0)]  public ushort bc;
        [FieldOffset(2)]  public ushort de;
        [FieldOffset(4)]  public ushort hl;
        [FieldOffset(6)]  public ushort af;
        [FieldOffset(8)]  public ushort sp;
        [FieldOffset(10)] public ushort pc;
        [FieldOffset(12)] public ushort aa;
    }
}
