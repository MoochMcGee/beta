using System.Collections.Generic;
using SimpleInjector;

namespace Beta.Platform.Packaging
{
    public interface IPackage
    {
        string Name { get; }

        IEnumerable<FileExtension> Extensions { get; }

        void RegisterServices(Container container);
    }
}
