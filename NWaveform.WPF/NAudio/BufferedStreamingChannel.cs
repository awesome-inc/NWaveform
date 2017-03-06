using System;
using NAudio.Wave;

namespace NWaveform.NAudio
{
    public class BufferedStreamingChannel : IStreamingAudioChannel, IDisposable
    {
        private readonly WaveProviderEx _waveProvider;
        protected internal readonly BufferedWaveStream BufferedStream;

        public Uri Source { get; }
        public IWaveProviderEx Stream => _waveProvider;
        public TimeSpan BufferSize => BufferedStream.BufferDuration;

        public TimeSpan PreserveAfterWrapAround
        {
            get { return BufferedStream.PreserveAfterWrapAround; }
            set { BufferedStream.PreserveAfterWrapAround = value; }
        }

        public BufferedStreamingChannel(Uri source, WaveFormat waveFormat, TimeSpan bufferSize)
        {
            Source = source;
            BufferedStream = new BufferedWaveStream(waveFormat, bufferSize);
            _waveProvider = new WaveProviderEx(BufferedStream) { Closeable = false };
        }

        protected internal void AddSamples(TimeSpan time, byte[] buffer, int numBytes = -1)
        {
            var length = numBytes > 0 ? numBytes : buffer.Length;
            BufferedStream.AddSamples(buffer, 0, length);
        }

        public virtual void Dispose()
        {
            _waveProvider?.ExplicitClose();
            BufferedStream?.Dispose();
        }
    }
}