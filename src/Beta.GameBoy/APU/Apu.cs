using Beta.Platform;
using Beta.Platform.Audio;
using Beta.Platform.Core;
using Beta.Platform.Messaging;

namespace Beta.GameBoy.APU
{
    // Name Addr 7654 3210 Function
    // -----------------------------------------------------------------
    //        Control/Status
    // NR50 FF24 ALLL BRRR Vin L enable, Left vol, Vin R enable, Right vol
    // NR51 FF25 NW21 NW21 Left enables, Right enables
    // NR52 FF26 P--- NW21 Power control/status, Channel length statuses

    //        Wave Table
    //      FF30 0000 1111 Samples $00 and $01
    //      ....
    //      FF3F 0000 1111 Samples $1E and $1F

    public partial class Apu : Processor, IConsumer<ClockSignal>
    {
        private readonly IAddressSpace addressSpace;
        private readonly IAudioBackend audio;

        private ChannelSq1 sq1;
        private ChannelSq2 sq2;
        private ChannelNoi noi;
        private ChannelWav wav;
        private Timing courseTiming;
        private Timing sampleTiming;
        private byte[] reg;
        private int course;

        public Apu(IAddressSpace addressSpace, IAudioBackend audio)
        {
            this.addressSpace = addressSpace;
            this.audio = audio;
            Single = 4;

            courseTiming.Period = 4194304 / 512;
            courseTiming.Single = 4;

            sampleTiming.Period = DELAY;
            sampleTiming.Single = PHASE;

            sq1 = new ChannelSq1(addressSpace);
            sq2 = new ChannelSq2(addressSpace);
            noi = new ChannelNoi(addressSpace);
            wav = new ChannelWav(addressSpace);
            reg = new byte[3];

            sq1.Initialize(0xff10);
            sq2.Initialize(0xff15);
            wav.Initialize(0xff1a);
            noi.Initialize(0xff1f);

            addressSpace.Map(0xff24, /*   */ ReadNR50, WriteNR50);
            addressSpace.Map(0xff25, /*   */ ReadNR51, WriteNR51);
            addressSpace.Map(0xff26, /*   */ ReadNR52, WriteNR52);
            addressSpace.Map(0xff27, 0xff2f, ReadNull, WriteNull);
            addressSpace.Map(0xff30, 0xff3f, wav.Read, wav.Write);
        }

        private byte ReadNull(ushort address)
        {
            return 0xff;
        }

        private byte ReadNR50(ushort address)
        {
            return reg[0];
        }

        private byte ReadNR51(ushort address)
        {
            return reg[1];
        }

        private byte ReadNR52(ushort address)
        {
            return (byte)(
                (reg[2] & 0x80) |
                (noi.Enabled ? 8 : 0) |
                (wav.Enabled ? 4 : 0) |
                (sq2.Enabled ? 2 : 0) |
                (sq1.Enabled ? 1 : 0) | 0x70);
        }

        private void WriteNull(ushort address, byte data)
        {
        }

        private void WriteNR50(ushort address, byte data)
        {
            if ((reg[2] & 0x80) == 0)
                return;

            reg[0] = data;

            Mixer.Level[0] = (data >> 4) & 0x7;
            Mixer.Level[1] = (data >> 0) & 0x7;
        }

        private void WriteNR51(ushort address, byte data)
        {
            if ((reg[2] & 0x80) == 0)
                return;

            reg[1] = data;

            Mixer.Flags[0] = (data >> 4) & 0xf;
            Mixer.Flags[1] = (data >> 0) & 0xf;
        }

        private void WriteNR52(ushort address, byte data)
        {
            if (((reg[2] ^ data) & 0x80) != 0)
            {
                switch (data & 0x80)
                {
                case 0x00:
                    WriteNR50(0x0000, 0x00);
                    WriteNR51(0x0000, 0x00);

                    sq1.PowerOff();
                    sq2.PowerOff();
                    wav.PowerOff();
                    noi.PowerOff();
                    break;

                case 0x80:
                    sq1.PowerOn();
                    sq2.PowerOn();
                    wav.PowerOn();
                    noi.PowerOn();
                    break;
                }
            }

            reg[2] = data;
        }

        private void Sample()
        {
            var samples = Mixer.MixSamples(
                sq1.Sample(),
                sq2.Sample(),
                wav.Sample(),
                noi.Sample());

            audio.Render(samples[0]);
            audio.Render(samples[1]);
        }

        public void Consume(ClockSignal e)
        {
            Update(e.Cycles);
        }

        public override void Update()
        {
            if (courseTiming.Clock())
            {
                switch (course)
                {
                case 0:
                case 4:
                    sq1.ClockDuration();
                    sq2.ClockDuration();
                    wav.ClockDuration();
                    noi.ClockDuration();
                    break;

                case 2:
                case 6:
                    sq1.ClockDuration();
                    sq2.ClockDuration();
                    wav.ClockDuration();
                    noi.ClockDuration();

                    sq1.ClockSweep();
                    break;

                case 7:
                    sq1.ClockEnvelope();
                    sq2.ClockEnvelope();
                    noi.ClockEnvelope();
                    break;
                }

                course = (course + 1) & 0x7;
            }

            if (sampleTiming.Clock())
            {
                Sample();
            }
        }
    }
}
