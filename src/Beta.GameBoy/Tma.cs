using Beta.GameBoy.CPU;
using Beta.Platform;
using Beta.Platform.Processors;

namespace Beta.GameBoy
{
    public class Tma
    {
        private static byte[] lut =
        {
            0x01, // (1.048576MHz / 256) =   4.096KHz
            0x40, // (1.048576MHz /   4) = 262.144KHz
            0x10, // (1.048576MHz /  16) =  65.536KHz
            0x04  // (1.048576MHz /  64) =  16.384KHz
        };

        private Cpu cpu;
        private GameSystem gameSystem;

        private Register16 div;
        private Register16 tma;
        private byte cnt;
        private byte mod;

        public Tma(GameSystem gameSystem)
        {
            this.gameSystem = gameSystem;
            cpu = gameSystem.cpu;
        }

        public void Initialize()
        {
            gameSystem.Hook(0xff04, a => div.h, (a, d) => div.h = 0);
            gameSystem.Hook(0xff05, a => tma.h, (a, d) => tma.h = d);
            gameSystem.Hook(0xff06, a => mod, (a, d) => mod = d);
            gameSystem.Hook(0xff07, a => cnt, (a, d) => cnt = d);
        }

        public void Update()
        {
            div.w += lut[3];

            if ((cnt & 0x4) != 0)
            {
                tma.w += lut[cnt & 3];

                if (tma.w < lut[cnt & 3])
                {
                    tma.h = mod;
                    cpu.RequestInterrupt(LR35902.Interrupt.ELAPSE);
                }
            }
        }
    }
}
