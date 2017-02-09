using NWaveform.NAudio;

namespace NWaveform.App
{
    public interface IStreamingAudioChannel
    {
        string Name { get; }
        IWaveProviderEx Stream { get; }
    }
}