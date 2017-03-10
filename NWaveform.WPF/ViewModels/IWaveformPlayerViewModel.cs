using System;
using System.ComponentModel;
using Caliburn.Micro;
using NWaveform.Events;
using NWaveform.Interfaces;

namespace NWaveform.ViewModels
{
    public interface IWaveformPlayerViewModel : INotifyPropertyChanged
        , IHandle<AudioShiftedEvent>
    {
        Uri Source { get; set; }
        IMediaPlayer Player { get; }
        IWaveformViewModel Waveform { get; }

        bool HasCurrentTime { get; }
        string CurrentTime { get; }
    }
}