namespace Beta.Famicom.CPU
{
    public sealed class DmcStateManager
    {
        private readonly DmcState dmc;

        public DmcStateManager(State state)
        {
            this.dmc = state.r2a03.dmc;
        }

        public void Read(ushort address, ref byte data)
        {
        }

        public void Write(ushort address, byte data)
        {
        }
    }
}
