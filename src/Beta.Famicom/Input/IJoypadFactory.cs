namespace Beta.Famicom.Input
{
    public interface IJoypadFactory
    {
        Joypad Create(int index);
    }
}
