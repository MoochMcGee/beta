using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Beta.Platform;
using Beta.Platform.Core;

namespace Beta
{
    public partial class FormHost : Form
    {
        private IGameSystem gameSystem;
        private Thread gameThread;

        public FormHost()
        {
            InitializeComponent();
        }

        public void Start(IEmulationLoop loop)
        {
            gameThread = new Thread(loop.Main);
            gameThread.Start();
        }

        public void Abort()
        {
            gameThread?.Abort();
            gameThread?.Join();
            gameThread = null;
        }

        private void FormHost_Load(object sender, EventArgs e)
        {
            const int scale = 4;

            ClientSize = new Size(
                scale * 256,
                scale * 240);
        }

        public void LoadGame(IGameSystemFactory gameSystemFactory, string fileName)
        {
            gameSystem = gameSystemFactory.Create(File.ReadAllBytes(fileName));
        }
    }
}
