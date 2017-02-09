using NWaveform.NAudio;

namespace NWaveform.App
{
    public class StreamingAudioChannel : IStreamingAudioChannel
    {
        public string Name { get; set;  }
        public IWaveProviderEx Stream { get; set; }
    }
}