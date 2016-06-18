using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Beta.Hosting;
using Beta.Platform.Packaging;
using SimpleInjector;

namespace Beta
{
    public partial class FormMain : Form
    {
        private readonly IFileSelector fileSelector;
        private readonly IPackage[] packages;

        private FormHost formHost;

        public FormMain(IFileSelector fileSelector, IPackageLoader loader)
        {
            this.fileSelector = fileSelector;
            this.packages = loader.Load();

            InitializeComponent();
        }

        private void OpenFile_Click(object sender, EventArgs e)
        {
            var file = fileSelector.Display(packages);
            if (file == null)
            {
                return;
            }

            var dotIndex = file.Name.LastIndexOf('.') + 1;
            var extension = file.Name.Substring(dotIndex, file.Name.Length - dotIndex);
            var linq = from factory in packages
                       where factory.Extensions.Any(o => o.Extension == extension)
                       select factory;

            var match = linq.SingleOrDefault();
            if (match == null)
            {
                return;
            }

            ShowHostForm(match, file);
        }

        private void ShowHostForm(IPackage package, FileInfo file)
        {
            var container = new Container();
            package.RegisterServices(container);

            formHost = container.GetInstance<FormHost>();
            formHost.Text = package.Name;
            formHost.LoadGame(file.FullName);
            formHost.ShowDialog(this);
            formHost.Close();
            formHost = null;
        }
    }
}
