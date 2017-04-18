namespace Beta.Platform
{
    public interface IDriverFactory
    {
        IDriver create(byte[] binary);
    }
}
