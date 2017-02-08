
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

    public class PointsReceivedEvent
    {
        public int[] LeftPoints { get;  }
        public int[] RightPoints { get; }

        public PointsReceivedEvent(int[] leftPoints, int[] rightPoints = null)
        {
            LeftPoints = leftPoints;
            RightPoints = rightPoints ?? leftPoints;
        }
    }

    public static class AudioSamplesExtensions
    {
        public static PointsReceivedEvent ToPoints(this SamplesReceivedEvent e, double duration, double width, double height)
        {
            var sx = width / duration;
            var sy = height / 2.0d;

            var x0 = (int)(sx * e.Start);
            var x1 = (int)(sx * e.End);
            var n = x1 - x0;

            var leftPoints = new int[2*n];
            var rightPoints = new int[2*n];

            var st = e.LeftPeaks.Length / (e.End - e.Start);

            for (var i = 0; i < n; i++)
            {
                var x = x0 + i;
                leftPoints[2 * i]     = rightPoints[2 * i] = x;

                var j = (int)(st * x / sx - e.Start * st);
                var y = (int) (sy * (1 - e.LeftPeaks[j]));

                leftPoints[2 * i + 1] = y;
                //rightPoints[2*i] = x0 + i;
            }

            return new PointsReceivedEvent(leftPoints, rightPoints);
        }
    }
}
