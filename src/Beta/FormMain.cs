using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Beta.Hosting;
using Beta.Platform;
using Beta.Platform.Configuration;

namespace Beta
{
    public partial class FormMain : Form
    {
        private readonly DriverDefinition[] definitions;
        private readonly IFileSelector fileSelector;

        private Size padding;

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
            this.config = definition.Configuration;

            var scale = Math.Min(
                1280 / config.Video.Width,
                720 / config.Video.Height);

            this.Size = new Size(
                padding.Width + (config.Video.Width * scale),
                padding.Height + (config.Video.Height * scale)
            );

            var container = Bootstrapper.Bootstrap(panelCanvas.Handle);
            container.RegisterSingleton(definition.Configuration);

            definition.Package.RegisterServices(container);

            LoadGame(container.GetInstance<IDriverFactory>(), file.FullName);
        }

        #region FormHost

        private ConfigurationFile config;
        private IDriver driver;
        private Thread driverThread;

        private void EmulationLoop()
        {
            try
            {
                driver.Main();
            }
            catch (ThreadAbortException) { }
        }

        public void LoadGame(IDriverFactory driverFactory, string fileName)
        {
            var binary = File.ReadAllBytes(fileName);

            driver = driverFactory.Create(binary);

            StartEmulation();
        }

        #endregion

        private void playButton_Click(object sender, EventArgs e)
        {
            StartEmulation();
        }

        private void pauseButton_Click(object sender, EventArgs e)
        {
        }

        private void stopButton_Click(object sender, EventArgs e)
        {
            AbortEmulation();
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            this.Text = $"Beta - {GitVersionInformation.SemVer}";

            padding = new Size(
                this.Width - panelCanvas.Width,
                this.Height - panelCanvas.Height
            );
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            AbortEmulation();
        }

        private void StartEmulation()
        {
            if (driverThread != null)
            {
                driverThread.Abort();
                driverThread.Join();
                driverThread = null;
            }

            driverThread = new Thread(driver.Main);
            driverThread.Start();

            playButton.Enabled = false;
            stopButton.Enabled = true;
        }

        private void AbortEmulation()
        {
            if (driverThread != null)
            {
                driverThread.Abort();
                driverThread.Join();
                driverThread = null;
            }

            playButton.Enabled = true;
            stopButton.Enabled = false;
        }
    }
}
