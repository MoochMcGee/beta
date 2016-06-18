using Beta.Platform.Core;

namespace Beta.Famicom
{
    public sealed class ResetButton : IResetButton
    {
        private readonly GameSystem gameSystem;

        public ResetButton(GameSystem gameSystem)
        {
            this.gameSystem = gameSystem;
        }

        public void Press()
        {
            gameSystem.Cpu.ResetSoft();
            gameSystem.Board.ResetSoft();
        }
    }
}
