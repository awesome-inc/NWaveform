namespace NWaveform.Events
{
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
                leftPoints[2 * i] = rightPoints[2 * i] = x;

                var j = (int)(st * x / sx - e.Start * st);
                var yl = (int) (sy * (1 - e.LeftPeaks[j]));
                var yr = (int) (sy * (1 + e.RightPeaks[j]));

                leftPoints[2 * i + 1] = yl;
                rightPoints[2 * i + 1] = yr;
            }

            return new PointsReceivedEvent(leftPoints, rightPoints);
        }
    }
}