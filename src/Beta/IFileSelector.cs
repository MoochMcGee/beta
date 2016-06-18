using System.IO;
using Beta.Platform.Packaging;

namespace Beta
{
    public interface IFileSelector
    {
        FileInfo Display(IPackage[] packages);
    }
}
