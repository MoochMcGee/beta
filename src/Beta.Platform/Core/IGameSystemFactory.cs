namespace Beta.Platform.Core
{
    public interface IGameSystemFactory
    {
        IGameSystem Create(byte[] binary);
    }
}
