using System;
using NWaveform.Model;

namespace NWaveform.Interfaces
{
    public interface IWaveFormRepository
    {
        WaveformData For(Uri uri, Action<Progress> onProgress = null);
    }
}