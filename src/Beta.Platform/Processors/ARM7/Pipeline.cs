namespace Beta.Platform.Processors.ARM7
{
    public struct Pipeline
    {
        public uint execute;
        public uint decode;
        public uint fetch;
        public bool refresh;
    }
}
