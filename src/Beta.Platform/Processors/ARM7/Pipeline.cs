namespace Beta.Platform.Processors.ARM7
{
    public sealed class Pipeline
    {
        public Stage execute;
        public Stage decode;
        public Stage fetch;
        public bool refresh;

        public struct Stage
        {
            public uint address;
            public uint data;
        }
    }
}
