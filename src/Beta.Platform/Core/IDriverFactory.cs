namespace Beta.Platform.Core
{
    public interface IDriverFactory
    {
        IDriver Create(byte[] binary);
    }
}
