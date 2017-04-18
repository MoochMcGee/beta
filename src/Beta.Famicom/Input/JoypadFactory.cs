namespace Beta.Famicom.Input
{
    public static class JoypadFactory
    {
        public static IJoypad create(int index)
        {
            return new StandardController(index);
        }
    }
}
