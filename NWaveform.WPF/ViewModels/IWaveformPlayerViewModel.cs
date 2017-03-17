using System;
using Caliburn.Micro;
using NWaveform.Events;
using NWaveform.Interfaces;

namespace NWaveform.ViewModels
{
    public interface IWaveformPlayerViewModel : IScreen
        , IHandle<AudioShiftedEvent>
    {
        Uri Source { get; set; }
        IMediaPlayer Player { get; }
        IWaveformViewModel Waveform { get; }

        DateTimeOffset? StartTime { get; }
        bool HasCurrentTime { get; }
        string CurrentTime { get; }
    }
}