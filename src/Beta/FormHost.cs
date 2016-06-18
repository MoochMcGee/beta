using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Beta.Platform.Audio;
using Beta.Platform.Core;
using Beta.Platform.Video;

namespace Beta
{
    public partial class FormHost : Form
    {
        private readonly IAudioParameterProvider audioParameters;
        private readonly IVideoParameterProvider videoParameters;

        private IAudioBackend audio;
        private IVideoBackend video;
        private IGameSystem gameSystem;
        private Thread gameThread;

        public FormHost(
            IGameSystem gameSystem,
            IPowerButton powerButton,
            IResetButton resetButton,
            IAudioParameterProvider audioParameters,
            IVideoParameterProvider videoParameters)
        {
            this.gameSystem = gameSystem;
            this.audioParameters = audioParameters;
            this.videoParameters = videoParameters;

            InitializeComponent();
        }

        private void FormHost_Load(object sender, EventArgs e)
        {
            InitializeAudio();
            InitializeVideo();
            StartThread();
        }

        private void FormHost_FormClosing(object sender, FormClosingEventArgs e)
        {
            AbortThread();
            UninitializeAudio();
            UninitializeVideo();
        }

        private void AbortThread()
        {
            gameThread?.Abort();
            gameThread?.Join();
            gameThread = null;
        }

        private void StartThread()
        {
            gameThread = new Thread(gameSystem.Emulate);
            gameThread.Start();
        }

        private void InitializeAudio()
        {
            var parameters = audioParameters.GetValue();

            audio = new AudioBackend(Handle, parameters);
            audio.Initialize();

            gameSystem.Audio = audio;
        }

        private void InitializeVideo()
        {
            var parameters = videoParameters.GetValue();

            video = new VideoBackend(Handle, parameters);
            video.Initialize();

            gameSystem.Video = video;

            SetClientSize(
                parameters.Width,
                parameters.Height);
        }

        private void UninitializeVideo()
        {
            video?.Dispose();
            video = null;
        }

        private void UninitializeAudio()
        {
            audio?.Dispose();
            audio = null;
        }

        private void SetClientSize(int width, int height)
        {
            const int scale = 4;

            ClientSize = new Size(
                scale * width,
                scale * height);
        }

        public void LoadGame(string fileName)
        {
            gameSystem.LoadGame(File.ReadAllBytes(fileName));
        }
    }
}
