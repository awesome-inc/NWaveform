using System.Collections.Generic;
using System.Windows.Media;
using NWaveform.Interfaces;
using NWaveform.Model;

namespace NWaveform.ViewModels
{
    public interface IWaveformViewModel : IWaveformDisplayViewModel
    {
        IPositionProvider PositionProvider { get; set; }

        void SetWaveform(PeakInfo[] peaks, double duration);

        double Position { get; set; }
        double TicksEach { get; set; }

        IAudioSelectionViewModel Selection { get; set; }
        IMenuViewModel SelectionMenu { get; set; }

        IList<ILabelVievModel> Labels { get; set; }
        ILabelVievModel SelectedLabel { get; set; }

        SolidColorBrush BackgroundBrush { get; set; }
        SolidColorBrush PositionBrush { get; set; }
        SolidColorBrush SelectionBrush { get; set; }
        SolidColorBrush UserBrush { get; set; }
        SolidColorBrush SeparationLeftBrush { get; }
        SolidColorBrush SeparationRightBrush { get; }
        SolidColorBrush UserTextBrush { get; set; }

        PointCollection UserChannel { get; set; }
        PointCollection SeparationLeftChannel { get; set; }
        PointCollection SeparationRightChannel { get; set; }
    }
}