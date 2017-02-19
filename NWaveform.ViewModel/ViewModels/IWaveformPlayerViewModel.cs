using System;
using System.ComponentModel;
using Caliburn.Micro;
using NWaveform.Events;
using NWaveform.Interfaces;
using NWaveform.Model;

namespace NWaveform.ViewModels
{
    public interface IWaveformPlayerViewModel : INotifyPropertyChanged, IHandle<StartTimeChanged>
    {
        Uri Source { get; set; }
        IMediaPlayer Player { get; }
        IWaveformViewModel Waveform { get; }
        WaveformData GetWaveform();

        bool HasCurrentTime { get; }
        string CurrentTime { get; }
    }
}