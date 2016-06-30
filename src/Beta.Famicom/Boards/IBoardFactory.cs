namespace Beta.Famicom.Boards
{
    public interface IBoardFactory
    {
        Board GetBoard(byte[] binary);
    }
}
