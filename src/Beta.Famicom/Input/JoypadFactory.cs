namespace Beta.Famicom.Input
{
    public sealed class JoypadFactory : IJoypadFactory
    {
        public Joypad Create(int index)
        {
            return new StandardController(index);
        }
    }
}
