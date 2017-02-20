using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using FontAwesome.Sharp;

namespace NWaveform.ViewModels
{
    public interface ILabelVievModel : INotifyPropertyChanged
    {
        string Text { get; set; }
        string Tooltip { get; set; }
        double Position { get; set; }
        double Magnitude { get; set; }
        IAudioSelectionViewModel Selection { get; set; }

        IconChar Icon { get; set; }
        Brush Background { get; set; }
        Brush Foreground { get; set; }
        int FontSize { get; set; }

        IMenuViewModel Menu { get; set; }
        FontWeight FontWeight { get; set; }
    }
}
