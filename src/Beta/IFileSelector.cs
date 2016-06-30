using System.IO;
using Beta.Hosting;

namespace Beta
{
    public interface IFileSelector
    {
        FileInfo Display(GameSystemDefinition[] packages);
    }
}
