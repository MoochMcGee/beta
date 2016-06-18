namespace Beta.Platform.Core
{
    public interface IProcessor
    {
        void Update();

        void Update(int amount);

        void Initialize();
    }
}
