using System;
using NWaveform.Model;

namespace NWaveform.Interfaces
{
    public interface IGetWaveform
    {
        WaveformData For(Uri source);
    }
}