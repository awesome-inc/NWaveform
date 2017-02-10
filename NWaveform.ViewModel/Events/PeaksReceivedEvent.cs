
using System;
using NWaveform.Model;

namespace NWaveform.Events
{
    public class PeaksReceivedEvent
    {
        public string Source { get; }
        public double Start { get; }
        public double End { get; }
        public PeakInfo[] Peaks { get; }

        public PeaksReceivedEvent(string source, double start, double end, PeakInfo[] peaks)
        {
            if (string.IsNullOrWhiteSpace(source)) throw new ArgumentException("Must not be null, empty or whitespace", nameof(source));
            if (start < 0) throw new ArgumentOutOfRangeException(nameof(start), "Must not be negative");
            if (end <= start) throw new ArgumentOutOfRangeException(nameof(end), $"Must be greater than {nameof(start)}");
            if (peaks == null) throw new ArgumentNullException(nameof(peaks));

            Source = source;
            Start = start;
            End = end;
            Peaks = peaks;
        }
    }
}
