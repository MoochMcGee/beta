using Beta.Platform.Packaging;

namespace Beta.Hosting
{
    public interface IPackageLoader
    {
        IPackage[] Load();
    }
}
