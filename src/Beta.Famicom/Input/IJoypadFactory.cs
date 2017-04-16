namespace Beta.Famicom.Input
{
    public interface IJoypadFactory
    {
        IJoypad Create(int index);
    }
}
