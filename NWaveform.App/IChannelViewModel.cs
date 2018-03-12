using System;
using System.ComponentModel;
using NAudio.Wave;
using NWaveform.ViewModels;

namespace NWaveform.App
{
    public interface IChannelViewModel : INotifyPropertyChanged
    {
        string Source { get; }
        TimeSpan Duration { get; }
        IWaveformDisplayViewModel Waveform { get; }

        ISampleProvider MixerInput { get; }
        bool IsPlaying { get; set; }
        double Volume { get; set; }
        double Balance { get; set; }
    }
}
