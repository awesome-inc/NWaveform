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

        public SamplesReceivedEvent(Uri source, TimeSpan start, WaveFormat waveFormat, byte[] data, int numBytes = 0)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (start < TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(start), "Must not be negative");
            if (waveFormat == null) throw new ArgumentNullException(nameof(waveFormat));
            if (data == null) throw new ArgumentNullException(nameof(data));

            Source = source;
            Start = start;
            WaveFormat = waveFormat;
            var n = numBytes > 0 ? numBytes : data.Length;
            Data = new byte[n];
            Array.Copy(data, Data, n);
        }
    }
}