using System;
using Beta.Famicom.CPU;
using Beta.Famicom.Formats;
using Beta.Platform;
using Beta.Platform.Exceptions;
using Beta.Platform.Messaging;

namespace Beta.Famicom.Boards.Konami
{
    [BoardName("KONAMI-VRC-6")]
    public class KonamiVrc6 : Board
    {
        private Irq irq;
        private Sound sound;
        private int[] chrPages;
        private int[] prgPages;
        private int mirroring;

        public KonamiVrc6(CartridgeImage image)
            : base(image)
        {
            chrPages = new int[8];
            prgPages = new int[3];
            prgPages[0] = +0 << 13;
            prgPages[1] = +0 << 13;
            prgPages[2] = -1 << 13;

            irq = new Irq();
            sound = new Sound();
        }

        private void Poke8000(ushort address, byte data)
        {
            prgPages[0] = data << 14;
        }

        private void PokeB003(ushort address, byte data)
        {
            mirroring = (data >> 2) & 0x03;
        }

        private void PokeC000(ushort address, byte data)
        {
            prgPages[1] = data << 13;
        }

        private void PokeD000(ushort address, byte data)
        {
            chrPages[0] = data << 10;
        }

        private void PokeD001(ushort address, byte data)
        {
            chrPages[1] = data << 10;
        }

        private void PokeD002(ushort address, byte data)
        {
            chrPages[2] = data << 10;
        }

        private void PokeD003(ushort address, byte data)
        {
            chrPages[3] = data << 10;
        }

        private void PokeE000(ushort address, byte data)
        {
            chrPages[4] = data << 10;
        }

        private void PokeE001(ushort address, byte data)
        {
            chrPages[5] = data << 10;
        }

        private void PokeE002(ushort address, byte data)
        {
            chrPages[6] = data << 10;
        }

        private void PokeE003(ushort address, byte data)
        {
            chrPages[7] = data << 10;
        }

        private void PokeF000(ushort address, byte data)
        {
            irq.Refresh = data;
        }

        private void PokeF001(ushort address, byte data)
        {
            irq.Mode = (data & 0x04) != 0;
            irq.Enabled = (data & 0x02) != 0;
            irq.EnabledRefresh = (data & 0x01) != 0;
            irq.Scaler = 341;

            if (irq.Enabled)
                irq.Counter = irq.Refresh;

            Cpu.Irq(0);
        }

        private void PokeF002(ushort address, byte data)
        {
            irq.Enabled = irq.EnabledRefresh;
            Cpu.Irq(0);
        }

        protected override int DecodeChr(ushort address)
        {
            return (address & 0x3ff) | chrPages[(address >> 10) & 7];
        }

        protected override int DecodePrg(ushort address)
        {
            switch (address & 0xe000)
            {
            case 0x8000:
            case 0xa000: return (address & 0x3fff) | prgPages[0];
            case 0xc000: return (address & 0x1fff) | prgPages[1];
            case 0xe000: return (address & 0x1fff) | prgPages[2];
            }

            throw new CompilerPleasingException();
        }

        public override void Consume(ClockSignal e)
        {
            if (!irq.Enabled)
                return;

            if (irq.Mode)
            {
                if (irq.Clock())
                    Cpu.Irq(1);
            }
            else
            {
                irq.Scaler -= 3;

                if (irq.Scaler <= 0)
                {
                    irq.Scaler += 341;

                    if (irq.Clock())
                    {
                        Cpu.Irq(1);
                    }
                }
            }
        }

        public override void Initialize()
        {
            base.Initialize();

            Cpu.Hook(sound);
        }

