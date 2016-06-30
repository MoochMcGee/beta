using System;
using System.IO;
using Beta.Platform.Configuration;
using SharpDX;
using SharpDX.Multimedia;
using SharpDX.XAudio2;

namespace Beta.Platform.Audio
{
    public sealed class AudioBackend : IAudioBackend
    {
        private const int STREAM_COUNT = 4;
        private const int STREAM_MASK = STREAM_COUNT - 1;

        private DataStream[] streams;
        private int length;

        private AudioBuffer buffer;
        private MasteringVoice master;
        private SourceVoice source;
        private XAudio2 engine;
        private int streamIndex;

        public AudioBackend(IHwndProvider hwndProvider, ConfigurationFile config)
        {
            engine = new XAudio2();
            engine.StartEngine();

            var format = new WaveFormat(config.Audio.SampleRate, 16, config.Audio.Channels);

            length = format.AverageBytesPerSecond / 30; // 33.3~ msec

            master = new MasteringVoice(engine, format.Channels, format.SampleRate);
            source = new SourceVoice(engine, format, VoiceFlags.None, 1f);
            source.Start();

            streams = new DataStream[STREAM_COUNT];

            for (var i = 0; i < STREAM_COUNT; i++)
            {
                streams[i] = new DataStream(length, true, true);
            }

            buffer = new AudioBuffer
            {
                AudioBytes = length,
                Stream = streams[streamIndex++ & STREAM_MASK]
            };
        }

        ~AudioBackend()
        {
            Dispose(false);
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

        private void Render()
        {
            buffer.Stream.Seek(0L, SeekOrigin.Begin);

            source.SubmitSourceBuffer(buffer, null);

            buffer.Stream = streams[streamIndex++ & STREAM_MASK];
            buffer.Stream.Seek(0L, SeekOrigin.Begin);

            while (source.State.BuffersQueued > 1)
            {
            }
        }

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        public void Initialize()
        {
        }

        public void Render(int sample)
        {
            buffer.Stream.Write(BitConverter.GetBytes((short)sample), 0, 2);

            if (buffer.Stream.Position == length)
            {
                Render();
            }
        }
    }
}
