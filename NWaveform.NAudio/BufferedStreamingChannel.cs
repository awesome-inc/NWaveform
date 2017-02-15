using System;
using NAudio.Wave;

namespace NWaveform.NAudio
{
    public class BufferedStreamingChannel : IStreamingAudioChannel, IDisposable
    {
        private readonly WaveProviderEx _waveProvider;
        internal readonly BufferedWaveStream BufferedStream;

        public string Name { get; }
        public IWaveProviderEx Stream => _waveProvider;

        public TimeSpan PreserveAfterWrapAround { get; set; }
        public TimeSpan WarnBeforeWrapAround { get; set; }

        public BufferedStreamingChannel(string name, WaveFormat waveFormat, TimeSpan bufferSize)
        {
            PreserveAfterWrapAround = TimeSpan.FromSeconds(bufferSize.TotalSeconds / 6);
            WarnBeforeWrapAround = TimeSpan.FromSeconds(30);

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