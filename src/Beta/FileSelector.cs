using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Beta.Platform.Packaging;

namespace Beta
{
    public sealed class FileSelector : IFileSelector
    {
        public FileInfo Display(IPackage[] packages)
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Title = "Open file...";
                openFileDialog.Filter = CreateFilter(packages);

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    return new FileInfo(openFileDialog.FileName);
                }
            }

            return null;
        }

        private static string CreateAll(IPackage[] packages)
        {
            var extensions = packages.SelectMany(e => e.Extensions);

            return $"All Files|{string.Join(";", extensions)}";
        }

        private static string CreateFilter(IPackage package)
        {
            var name = package.Name;
            var extensions = string.Join(";", package.Extensions);

            return $"{name}|{extensions}";
        }

        private static string CreateFilter(IPackage[] packages)
        {
            var all = CreateAll(packages);
            var filters = from factory in packages
                          select CreateFilter(factory);

            var list = new List<string>();
            list.Add(all);
            list.AddRange(filters);

            return string.Join("|", list);
        }
    }
}
