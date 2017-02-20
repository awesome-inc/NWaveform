using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Media;
using NWaveform.Interfaces;
using NWaveform.Model;

namespace NWaveform.ViewModels
{
    public interface IWaveformViewModel : INotifyPropertyChanged
    {
        IPositionProvider PositionProvider { get; set; }

        void SetWaveform(WaveformData waveform);

        double Position { get; set; }
        double Duration { get; set; }
        bool HasDuration { get; }
        double TicksEach { get; set; }

        IAudioSelectionViewModel Selection { get; set; }
        IMenuViewModel SelectionMenu { get; set; }

        IList<ILabelVievModel> Labels { get; set; }
        ILabelVievModel SelectedLabel { get; set; }

        SolidColorBrush BackgroundBrush { get; set; }
        SolidColorBrush PositionBrush { get; set; }
        SolidColorBrush SelectionBrush { get; set; }
        SolidColorBrush LeftBrush { get; set; }
        SolidColorBrush RightBrush { get; set; }
        SolidColorBrush UserBrush { get; set; }
        SolidColorBrush SeparationLeftBrush { get; }
        SolidColorBrush SeparationRightBrush { get; }
        SolidColorBrush UserTextBrush { get; set; }

        int[] LeftChannel { get; }
        int[] RightChannel { get; }
        PointCollection UserChannel { get; set; }
        PointCollection SeparationLeftChannel { get; set; }
        PointCollection SeparationRightChannel { get; set; }

    }
}