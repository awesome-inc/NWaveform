namespace NWaveform.NAudio
{
    public interface IStreamingAudioChannel
    {
        string Name { get; }
        IWaveProviderEx Stream { get; }
    }
}