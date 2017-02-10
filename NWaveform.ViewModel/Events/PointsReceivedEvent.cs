using System;

namespace NWaveform.Events
{
    public class PointsReceivedEvent
    {
        public string Source { get; }
        public int[] LeftPoints { get;  }
        public int[] RightPoints { get; }

        public PointsReceivedEvent(string source, int[] leftPoints, int[] rightPoints = null)
        {
            if (string.IsNullOrWhiteSpace(source)) throw new ArgumentException("Must not be null, empty or whitespace", nameof(source));
            Source = source;
            LeftPoints = leftPoints;
            RightPoints = rightPoints ?? leftPoints;
        }
    }
}