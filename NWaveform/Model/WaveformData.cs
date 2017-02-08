using System;

namespace NWaveform.Model
{
    [Serializable]
    public sealed class Channel
    {
        public float[] Samples = new float[0];
    }

    [Serializable]
    public sealed class WaveformData
    {
        public string Source = string.Empty;
        public string Description = string.Empty;
        public TimeSpan Duration = TimeSpan.Zero;
        public int SampleRate = 0;
        public Channel[] Channels = new Channel[0];

        public static bool IsNullOrEmpty(WaveformData waveformData)
        {
            return waveformData == null
                || waveformData.Duration == TimeSpan.Zero
                || waveformData.Channels == null || waveformData.Channels.Length == 0;
        }
    }
}