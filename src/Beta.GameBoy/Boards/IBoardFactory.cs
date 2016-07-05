namespace Beta.GameBoy.Boards
{
    public interface IBoardFactory
    {
        Board Create(byte[] binary);
    }
}
