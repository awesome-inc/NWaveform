using System;
using NAudio.Wave;

namespace NWaveform.App
{
    public interface IMixerChannel : ISampleProvider
    {
        bool IsPlaying { get; set; }
        double Volume { get; set; }
        double Balance { get; set; }
        Uri Source { get; set; } 
    }
}
