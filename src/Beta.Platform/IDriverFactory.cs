namespace Beta.Platform
{
    public interface IDriverFactory
    {
        IDriver Create(byte[] binary);
    }
}
