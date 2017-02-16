using System;

namespace NWaveform.Events
{
    public class RefreshWaveformEvent
    {
        public Uri Source { get; }

        public RefreshWaveformEvent(Uri source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            Source = source;
        }
    }
}