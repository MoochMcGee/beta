namespace Beta.GameBoy.APU
{
    public sealed class ApuRegisters
    {
        public int sequence_timer = 4194304 / 2048;
        public int sequence_step;

        public byte[] speaker_select = new byte[2];
        public byte[] speaker_volume = new byte[2];
    }
}
