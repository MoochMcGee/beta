using Beta.Platform.Core;

namespace Beta.Famicom
{
    public sealed class PowerButton : IPowerButton
    {
        private readonly GameSystem gameSystem;

        public PowerButton(GameSystem gameSystem)
        {
            this.gameSystem = gameSystem;
        }

        public void Press()
        {
            gameSystem.Cpu.ResetHard();
            gameSystem.Board.ResetHard();
        }
    }
}
