using Beta.Famicom.Abstractions;

namespace Beta.Famicom.Boards
{
    public interface IBoard
    {
        void Clock();

        void CpuAddressUpdate(ushort address);

        void PpuAddressUpdate(ushort address);

        void MapToCpu(IBus bus);

        void MapToPpu(IBus bus);

        int VRamA10(ushort address);

        void ResetHard();

        void ResetSoft();

        void Initialize();
    }
}
