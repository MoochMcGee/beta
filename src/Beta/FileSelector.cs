using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Beta.Hosting;

namespace Beta
{
    public sealed class FileSelector : IFileSelector
    {
        public FileInfo Display(DriverDefinition[] drivers)
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Title = "Open file...";
                openFileDialog.Filter = CreateFilter(drivers);

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    return new FileInfo(openFileDialog.FileName);
                }
            }

            return null;
        }

        private static string CreateFilter(DriverDefinition[] drivers)
        {
            var all = CreateAll(drivers);
            var filters =
                from driver in drivers
                select CreateFilter(driver);

            var list = new List<string>();
            list.Add(all);
            list.AddRange(filters);

            return string.Join("|", list);
        }

        private static string CreateFilter(DriverDefinition e)
        {
            var linq =
                from extension in e.Configuration.Extensions
                select "*" + extension;

            return $"{e.Configuration.Name}|{string.Join(";", linq)}";
        }

        private static string CreateAll(DriverDefinition[] drivers)
        {
            var linq =
                from driver in drivers
                from extension in driver.Configuration.Extensions
                select "*" + extension;

            return $"All Files|{string.Join(";", linq)}";
        }
    }
}
