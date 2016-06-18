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
        private readonly IGameSystemFactory gameSystemFactory;
        private readonly IAudioParameterProvider audioParameterProvider;
        private readonly IVideoParameterProvider videoParameterProvider;

        private IAudioBackend audio;
        private IVideoBackend video;
        private IGameSystem gameSystem;
        private Thread gameThread;

        public FormHost(
            IGameSystemFactory gameSystemFactory,
            IAudioParameterProvider audioParameterProvider,
            IVideoParameterProvider videoParameterProvider)
        {
            this.gameSystemFactory = gameSystemFactory;
            this.audioParameterProvider = audioParameterProvider;
            this.videoParameterProvider = videoParameterProvider;

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
            var parameters = audioParameterProvider.GetValue();

            audio = new AudioBackend(Handle, parameters);
            audio.Initialize();

            gameSystem.Audio = audio;
        }

        private void InitializeVideo()
        {
            var parameters = videoParameterProvider.GetValue();

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
            gameSystem = gameSystemFactory.Create(File.ReadAllBytes(fileName));
        }
    }
}
