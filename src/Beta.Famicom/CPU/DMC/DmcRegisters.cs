namespace Beta.Famicom.CPU
{
    public sealed class DmcRegisters
    {
        private readonly DmcState dmc;

        public DmcRegisters(State state)
        {
            this.dmc = state.r2a03.dmc;
        }

        public void Write(ushort address, byte data)
        {
        }
    }
}
