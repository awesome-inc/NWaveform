using System;

namespace NWaveform.Events
{
    public class AudioShiftedEvent
    {
        public Uri Source { get; }
        public TimeSpan Shift { get; }

        public AudioShiftedEvent(Uri source, TimeSpan shift)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            Source = source;
            Shift = shift;
        }
    }
}