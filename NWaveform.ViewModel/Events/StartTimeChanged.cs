using System;

namespace NWaveform.Events
{
    public class StartTimeChanged
    {
        public Uri Source { get; }
        public DateTimeOffset? StartTime { get; }

        public StartTimeChanged(Uri source, DateTimeOffset? startTime)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            Source = source;
            StartTime = startTime;
        }
    }
}