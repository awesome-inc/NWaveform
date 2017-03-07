using System;

namespace NWaveform.NAudio
{
    public interface IStreamingAudioChannel
    {
        DateTimeOffset? CreationTime { get; }
        Uri Source { get; }
        IWaveProviderEx Stream { get; }
    }
}