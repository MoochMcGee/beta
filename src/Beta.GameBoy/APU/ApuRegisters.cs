using Beta.Platform.Exceptions;

namespace Beta.GameBoy.APU
{
    public sealed class ApuRegisters
    {
        private readonly ApuState apu;
        private readonly Sq1Registers sq1;
        private readonly Sq2Registers sq2;
        private readonly WavRegisters wav;
        private readonly NoiRegisters noi;

        public ApuRegisters(State state)
        {
            this.apu = state.apu;
            this.sq1 = new Sq1Registers(state);
            this.sq2 = new Sq2Registers(state);
            this.wav = new WavRegisters(state);
            this.noi = new NoiRegisters(state);
        }

        public byte Read(ushort address)
        {
            if (address >= 0xff10 && address <= 0xff14) { return sq1.Read(address); }
            if (address >= 0xff15 && address <= 0xff19) { return sq2.Read(address); }
            if (address >= 0xff1a && address <= 0xff1e) { return wav.Read(address); }
            if (address >= 0xff1f && address <= 0xff23) { return noi.Read(address); }

            switch (address)
            {
            case 0xff24:
                return (byte)(
                    (apu.output_vin_l      << 7) |
                    (apu.speaker_volume[0] << 4) |
                    (apu.output_vin_r      << 3) |
                    (apu.speaker_volume[1] << 0)
                );

            case 0xff25:
                return (byte)(
                    (apu.speaker_select[0] << 4) |
                    (apu.speaker_select[1] << 0)
                );

            case 0xff26:
                return (byte)(
                    (apu.enabled     ? 0x80 : 0) |
                    (apu.noi.enabled ? 0x08 : 0) |
                    (apu.wav.enabled ? 0x04 : 0) |
                    (apu.sq2.enabled ? 0x02 : 0) |
                    (apu.sq1.enabled ? 0x01 : 0) | 0x70
                );

            case 0xff27: return 0xff;
            case 0xff28: return 0xff;
            case 0xff29: return 0xff;
            case 0xff2a: return 0xff;
            case 0xff2b: return 0xff;
            case 0xff2c: return 0xff;
            case 0xff2d: return 0xff;
            case 0xff2e: return 0xff;
            case 0xff2f: return 0xff;
            }

            throw new CompilerPleasingException();
        }

        public void Write(ushort address, byte data)
        {
            if (address >= 0xff10 && address <= 0xff14) { sq1.Write(address, data); }
            if (address >= 0xff15 && address <= 0xff19) { sq2.Write(address, data); }
            if (address >= 0xff1a && address <= 0xff1e) { wav.Write(address, data); }
            if (address >= 0xff1f && address <= 0xff23) { noi.Write(address, data); }

            switch (address)
            {
            case 0xff24:
                apu.output_vin_l      = (data >> 7) & 1;
                apu.speaker_volume[0] = (data >> 4) & 7;
                apu.output_vin_r      = (data >> 3) & 1;
                apu.speaker_volume[1] = (data >> 0) & 7;
                break;

            case 0xff25:
                apu.speaker_select[0] = (data >> 4) & 15;
                apu.speaker_select[1] = (data >> 0) & 15;
                break;

            case 0xff26:
                apu.enabled = (data & 0x80) != 0;
                break;
            }
        }
    }
}
