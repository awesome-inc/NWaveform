
using System;
using NWaveform.Model;

namespace NWaveform.Events
{
    public class PeaksReceivedEvent
    {
        public Uri Source { get; }
        public double Start { get; }
        public double End { get; }
        public PeakInfo[] Peaks { get; }
        public DateTime? AudioSampleTime { get; }

        public PeaksReceivedEvent(Uri source, double start, double end, PeakInfo[] peaks, DateTime? audioSampleTime = null)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (start < 0) throw new ArgumentOutOfRangeException(nameof(start), "Must not be negative");
            if (end <= start) throw new ArgumentOutOfRangeException(nameof(end), $"Must be greater than {nameof(start)}");
            if (peaks == null) throw new ArgumentNullException(nameof(peaks));

            Source = source;
            Start = start;
            End = end;
            Peaks = peaks;
            AudioSampleTime = audioSampleTime;
        }
    }
}