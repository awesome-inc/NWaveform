using System;

namespace NWaveform.NAudio
{
    public interface IChannelFactory
    {
        IStreamingAudioChannel Create(Uri source);
    }
}
