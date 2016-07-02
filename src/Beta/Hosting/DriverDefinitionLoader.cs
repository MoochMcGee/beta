using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Beta.Platform.Configuration;
using SimpleInjector.Packaging;

namespace Beta.Hosting
{
    public sealed class DriverDefinitionLoader : IDriverDefinitionLoader
    {
        public DriverDefinition[] Load()
        {
            var directory = new DirectoryInfo("drivers");
            var linq =
                from path in directory.GetDirectories("*.sys")
                from file in path.GetFiles("*.dll")
                from type in Assembly.LoadFile(file.FullName).GetExportedTypes()
                where typeof(IPackage) != type
                where typeof(IPackage).IsAssignableFrom(type)
                select new DriverDefinition
                {
                    Package = (IPackage)Activator.CreateInstance(type),
                    Configuration = ConfigurationLoader.Load($@"{path.FullName}\conf.json")
                };

            return linq.ToArray();
        }
    }
}
