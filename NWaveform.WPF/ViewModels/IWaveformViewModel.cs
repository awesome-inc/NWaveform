using System.Collections.Generic;
using System.Windows.Media;
using NWaveform.Interfaces;

namespace NWaveform.ViewModels
{
    public interface IWaveformViewModel : IWaveformDisplayViewModel
    {
        IPositionProvider PositionProvider { get; set; }

        SolidColorBrush BackgroundBrush { get; set; }

        double Position { get; set; }
        SolidColorBrush PositionBrush { get; set; }

        double TicksEach { get; set; }

        IAudioSelectionViewModel Selection { get; set; }
        IMenuViewModel SelectionMenu { get; set; }
        SolidColorBrush SelectionBrush { get; set; }

        PointCollection UserChannel { get; set; }
        SolidColorBrush UserBrush { get; set; }

        IList<ILabelVievModel> Labels { get; set; }
        ILabelVievModel SelectedLabel { get; set; }
        SolidColorBrush UserTextBrush { get; set; }


        PointCollection SeparationLeftChannel { get; set; }
        SolidColorBrush SeparationLeftBrush { get; }

        PointCollection SeparationRightChannel { get; set; }
        SolidColorBrush SeparationRightBrush { get; }
    }
}