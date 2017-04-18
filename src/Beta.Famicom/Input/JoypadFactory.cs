namespace Beta.Famicom.Input
{
    public static class JoypadFactory
    {
        public static IJoypad Create(int index)
        {
            return new StandardController(index);
        }
    }
}
