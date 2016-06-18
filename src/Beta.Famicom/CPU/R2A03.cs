using Beta.Famicom.Abstractions;
using Beta.Famicom.Messaging;
using Beta.Famicom.Input;
using Beta.Platform.Messaging;
using Beta.Platform.Processors.RP6502;

namespace Beta.Famicom.CPU
{
    public partial class R2A03 : Core, IConsumer<VblNmiSignal>
    {
        private readonly IProducer<ClockSignal> clockProducer;

        private GameSystem gameSystem;
        private int strobe;
        private bool dma;
        private ushort dmaAddr;

        public Joypad Joypad1;
        public Joypad Joypad2;

        public R2A03(IRP6502Bus bus, GameSystem gameSystem, IProducer<VblNmiSignal> vblNmiProducer, IProducer<ClockSignal> clockProducer)
            : base(bus)
        {
            this.gameSystem = gameSystem;
            this.clockProducer = clockProducer;

            vblNmiProducer.Subscribe(this);

            Single = 264;
            Single = 132;

            Joypad1 = new StandardController(0);
            Joypad2 = new StandardController(1);

            sq1 = new ChannelSqr();
            sq2 = new ChannelSqr();
            tri = new ChannelTri();
            noi = new ChannelNoi();
            dmc = new ChannelDmc(this);
        }

        protected override void Dispatch()
        {
            clockProducer.Produce(new ClockSignal(Single));

            apuToggle = !apuToggle;

            if (apuToggle)
            {
                ClockSequence();

                dmc.Clock();

                if (reg4017.h != 0)
                {
                    reg4017.h = 0;

                    irqEnabled = (reg4017.l & 0x40) == 0;
                    mode = (reg4017.l & 0x80) >> 7;
                    step = 0;

                    frameTimer.Cycles = timingTable[0][mode][step];

                    if (mode == 1)
                    {
                        ClockQuad();
                        ClockHalf();
                    }
                    else
                    {
                        ClockQuad();
                    }

                    if (!irqEnabled)
                    {
                        irqPending = false;
                        Irq(0);
                    }
                }

                sampleTimer += PHASE * 2;

                if (sampleTimer >= DELAY)
                {
                    sampleTimer -= DELAY;
                    Sample();
                }
            }
        }

        public void Initialize()
        {
            Joypad1.Initialize();
            Joypad2.Initialize();
            InitializeSequence();
        }

        public override void Update()
        {
            base.Update();

            if (dma)
            {
                dma = false;

                for (var i = 0; i < 256; i++, dmaAddr++)
                {
                    Poke(0x2004, Peek(dmaAddr));
                }
            }
        }

        // peek
        private void Peek4015(ushort address, ref byte data)
        {
            data = (byte)(
                (sq1.Enabled ? 0x01 : 0) |
                (sq2.Enabled ? 0x02 : 0) |
                (tri.Enabled ? 0x04 : 0) |
                (noi.Enabled ? 0x08 : 0) |
                (dmc.Enabled ? 0x10 : 0) |
                (irqPending ? 0x40 : 0) |
                (dmc.IrqPending ? 0x80 : 0));

            irqPending = false;
            Irq(0);
        }

        private void Peek4016(ushort address, ref byte data)
        {
            data &= 0xe0;
            data |= Joypad1.GetData(strobe);
        }

        private void Peek4017(ushort address, ref byte data)
        {
            data &= 0xe0;
            data |= Joypad2.GetData(strobe);
        }

        // poke
        private void Poke4014(ushort address, ref byte data)
        {
            dma = true;
            dmaAddr = (ushort)(data << 8);
        }

        private void Poke4015(ushort address, ref byte data)
        {
            dmc.IrqPending = false;
            Irq(0);

            sq1.Enabled = (data & 0x01) != 0;
            sq2.Enabled = (data & 0x02) != 0;
            tri.Enabled = (data & 0x04) != 0;
            noi.Enabled = (data & 0x08) != 0;
            dmc.Enabled = (data & 0x10) != 0;
        }

        private void Poke4016(ushort address, ref byte data)
        {
            strobe = (data & 1);

            if (strobe == 0)
            {
                Joypad1.SetData();
                Joypad2.SetData();
            }
        }

        private void Poke4017(ushort address, ref byte data)
        {
            reg4017.l = data;
            reg4017.h = 1;
        }

        public void MapTo(IBus bus)
        {
            bus.Decode("0100 0000 0000 0000").Poke(sq1.PokeReg1);
            bus.Decode("0100 0000 0000 0001").Poke(sq1.PokeReg2);
            bus.Decode("0100 0000 0000 0010").Poke(sq1.PokeReg3);
            bus.Decode("0100 0000 0000 0011").Poke(sq1.PokeReg4);

            bus.Decode("0100 0000 0000 0100").Poke(sq2.PokeReg1);
            bus.Decode("0100 0000 0000 0101").Poke(sq2.PokeReg2);
            bus.Decode("0100 0000 0000 0110").Poke(sq2.PokeReg3);
            bus.Decode("0100 0000 0000 0111").Poke(sq2.PokeReg4);

            bus.Decode("0100 0000 0000 1000").Poke(tri.PokeReg1);
            bus.Decode("0100 0000 0000 1001").Poke(tri.PokeReg2);
            bus.Decode("0100 0000 0000 1010").Poke(tri.PokeReg3);
            bus.Decode("0100 0000 0000 1011").Poke(tri.PokeReg4);

            bus.Decode("0100 0000 0000 1100").Poke(noi.PokeReg1);
            bus.Decode("0100 0000 0000 1101").Poke(noi.PokeReg2);
            bus.Decode("0100 0000 0000 1110").Poke(noi.PokeReg3);
            bus.Decode("0100 0000 0000 1111").Poke(noi.PokeReg4);

            bus.Decode("0100 0000 0001 0000").Poke(dmc.PokeReg1);
            bus.Decode("0100 0000 0001 0001").Poke(dmc.PokeReg2);
            bus.Decode("0100 0000 0001 0010").Poke(dmc.PokeReg3);
            bus.Decode("0100 0000 0001 0011").Poke(dmc.PokeReg4);

            bus.Decode("0100 0000 0001 0100").Poke(Poke4014);
            bus.Decode("0100 0000 0001 0101").Peek(Peek4015).Poke(Poke4015);
            bus.Decode("0100 0000 0001 0110").Peek(Peek4016).Poke(Poke4016);
            bus.Decode("0100 0000 0001 0111").Peek(Peek4017).Poke(Poke4017);
        }

        public void Consume(VblNmiSignal e)
        {
            Nmi(e.Value);
        }
    }
}
