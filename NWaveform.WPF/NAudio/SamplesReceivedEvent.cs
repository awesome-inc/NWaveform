using System;
using NAudio.Wave;

namespace NWaveform.NAudio
{
    public class SamplesReceivedEvent
    {
        public Uri Source { get; }
        public TimeSpan Start { get; }
        public WaveFormat WaveFormat { get; }
        public DateTime? CurrentAudioTime { get; }
        public byte[] Data { get; }

        public SamplesReceivedEvent(Uri source, TimeSpan start, WaveFormat waveFormat, byte[] data, int offset = 0, int count = 0, DateTime? currentAudioTime = null)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (start < TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(start), "Must not be negative");
            if (data == null) throw new ArgumentNullException(nameof(data));

            Source = source;
            Start = start;
            WaveFormat = waveFormat ?? throw new ArgumentNullException(nameof(waveFormat));
            CurrentAudioTime = currentAudioTime;
            var n = count > 0 ? count : data.Length - offset;
            Data = new byte[n];
            Buffer.BlockCopy(data, offset, Data, 0, n);
        }
    }
}
