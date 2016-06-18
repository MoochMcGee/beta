namespace Beta.Platform.Core
{
    public sealed class DefaultResetButton : IResetButton
    {
        private readonly IPowerButton powerButton;

        public DefaultResetButton(IPowerButton powerButton)
        {
            this.powerButton = powerButton;
        }

        public void Press()
        {
            powerButton.Press();
        }
    }
}
