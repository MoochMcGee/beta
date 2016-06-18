namespace Beta.Famicom.Formats
{
    public interface ICartridgeFactory
    {
        CartridgeImage Create(byte[] binary);
    }
}
