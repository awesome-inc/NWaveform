namespace NWaveform.Model
{
    public class PeakInfo
    {
        public PeakInfo(float min, float max)
        {
            Max = max;
            Min = min;
        }

        public float Min { get; }
        public float Max { get; }
    }
}