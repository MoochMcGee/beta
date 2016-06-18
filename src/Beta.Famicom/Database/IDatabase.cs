namespace Beta.Famicom.Database
{
    public interface IDatabase
    {
        Board Find(byte[] data);
    }
}
