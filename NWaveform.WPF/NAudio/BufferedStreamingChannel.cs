using System;
using System.Diagnostics;
using Caliburn.Micro;
using NAudio.Wave;
using NWaveform.Events;

namespace NWaveform.NAudio
{
    public class BufferedStreamingChannel : IStreamingAudioChannel, IDisposable
    {
        private readonly WaveProviderEx _waveProvider;
        protected internal readonly BufferedWaveStream BufferedStream;
        private readonly IEventAggregator _events;

        public DateTimeOffset? StartTime { get; private set; } = DateTimeOffset.UtcNow;
        public Uri Source { get; }
        public IWaveProviderEx Stream => _waveProvider;

        public TimeSpan PreserveAfterWrapAround
        {
            get { return BufferedStream.PreserveAfterWrapAround; }
            set { BufferedStream.PreserveAfterWrapAround = value; }
        }

        public BufferedStreamingChannel(IEventAggregator events, Uri source, WaveFormat waveFormat, TimeSpan bufferSize)
        {
            if (events == null) throw new ArgumentNullException(nameof(events));
            if (source == null) throw new ArgumentNullException(nameof(source));
            _events = events;
            Source = source;
            BufferedStream = new BufferedWaveStream(waveFormat, bufferSize);
            BufferedStream.WrappedAround += BufferedStream_WrappedAround;
            _waveProvider = new WaveProviderEx(BufferedStream) { Closeable = false };
        }

        public int AddSamples(byte[] buffer, int offset = 0, int length = 0)
        {
            var pos = BufferedStream.WritePosition;
            var count = length > 0 ? length : buffer.Length - offset;

            var exceeding = (int)(pos + count - BufferedStream.Length);
            if (exceeding <= 0)
            {
                var time = BufferedStream.CurrentWriteTime;
                SafePublish(new SamplesReceivedEvent(Source, time, BufferedStream.WaveFormat, buffer, offset, count), "Could not add samples");
                return BufferedStream.AddSamples(buffer, offset, count);
            }

            // split in two writes: a) until full, b) exceeding
            var delta = count - exceeding;
            var written = AddSamples(buffer, offset, delta);
            written += AddSamples(buffer, offset + delta, exceeding);
            return written;
        }

        public virtual void Dispose()
        {
            _waveProvider?.ExplicitClose();
            BufferedStream?.Dispose();
        }

        private void BufferedStream_WrappedAround(object sender, EventArgs e)
        {
            StartTime += BufferedStream.SkippedDuration;
            SafePublish(new AudioShiftedEvent(Source, BufferedStream.SkippedDuration), "Could not wrap around buffer");
        }

        private void SafePublish(object message, string couldNot)
        {
            try { _events.PublishOnCurrentThread(message); }
            catch (Exception ex) { Trace.TraceWarning($"{couldNot}: {ex}"); }
        }
    }
}