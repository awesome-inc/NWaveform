using System;

namespace NWaveform.NAudio
{
    public interface IStreamingAudioChannel
    {
        DateTimeOffset? StartTime { get; }
        Uri Source { get; }
        IWaveProviderEx Stream { get; }
    }
}