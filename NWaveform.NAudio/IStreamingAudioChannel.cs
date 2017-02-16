using System;

namespace NWaveform.NAudio
{
    public interface IStreamingAudioChannel
    {
        Uri Source { get; }
        IWaveProviderEx Stream { get; }
    }
}