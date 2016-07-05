using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Beta.Platform.Configuration;
using Beta.Platform.Core;

namespace Beta
{
    public partial class FormHost : Form
    {
        private readonly ConfigurationFile config;

        private IDriver gameSystem;
        private Thread gameThread;

        public FormHost(ConfigurationFile config)
        {
            this.config = config;

            InitializeComponent();
        }

        public void Start()
        {
            gameThread = new Thread(EmulationLoop);
            gameThread.Start();
        }

        public void Abort()
        {
            gameThread?.Abort();
            gameThread?.Join();
            gameThread = null;
        }

        private void EmulationLoop()
        {
            try
            {
                gameSystem.Main();
            }
            catch (ThreadAbortException) { }
        }

        private void FormHost_Load(object sender, EventArgs e)
        {
            const int scale = 4;

            ClientSize = new Size(
                scale * config.Video.Width,
                scale * config.Video.Height);
        }

        public void LoadGame(IDriverFactory factory, string fileName)
        {
            var binary = File.ReadAllBytes(fileName);

            gameSystem = factory.Create(binary);
        }
    }
}
