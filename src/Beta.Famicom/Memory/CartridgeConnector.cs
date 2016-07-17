namespace Beta.Famicom.Memory
{
    public sealed class CartridgeConnector
    {
        public void PpuRead(ushort address, ref byte data) { }

        public void PpuWrite(ushort address, byte data) { }

        public void CpuRead(ushort address, ref byte data) { }

        public void CpuWrite(ushort address, byte data) { }
    }
}
