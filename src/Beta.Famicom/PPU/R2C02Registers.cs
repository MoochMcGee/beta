namespace Beta.Famicom.PPU
{
    public sealed class R2C02Registers
    {
        private readonly R2C02State r2c02;

        public R2C02Registers(State state)
        {
            this.r2c02 = state.r2c02;
        }

        public void Read(ushort address, ref byte data)
        {
        }

        public void Write(ushort address, byte data)
        {
        }
    }
}
