namespace Beta.Famicom.CPU
{
    public sealed class R2A03StateManager
    {
        private readonly Sq1StateManager sq1;
        private readonly Sq2StateManager sq2;
        private readonly TriStateManager tri;
        private readonly NoiStateManager noi;
        private readonly DmcStateManager dmc;

        public R2A03StateManager(State state)
        {
            this.sq1 = new Sq1StateManager(state);
            this.sq2 = new Sq2StateManager(state);
            this.tri = new TriStateManager(state);
            this.noi = new NoiStateManager(state);
            this.dmc = new DmcStateManager(state);
        }

        public void Read(ushort address, ref byte data)
        {
            // switch (address & ~3)
            // {
            // case 0x4000: sq1.Read(address, ref data); break;
            // case 0x4004: sq2.Read(address, ref data); break;
            // case 0x4008: tri.Read(address, ref data); break;
            // case 0x400c: noi.Read(address, ref data); break;
            // case 0x4010: dmc.Read(address, ref data); break;
            // }

            if (address == 0x4014) { }
            if (address == 0x4015) { }
            if (address == 0x4016) { }
            if (address == 0x4017) { }
        }

        public void Write(ushort address, byte data)
        {
            switch (address & ~3)
            {
            case 0x4000: sq1.Write(address, data); break;
            case 0x4004: sq2.Write(address, data); break;
            case 0x4008: tri.Write(address, data); break;
            case 0x400c: noi.Write(address, data); break;
            case 0x4010: dmc.Write(address, data); break;
            }

            if (address == 0x4014) { }
            if (address == 0x4015) { }
            if (address == 0x4016) { }
            if (address == 0x4017) { }
        }
    }
}
