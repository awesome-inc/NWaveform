using System;
using NAudio.Wave;

namespace NWaveform.NAudio
{
    public class BufferedStreamingChannel : IStreamingAudioChannel, IDisposable
    {
        private readonly WaveProviderEx _waveProvider;
        internal readonly BufferedWaveStream BufferedStream;
        private TimeSpan _preserveAfterWrapAround;
        public TimeSpan BufferSize => BufferedStream.BufferDuration;

        public string Name { get; }
        public IWaveProviderEx Stream => _waveProvider;

        public TimeSpan PreserveAfterWrapAround
        {
            get { return _preserveAfterWrapAround; }
            set
            {
                if (value >= BufferSize) throw new ArgumentOutOfRangeException(nameof(value), $"{nameof(PreserveAfterWrapAround)} must be less than {nameof(BufferSize)}");
                _preserveAfterWrapAround = value;
            }
        }

        public BufferedStreamingChannel(string name, WaveFormat waveFormat, TimeSpan bufferSize)
        {
            Name = name;
            BufferedStream = new BufferedWaveStream(waveFormat, bufferSize)
            {
                DiscardOnBufferOverflow = true,
                ReadFully = true,
            };
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