namespace Beta.Famicom.Input
{
    public interface IJoypad
    {
        byte getData(int strobe);

        void setData();

        void update();
    }
}
