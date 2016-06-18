using Beta.Platform.Processors;

namespace Beta.GameBoy.CPU
{
    public class Cpu : LR35902
    {
        private GameSystem gameSystem;
        private byte ef;
        private byte rf;

        public Cpu(GameSystem gameSystem)
        {
            this.gameSystem = gameSystem;

            Single = 4;
        }

        protected override void Dispatch()
        {
            gameSystem.Dispatch();
        }

        protected override byte Peek(ushort address)
        {
            return gameSystem.PeekByte(address);
        }

        protected override void Poke(ushort address, byte data)
        {
            gameSystem.PokeByte(address, data);
        }

        public void Initialize()
        {
            gameSystem.Hook(0xff0f, a => rf, (a, data) => rf = data);
            gameSystem.Hook(0xffff, a => ef, (a, data) => ef = data);
        }

        public override void Update()
        {
            base.Update();

            var flags = (rf & ef) & -interrupt.ff1;

            if (flags != 0)
            {
                gameSystem.Dispatch();
                interrupt.ff1 = 0;

                if ((flags & 0x01) != 0) { rf ^= 0x01; Rst(0x40); return; }
                if ((flags & 0x02) != 0) { rf ^= 0x02; Rst(0x48); return; }
                if ((flags & 0x04) != 0) { rf ^= 0x04; Rst(0x50); return; }
                if ((flags & 0x08) != 0) { rf ^= 0x08; Rst(0x58); return; }
                if ((flags & 0x10) != 0) { rf ^= 0x10; Rst(0x60); return; }
            }
        }

        public void RequestInterrupt(byte flag)
        {
            rf |= flag;

            if ((ef & flag) != 0)
            {
                Halt = false;

                if (flag == Interrupt.JOYPAD)
                {
                    Stop = false;
                }
            }
        }
    }
}
