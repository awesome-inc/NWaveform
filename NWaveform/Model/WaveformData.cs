using System;
using System.Linq;

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
        public string Source = String.Empty;
        public string Description = String.Empty;
        public TimeSpan Duration = TimeSpan.Zero;
        public int SampleRate = 0;
        public Channel[] Channels = new Channel[0];

        public float[] ToData()
        {
            switch (Channels.Length)
            {
                case 1: // mono
                    return (float[])Channels[0].Samples.Clone();
                case 2: // stereo, interleave data
                    return
                        Channels[0].Samples.Zip(Channels[1].Samples, (f, s) => new[] { f, s })
                                   .SelectMany(f => f)
                                   .ToArray();
                default: // unsupported by now
                    throw new NotSupportedException("Only mono/stereo files supported by now.");
            }
        }

        public static bool IsNullOrEmpty(WaveformData waveformData)
        {
            return waveformData == null
                || waveformData.Duration == TimeSpan.Zero
                || waveformData.Channels == null || waveformData.Channels.Length == 0;
        }
    }
}