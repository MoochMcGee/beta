using System.Threading;
using Beta.Famicom.Abstractions;
using Beta.Famicom.Boards;
using Beta.Famicom.CPU;
using Beta.Famicom.Input;
using Beta.Famicom.Messaging;
using Beta.Famicom.PPU;
using Beta.Platform;
using Beta.Platform.Audio;
using Beta.Platform.Core;
using Beta.Platform.Messaging;
using Beta.Platform.Video;

namespace Beta.Famicom
{
    public sealed class GameSystem
        : IGameSystem
        , IConsumer<FrameSignal>
    {
        private byte[] vram = new byte[2048];
        private byte[] wram = new byte[2048];

        public IBoard Board;
        public R2A03 Cpu;
        public R2C02 Ppu;

        public IAudioBackend Audio { get; set; }

        public IVideoBackend Video { get; set; }

        public GameSystem(IBus cpuBus, IBus ppuBus)
        {
            vram.Initialize<byte>(0xff);
            wram.Initialize<byte>(0xff);

            cpuBus.Map("000- ---- ---- ----", reader: PeekWRam, writer: PokeWRam);
            ppuBus.Map("  1- ---- ---- ----", reader: PeekVRam, writer: PokeVRam);
        }

        private void PeekVRam(ushort address, ref byte data)
        {
            data = vram[(address & 0x3ff) | (Board.VRamA10(address) << 10)];
        }

        private void PeekWRam(ushort address, ref byte data)
        {
            data = wram[address & 0x7ff];
        }

        private void PokeVRam(ushort address, ref byte data)
        {
            vram[(address & 0x3ff) | (Board.VRamA10(address) << 10)] = data;
        }

        private void PokeWRam(ushort address, ref byte data)
        {
            wram[address & 0x7ff] = data;
        }

        public void Emulate()
        {
            Initialize();

            Cpu.ResetHard();
            Board.ResetHard();

            try
            {
                while (true)
                {
                    Cpu.Update();
                }
            }
            catch (ThreadAbortException) { }
        }

        public void Initialize()
        {
            Cpu.Initialize();
            Ppu.Initialize();

            Board.Initialize();
        }

        public void Consume(FrameSignal e)
        {
            Joypad.AutofireState = !Joypad.AutofireState;

            Cpu.Joypad1.Update();
            Cpu.Joypad2.Update();
        }
    }
}
