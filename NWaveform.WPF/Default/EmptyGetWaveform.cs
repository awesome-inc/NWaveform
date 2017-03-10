using System;
using NWaveform.Interfaces;
using NWaveform.Model;

namespace NWaveform.Default
{
    public class EmptyGetWaveform : IGetWaveform
    {
        public WaveformData For(Uri source) { return WaveformData.Empty; }
    }
}