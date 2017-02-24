using System.Windows.Media;
using NWaveform.ViewModels;

namespace NWaveform.Views
{
    public partial class WaveformDisplayView : IHaveWaveformImage
    {
        public WaveformDisplayView()
        {
            InitializeComponent();
        }

        public ImageBrush WaveformImageBrush => WaveformImage;
    }
}
