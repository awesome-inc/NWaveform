using System;

namespace NWaveform.Events
{
    public class PointsReceivedEvent
    {
        public Uri Source { get; }
        public int XOffset { get; }
        public int[] LeftPoints { get;  }
        public int[] RightPoints { get; }

        public PointsReceivedEvent(Uri source, int xOffset, int[] leftPoints, int[] rightPoints = null)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (xOffset < 0) throw new ArgumentOutOfRangeException(nameof(xOffset), "Must not be negative");
            Source = source;
            XOffset = xOffset;
            LeftPoints = leftPoints;
            RightPoints = rightPoints ?? leftPoints;
        }
    }
}