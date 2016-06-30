using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Beta.Hosting;

namespace Beta
{
    public sealed class FileSelector : IFileSelector
    {
        public FileInfo Display(GameSystemDefinition[] packages)
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

        private static string CreateAll(GameSystemDefinition[] packages)
        {
            var linq =
                from package in packages
                from extension in package.File.Extensions
                select "*" + extension;

            return $"All Files|{string.Join(";", linq)}";
        }

        private static string CreateFilter(GameSystemDefinition e)
        {
            var linq =
                from extension in e.File.Extensions
                select "*" + extension;

            return $"{e.File.Name}|{string.Join(";", linq)}";
        }

        private static string CreateFilter(GameSystemDefinition[] packages)
        {
            var all = CreateAll(packages);
            var filters =
                from package in packages
                select CreateFilter(package);

            var list = new List<string>();
            list.Add(all);
            list.AddRange(filters);

            return string.Join("|", list);
        }
    }
}
