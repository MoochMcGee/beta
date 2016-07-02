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
            var container = new Container();
            container.RegisterSingleton<IFileSelector, FileSelector>();
            container.RegisterSingleton<IDriverDefinitionLoader, DriverDefinitionLoader>();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(container.GetInstance<FormMain>());
        }
    }
}
