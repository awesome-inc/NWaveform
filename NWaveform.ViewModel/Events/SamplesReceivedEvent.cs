
using System;

namespace NWaveform.Events
{
    public class SamplesReceivedEvent
    {
        public double Start { get; }
        public double End { get; }
        public float[] LeftPeaks { get; }
        public float[] RightPeaks { get; }

        public SamplesReceivedEvent(double start, double end, float[] leftPeaks, float[] rightPeaks = null)
        {
            if (start < 0) throw new ArgumentOutOfRangeException(nameof(start), "Must not be negative");
            if (end <= start) throw new ArgumentOutOfRangeException(nameof(end), $"Must be greater than {nameof(start)}");
            if (leftPeaks == null) throw new ArgumentNullException(nameof(leftPeaks));

            Start = start;
            End = end;

            LeftPeaks = leftPeaks;
            RightPeaks = rightPeaks ?? leftPeaks;
        }
    }
}
