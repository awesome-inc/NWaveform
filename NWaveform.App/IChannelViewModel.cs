using System;
using System.ComponentModel;
using NWaveform.ViewModels;

namespace NWaveform.App
{
    public interface IChannelViewModel : INotifyPropertyChanged
    {
        string Source { get; }
        TimeSpan Duration { get; }
        IWaveformDisplayViewModel Waveform { get; }
    }
}