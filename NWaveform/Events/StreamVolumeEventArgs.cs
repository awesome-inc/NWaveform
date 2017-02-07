namespace NWaveform.Events
{
    public class StreamVolumeEventArgs : AudioEventArgs
    {
        public StreamVolumeEventArgs(float normalizedPosition, float[] values)
        {
            NormalizedPosition = normalizedPosition;
            Values = values;
        }

        public float NormalizedPosition { get; private set; }

        public float[] Values { get; private set; }
    }
}