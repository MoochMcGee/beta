using System;
using System.Windows.Forms;
using Beta.Hosting;
using SimpleInjector;

namespace Beta
{
    public static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var container = new Container();
            container.RegisterSingleton<IFileSelector, FileSelector>();

            container.RegisterSingleton<IPackageLoader, PackageLoader>();

            Application.Run(container.GetInstance<FormMain>());
        }
    }
}
