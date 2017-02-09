namespace NWaveform.Events
{
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
}