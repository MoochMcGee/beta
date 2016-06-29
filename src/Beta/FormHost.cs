using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
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

        private void FormHost_Load(object sender, EventArgs e)
        {
            SetClientSize(256, 240);

            gameThread = new Thread(gameSystem.Emulate);
            gameThread.Start();
        }

        private void FormHost_FormClosing(object sender, FormClosingEventArgs e)
        {
            gameThread?.Abort();
            gameThread?.Join();
            gameThread = null;
        }
        
        private void SetClientSize(int width, int height)
        {
            const int scale = 4;

            ClientSize = new Size(
                scale * width,
                scale * height);
        }

        public void LoadGame(IGameSystemFactory gameSystemFactory, string fileName)
        {
            gameSystem = gameSystemFactory.Create(File.ReadAllBytes(fileName));
        }
    }
}
