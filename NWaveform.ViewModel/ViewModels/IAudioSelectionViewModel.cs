using System.ComponentModel;

namespace NWaveform.ViewModels
{
    public interface IAudioSelectionViewModel : INotifyPropertyChanged
    {
        double Start { get; set; }
        double End { get; set; }
        double Duration { get; }

        double Top { get; set; }
        double Height { get; set; }
    }
}