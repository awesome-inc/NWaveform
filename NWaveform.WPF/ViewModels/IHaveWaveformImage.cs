using System.Windows.Media;

namespace NWaveform.ViewModels
{
    public interface IHaveWaveformImage
    {
        ImageBrush WaveformImageBrush { get; }
    }
}