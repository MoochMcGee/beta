using Beta.Famicom.Abstractions;
using Beta.Famicom.Messaging;
using Beta.Platform.Messaging;

namespace Beta.Famicom.Boards
{
    public interface IBoard : IConsumer<ClockSignal>
    {
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
