namespace Beta.Famicom.Messaging
{
    public sealed class PpuAddressSignal
    {
        public readonly ushort Address;

        public PpuAddressSignal(ushort address)
        {
            this.Address = address;
        }
    }
}
