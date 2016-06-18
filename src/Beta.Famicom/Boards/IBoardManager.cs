namespace Beta.Famicom.Boards
{
    public interface IBoardManager
    {
        IBoard GetBoard(GameSystem gameSystem, byte[] cart);
    }
}