        public override void MapToCpu(R2A03Bus bus)
        {
            base.MapToCpu(bus);

            var pin9 = 1 << int.Parse(GetPin("VRC6", 0x9).Replace("PRG A", ""));
            var pinA = 1 << int.Parse(GetPin("VRC6", 0xA).Replace("PRG A", ""));

            bus.Map("1000 ---- ---- ----", writer: Poke8000);
            bus.Map("1001 ---- ---- --00", writer: sound.Sq1.PokeReg1);
            bus.Map("1010 ---- ---- --00", writer: sound.Sq2.PokeReg1);
            bus.Map("1011 ---- ---- --00", writer: sound.Saw.PokeReg1);
            bus.Map("1100 ---- ---- ----", writer: PokeC000);
            bus.Map("1101 ---- ---- --00", writer: PokeD000);
            bus.Map("1110 ---- ---- --00", writer: PokeE000);
            bus.Map("1111 ---- ---- --00", writer: PokeF000);

            if (pin9 == 2)
            {
                // $8001
                bus.Map("1001 ---- ---- --01", writer: sound.Sq1.PokeReg2);
                bus.Map("1010 ---- ---- --01", writer: sound.Sq2.PokeReg2);
                bus.Map("1011 ---- ---- --01", writer: sound.Saw.PokeReg2);
                // $c001
                bus.Map("1101 ---- ---- --01", writer: PokeD001);
                bus.Map("1110 ---- ---- --01", writer: PokeE001);
                bus.Map("1111 ---- ---- --01", writer: PokeF001);
            }
            else
            {
                // $8002
                bus.Map("1001 ---- ---- --01", writer: sound.Sq1.PokeReg3);
                bus.Map("1010 ---- ---- --01", writer: sound.Sq2.PokeReg3);
                bus.Map("1011 ---- ---- --01", writer: sound.Saw.PokeReg3);
                // $c002
                bus.Map("1101 ---- ---- --01", writer: PokeD002);
                bus.Map("1110 ---- ---- --01", writer: PokeE002);
                bus.Map("1111 ---- ---- --01", writer: PokeF002);
            }

            if (pinA == 1)
            {
                // $8002
                bus.Map("1001 ---- ---- --10", writer: sound.Sq1.PokeReg3);
                bus.Map("1010 ---- ---- --10", writer: sound.Sq2.PokeReg3);
                bus.Map("1011 ---- ---- --10", writer: sound.Saw.PokeReg3);
                // $c002
                bus.Map("1101 ---- ---- --10", writer: PokeD002);
                bus.Map("1110 ---- ---- --10", writer: PokeE002);
                bus.Map("1111 ---- ---- --10", writer: PokeF002);
            }
            else
            {
                // $8001
                bus.Map("1001 ---- ---- --01", writer: sound.Sq1.PokeReg2);
                bus.Map("1010 ---- ---- --01", writer: sound.Sq2.PokeReg2);
                bus.Map("1011 ---- ---- --01", writer: sound.Saw.PokeReg2);
                // $c001
                bus.Map("1101 ---- ---- --01", writer: PokeD001);
                bus.Map("1110 ---- ---- --01", writer: PokeE001);
                bus.Map("1111 ---- ---- --01", writer: PokeF001);
            }

            // $8003
            // $9003
            // $a003
            bus.Map("1011 ---- ---- --11", writer: PokeB003);
            // $c003
            bus.Map("1101 ---- ---- --11", writer: PokeD003);
            bus.Map("1110 ---- ---- --11", writer: PokeE003);
            // $f003
        }

        public override bool VRAM(ushort address, out int a10)
        {
            var x = (address >> 10) & 1;
            var y = (address >> 11) & 1;

            switch (mirroring)
            {
            case 0: a10 = x; return true;
            case 1: a10 = y; return true;
            case 2: a10 = 0; return true;
            case 3: a10 = 1; return true;
            }

            throw new CompilerPleasingException();
        }

        private class Irq
        {
            public bool Mode;
            public bool Enabled;
            public bool EnabledRefresh;
            public int Counter;
            public int Refresh;
            public int Scaler;

            public bool Clock()
            {
                if (Counter == 0xff)
                {
                    Counter = Refresh;
                    return true;
                }

                Counter++;
                return false;
            }
        }

        public class Sound : R2A03.ChannelExt
        {
            public SqrChannel Sq1;
            public SqrChannel Sq2;
            public SawChannel Saw;

            public Sound()
            {
                Sq1 = new SqrChannel();
                Sq2 = new SqrChannel();
                Saw = new SawChannel();
            }

