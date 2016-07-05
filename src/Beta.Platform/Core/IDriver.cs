namespace Beta.Platform.Core
{
    public delegate byte Reader(uint address);

    public delegate void Writer(uint address, byte data);

    public interface IDriver
    {
        void Main();
    }
}
