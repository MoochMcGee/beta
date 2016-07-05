using Beta.GameBoyAdvance.CPU;

namespace Beta.GameBoyAdvance.Memory
{
    public interface IMemoryMap
    {
        void Initialize(Cpu cpu, GamePak gamePak);

        uint Read(int size, uint address);

        void Write(int size, uint address, uint data);
    }
}