            public override short Render()
            {
                var output = 0;

                output += (Sq1.Render() * 32767) / 15;
                output += (Sq2.Render() * 32767) / 15;
                output += (Saw.Render() * 32767) / 31;

                return (short)(output / 3);
            }

            public sealed class SqrChannel : R2A03.Channel
            {
                private static int[][] dutyTable = new[]
                {
                    new[] { 0x00, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f},
                    new[] { 0x00, 0x00, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f},
                    new[] { 0x00, 0x00, 0x00, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f},
                    new[] { 0x00, 0x00, 0x00, 0x00, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f},
                    new[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f},
                    new[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f},
                    new[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f},
                    new[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f},
                    //-- 'digital' mode
                    new[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00},
                    new[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00},
                    new[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00},
                    new[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00},
                    new[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00},
                    new[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00},
                    new[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00},
                    new[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00}
                };

                private bool enabled;
                private int form;
                private int step;
                private int level;

                public SqrChannel()
                {
                    Timing.Cycles =
                    Timing.Single = R2A03.PHASE;
                    Timing.Period = R2A03.DELAY;
                }

                public void PokeReg1(ushort address, byte data)
                {
                    form = data >> 4 & 0xF;
                    level = data & 0xF;
                }

                public void PokeReg2(ushort address, byte data)
                {
                    Frequency = (Frequency & ~0x0FF) | (data << 0 & 0x0FF);
                    Timing.Single = (Frequency + 1) * R2A03.PHASE;
                }

                public void PokeReg3(ushort address, byte data)
                {
                    Frequency = (Frequency & ~0xF00) | (data << 8 & 0xF00);
                    Timing.Single = (Frequency + 1) * R2A03.PHASE;

                    enabled = (data & 0x80) != 0;
                }

                public byte Render()
                {
                    var sum = Timing.Cycles;
                    Timing.Cycles -= Timing.Period;

                    if (enabled)
                    {
                        if (Timing.Cycles >= 0)
                        {
                            return (byte)(level >> dutyTable[form][step]);
                        }

                        sum >>= dutyTable[form][step];

                        for (; Timing.Cycles < 0; Timing.Cycles += Timing.Single)
                        {
                            sum += Math.Min(-Timing.Cycles, Timing.Single) >> dutyTable[form][step = (step + 1) & 0xF];
                        }

                        return (byte)((sum * level) / Timing.Period);
                    }

                    var count = (~Timing.Cycles + Timing.Single) / Timing.Single;

                    step = (step + count) & 0xF;
                    Timing.Cycles += count * Timing.Single;

                    return 0;
                }
            }

            public sealed class SawChannel : R2A03.Channel
            {
                private bool enabled;
                private int accum;
                private int rate;
                private int step;

                public SawChannel()
                {
                    Timing.Cycles =
                    Timing.Single = R2A03.PHASE;
                    Timing.Period = R2A03.DELAY;
                }

                public void PokeReg1(ushort address, byte data)
                {
                    rate = data & 0x3F;
                }

                public void PokeReg2(ushort address, byte data)
                {
                    Frequency = (Frequency & ~0x0FF) | (data << 0 & 0x0FF);
                    Timing.Single = (Frequency + 1) * R2A03.PHASE;
                }

                public void PokeReg3(ushort address, byte data)
                {
                    Frequency = (Frequency & ~0xF00) | (data << 8 & 0xF00);
                    Timing.Single = (Frequency + 1) * R2A03.PHASE;

                    enabled = (data & 0x80) != 0;
                }

                public byte Render()
                {
                    var sum = Timing.Cycles;
                    Timing.Cycles -= Timing.Period;

                    if (Timing.Cycles >= 0)
                    {
                        return (byte)(accum >> 3 & 0x1F);
                    }

                    sum *= (accum >> 3);

                    for (; Timing.Cycles < 0; Timing.Cycles += Timing.Single)
                    {
                        step = (step + 1) % 14;
                        accum = (rate * (step >> 1));

                        sum += Math.Min(-Timing.Cycles, Timing.Single) * (accum >> 3 & 0x1F);
                    }

                    return (byte)(enabled ? (sum / Timing.Period) : 0);
                }
            }
        }
    }
}
