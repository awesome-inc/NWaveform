using System;
using System.ComponentModel;
using System.Windows.Media;
using Caliburn.Micro;
using NWaveform.Events;

namespace NWaveform.ViewModels
{
    public interface IWaveformDisplayViewModel
        : INotifyPropertyChanged
        , IHandleWithTask<PeaksReceivedEvent>
        , IHandleWithTask<AudioShiftedEvent>
    {
        Uri Source { get; set; }
        double Duration { get; set; }
        SolidColorBrush LeftBrush { get; set; }
        SolidColorBrush RightBrush { get; set; }
        int[] LeftChannel { get; }
        int[] RightChannel { get; }
    }
}