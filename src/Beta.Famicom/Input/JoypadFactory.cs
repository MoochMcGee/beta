namespace Beta.Famicom.Input
{
    public sealed class JoypadFactory : IJoypadFactory
    {
        public IJoypad Create(int index)
        {
            return new StandardController(index);
        }
    }
}
