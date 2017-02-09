using System.ComponentModel;
using NWaveform.Model;

namespace NWaveform.Interfaces
{
    public class EmptyPositionProvider : IPositionProvider
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public double Position { get; set; }
        public double Duration { get; set; }
        public AudioSelection AudioSelection { get; set; }
    }
}