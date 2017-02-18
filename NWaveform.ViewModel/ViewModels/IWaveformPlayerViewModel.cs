using System;
using System.ComponentModel;
using NWaveform.Interfaces;
using NWaveform.Model;

namespace NWaveform.ViewModels
{
    public interface IWaveformPlayerViewModel : INotifyPropertyChanged
    {
        Uri Source { get; set; }
        IMediaPlayer Player { get; }
        IWaveformViewModel Waveform { get; }
        WaveformData GetWaveform();

        bool HasCurrentTime { get; }
        DateTimeOffset? CurrentTime { get; }
    }
}