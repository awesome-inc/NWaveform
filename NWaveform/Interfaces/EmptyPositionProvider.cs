using System;
using System.ComponentModel;
using NWaveform.Model;

namespace NWaveform.Interfaces
{
    public class EmptyPositionProvider : IPositionProvider
    {
        public Uri Source { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;
        public double Position { get; set; }
        public double Duration { get; set; }
        public AudioSelection AudioSelection { get; set; }
    }
}