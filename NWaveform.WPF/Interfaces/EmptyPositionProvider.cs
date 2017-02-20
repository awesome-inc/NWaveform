using System;
using Caliburn.Micro;
using NWaveform.Model;

namespace NWaveform.Interfaces
{
    public class EmptyPositionProvider : PropertyChangedBase, IPositionProvider
    {
        public Uri Source { get; set; }
        public double Position { get; set; }
        public double Duration { get; set; }
        public AudioSelection AudioSelection { get; set; }
    }
}