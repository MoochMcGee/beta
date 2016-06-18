using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Beta.Platform.Packaging;

namespace Beta.Hosting
{
    public sealed class PackageLoader : IPackageLoader
    {
        public IPackage[] Load()
        {
            var directory = new DirectoryInfo(".");
            var linq = from file in directory.GetFiles("*.dll")
                       from type in Assembly.LoadFile(file.FullName).GetExportedTypes()
                       where typeof(IPackage) != type
                       where typeof(IPackage).IsAssignableFrom(type)
                       select (IPackage)Activator.CreateInstance(type);

            return linq.ToArray();
        }
    }
}
