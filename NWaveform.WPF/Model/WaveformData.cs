using System;
using System.Collections.Generic;
using System.Linq;

namespace NWaveform.Model
{
    public sealed class WaveformData
    {
        public static readonly WaveformData Empty = new WaveformData(TimeSpan.Zero);

        public TimeSpan Duration { get; }
        public PeakInfo[] Peaks { get; }

        public WaveformData(TimeSpan duration, IEnumerable<PeakInfo> peaks = null)
        {
            Duration = duration;
            Peaks = (peaks ?? Enumerable.Empty<PeakInfo>()).ToArray();
        }

        public static bool IsNullOrEmpty(WaveformData waveformData)
        {
            return waveformData == null
                || waveformData.Duration == TimeSpan.Zero
                || waveformData.Peaks == null || waveformData.Peaks.Length == 0;
        }
    }
}