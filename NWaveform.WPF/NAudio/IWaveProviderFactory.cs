using System;

namespace NWaveform.NAudio
{
    public interface IWaveProviderFactory
    {
        IWaveProviderEx Create(Uri source);
    }
}