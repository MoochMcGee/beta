using Beta.Platform.Exceptions;

namespace Beta.GameBoy.APU
{
    public static class ApuRegisters
    {
        public static byte Read(ApuState e, ushort address)
        {
            if (address >= 0xff10 && address <= 0xff14) { return Sq1Registers.Read(e.sq1, address); }
            if (address >= 0xff15 && address <= 0xff19) { return Sq2Registers.Read(e.sq2, address); }
            if (address >= 0xff1a && address <= 0xff1e) { return WavRegisters.Read(e.wav, address); }
            if (address >= 0xff1f && address <= 0xff23) { return NoiRegisters.Read(e.noi, address); }

            switch (address)
            {
            case 0xff24:
                return (byte)(
                    (e.output_vin_l      << 7) |
                    (e.speaker_volume[0] << 4) |
                    (e.output_vin_r      << 3) |
                    (e.speaker_volume[1] << 0)
                );

            case 0xff25:
                return (byte)(
                    (e.speaker_select[0] << 4) |
                    (e.speaker_select[1] << 0)
                );

            case 0xff26:
                return (byte)(
                    (e.enabled     ? 0x80 : 0) |
                    (e.noi.enabled ? 0x08 : 0) |
                    (e.wav.enabled ? 0x04 : 0) |
                    (e.sq2.enabled ? 0x02 : 0) |
                    (e.sq1.enabled ? 0x01 : 0) | 0x70
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

        public static void Write(ApuState e, ushort address, byte data)
        {
            if (address >= 0xff10 && address <= 0xff14) { Sq1Registers.Write(e.sq1, address, data); return; }
            if (address >= 0xff15 && address <= 0xff19) { Sq2Registers.Write(e.sq2, address, data); return; }
            if (address >= 0xff1a && address <= 0xff1e) { WavRegisters.Write(e.wav, address, data); return; }
            if (address >= 0xff1f && address <= 0xff23) { NoiRegisters.Write(e.noi, address, data); return; }

            switch (address)
            {
            case 0xff24:
                e.output_vin_l      = (data >> 7) & 1;
                e.speaker_volume[0] = (data >> 4) & 7;
                e.output_vin_r      = (data >> 3) & 1;
                e.speaker_volume[1] = (data >> 0) & 7;
                break;

            case 0xff25:
                e.speaker_select[0] = (data >> 4) & 15;
                e.speaker_select[1] = (data >> 0) & 15;
                break;

            case 0xff26:
                e.enabled = (data & 0x80) != 0;
                break;
            }
        }
    }
}
