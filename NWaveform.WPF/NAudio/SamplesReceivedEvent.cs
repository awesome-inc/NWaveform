using System;
using NAudio.Wave;

namespace NWaveform.NAudio
{
    public class SamplesReceivedEvent
    {
        public Uri Source { get; }
        public TimeSpan Start { get; }
        public WaveFormat WaveFormat { get; }
        public byte[] Data { get; }

        public SamplesReceivedEvent(Uri source, TimeSpan start, WaveFormat waveFormat, byte[] data, int offset = 0, int count = 0)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (start < TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(start), "Must not be negative");
            if (waveFormat == null) throw new ArgumentNullException(nameof(waveFormat));
            if (data == null) throw new ArgumentNullException(nameof(data));

            Source = source;
            Start = start;
            WaveFormat = waveFormat;
            var n = count > 0 ? count : data.Length - offset;
            Data = new byte[n];
            Buffer.BlockCopy(data, offset, Data, 0, n);
        }
    }
}