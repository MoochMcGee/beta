using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Beta.Platform.Configuration;
using SimpleInjector.Packaging;

namespace Beta.Hosting
{
    public sealed class PackageLoader : IPackageLoader
    {
        public GameSystemDefinition[] Load()
        {
            var directory = new DirectoryInfo("systems");
            var linq =
                from path in directory.GetDirectories("*.sys")
                from file in path.GetFiles("*.dll")
                from type in Assembly.LoadFile(file.FullName).GetExportedTypes()
                where typeof(IPackage) != type
                where typeof(IPackage).IsAssignableFrom(type)
                select new GameSystemDefinition
                {
                    Package = (IPackage)Activator.CreateInstance(type),
                    File = ConfigurationLoader.Load($@"{path.FullName}\conf.json")
                };

            return linq.ToArray();
        }
    }
}
