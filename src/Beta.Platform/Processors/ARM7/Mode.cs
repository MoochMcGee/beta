namespace Beta.Platform.Processors.ARM7
{
    public static class Mode
    {
        public const uint USR = 0x10;
        public const uint FIQ = 0x11;
        public const uint IRQ = 0x12;
        public const uint SVC = 0x13;
        public const uint ABT = 0x17;
        public const uint UND = 0x1b;
        public const uint SYS = 0x1f;
    }
}
