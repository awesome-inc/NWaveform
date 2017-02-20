namespace NWaveform.ViewModels
{
    internal static class ArrayExtensions
    {
        public static void Set(this int[] channel, int value, int offset = 0)
        {
            for (var i = offset; i < channel.Length; i++) channel[i] = value;
        }

        public static void FlushedCopy(this int[] destPoints, int xOffset, int[] sourcePoints, int zeroValue)
        {
            if (xOffset < 0) return;
            for (var i = 0; i < sourcePoints.Length; i++)
            {
                if (xOffset + i >= destPoints.Length) break;
                destPoints[xOffset + i] = sourcePoints[i];
            }
            // zero tail of points
            Set(destPoints, zeroValue, xOffset + sourcePoints.Length);
        }
    }
}