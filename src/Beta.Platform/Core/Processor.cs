namespace Beta.Platform.Core
{
    public abstract class Processor
    {
        public int Cycles;
        public int Single;

        public virtual void Update()
        {
        }

        public virtual void Update(int cycles)
        {
            for (Cycles += cycles; Cycles >= Single; Cycles -= Single)
            {
                Update();
            }
        }
    }
}
