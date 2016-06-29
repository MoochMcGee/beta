namespace Beta.Famicom.Boards
{
    public interface IBoardFactory
    {
        Board GetBoard(GameSystem gameSystem, byte[] cart);
    }
}
