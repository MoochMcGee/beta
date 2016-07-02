using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Beta.Hosting;
using Beta.Platform;
using Beta.Platform.Core;

namespace Beta
{
    public partial class FormMain : Form
    {
        private readonly DriverDefinition[] definitions;
        private readonly IFileSelector fileSelector;

        private FormHost formHost;

        public FormMain(IFileSelector fileSelector, IDriverDefinitionLoader loader)
        {
            this.fileSelector = fileSelector;
            this.definitions = loader.Load();

            InitializeComponent();
        }

        private void OpenFile_Click(object sender, EventArgs e)
        {
            var file = fileSelector.Display(definitions);
            if (file == null)
            {
                return;
            }

            var dotIndex = file.Name.LastIndexOf('.');
            var extension = file.Name.Substring(dotIndex, file.Name.Length - dotIndex);
            var linq =
                from d in definitions
                where d.Configuration.Extensions.Any(o => o == extension)
                select d;

            var definition = linq.SingleOrDefault();
            if (definition == null)
            {
                return;
            }

            ShowHostForm(definition, file);
        }

        private void ShowHostForm(DriverDefinition definition, FileInfo file)
        {
            formHost = new FormHost(definition.Configuration);

            var container = Bootstrapper.Bootstrap(formHost.Handle);
            container.RegisterSingleton(definition.Configuration);

            definition.Package.RegisterServices(container);

            formHost.Text = $"{definition.Configuration.Name} - {file.Name}";
            formHost.LoadGame(container.GetInstance<IDriverFactory>(), file.FullName);

            formHost.Start();
            formHost.ShowDialog(this);
            formHost.Abort();

            formHost.Close();
            formHost = null;
        }
    }
}
