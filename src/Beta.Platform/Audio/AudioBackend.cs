using System;
using System.Threading;
using Beta.Platform.Configuration;
using SharpDX;
using SharpDX.Multimedia;
using SharpDX.XAudio2;

namespace Beta.Platform.Audio
{
    public sealed class AudioBackend : IAudioBackend
    {
        private AudioBuffer buffer;
        private MasteringVoice master;
        private SourceVoice source;
        private XAudio2 engine;
        private int length;

        public AudioBackend(HwndProvider hwndProvider, ConfigurationFile config)
        {
            engine = new XAudio2();
            engine.StartEngine();

            var format = new WaveFormat(config.Audio.SampleRate, config.Audio.Channels);

            length = format.ConvertLatencyToByteSize(16);

            master = new MasteringVoice(engine, format.Channels, format.SampleRate);
            source = new SourceVoice(engine, format);
            source.Start();

            buffer = CreateAudioBuffer();
        }

        ~AudioBackend()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            source.Stop();
            engine.StopEngine();

            master.Dispose();
            master = null;

            source.Dispose();
            source = null;

            engine.Dispose();
            engine = null;
        }

        public void Render(int sample)
        {
            buffer.Stream.WriteByte((byte)(sample >> 0));
            buffer.Stream.WriteByte((byte)(sample >> 8));

            if (buffer.Stream.Position == length)
            {
                Render();
            }
        }

        public void Render()
        {
            source.SubmitSourceBuffer(buffer, null);

            buffer = CreateAudioBuffer();

            while (source.State.BuffersQueued > 2)
            {
                Thread.Sleep(1);
            }
        }

        private AudioBuffer CreateAudioBuffer()
        {
            return new AudioBuffer
            {
                AudioBytes = length,
                Stream = new DataStream(length, true, true)
            };
        }
    }
}
