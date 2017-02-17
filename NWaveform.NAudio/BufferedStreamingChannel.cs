using System;
using NAudio.Wave;

namespace NWaveform.NAudio
{
    public class BufferedStreamingChannel : IStreamingAudioChannel, IDisposable
    {
        private readonly WaveProviderEx _waveProvider;
        protected internal readonly BufferedWaveStream BufferedStream;
        private TimeSpan _preserveAfterWrapAround;
        private byte[] _preservedBuffer;

        public Uri Source { get; }
        public IWaveProviderEx Stream => _waveProvider;
        public TimeSpan BufferSize => BufferedStream.BufferDuration;
        public event EventHandler WrappedAround;

        public TimeSpan PreserveAfterWrapAround
        {
            get { return _preserveAfterWrapAround; }
            set
            {
                if (value >= BufferSize) throw new ArgumentOutOfRangeException(nameof(value), $"{nameof(PreserveAfterWrapAround)} must be less than {nameof(BufferSize)}");
                _preserveAfterWrapAround = value;
                var preservedBytes = (int)(value.TotalSeconds * BufferedStream.WaveFormat.AverageBytesPerSecond);
                _preservedBuffer = preservedBytes > 0 ? new byte[preservedBytes] : null;
            }
        }

        public BufferedStreamingChannel(Uri source, WaveFormat waveFormat, TimeSpan bufferSize)
        {
            Source = source;
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

            var exceededLen = (int)(BufferedStream.WritePosition + length - BufferedStream.Length);
            if (exceededLen < 0)
            {
                BufferedStream.AddSamples(buffer, 0, length);
            }
            else
            {
                var pos = BufferedStream.Position;
                var skippedBytes = BufferedStream.Length;

                if (_preservedBuffer != null)
                {
                    skippedBytes -= _preservedBuffer.Length;
                    var n = _preservedBuffer.Length - length;
                    BufferedStream.Position = BufferedStream.BufferLength - n;
                    BufferedStream.Read(_preservedBuffer, 0, n);
                    BufferedStream.ClearBuffer();
                    BufferedStream.AddSamples(_preservedBuffer, 0, n);
                }

                BufferedStream.AddSamples(buffer, 0, length);

                // preserve position
                BufferedStream.Position = pos - skippedBytes;
                Stream.CurrentTime = BufferedStream.CurrentTime;

                OnWrappedAround();
            }
        }

        protected virtual void OnWrappedAround()
        {
            WrappedAround?.Invoke(this, EventArgs.Empty);
        }

        public virtual void Dispose()
        {
            _waveProvider?.ExplicitClose();
            BufferedStream?.Dispose();
        }
    }
}