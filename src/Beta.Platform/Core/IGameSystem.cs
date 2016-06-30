namespace Beta.Platform.Core
{
    public delegate byte Peek(uint address);

    public delegate void Poke(uint address, byte data);

    public interface IGameSystem
    {
        void Initialize();
    }
}
