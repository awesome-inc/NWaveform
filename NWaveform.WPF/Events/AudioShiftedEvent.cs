using System;

namespace NWaveform.Events
{
    public class AudioShiftedEvent
    {
        public Uri Source { get; }
        public TimeSpan Shift { get; }
        public DateTimeOffset? NewStartTime { get; }

        public AudioShiftedEvent(Uri source, TimeSpan shift, DateTimeOffset? newStartTime)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            Source = source;
            Shift = shift;
            NewStartTime = newStartTime;
        }
    }
}