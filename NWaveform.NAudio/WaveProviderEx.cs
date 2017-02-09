using System;
using NAudio.Wave;

namespace NWaveform.NAudio
{
    internal class WaveProviderEx : WaveChannel32, IWaveProviderEx
    {
        public WaveProviderEx(Uri source)
            : base(new AudioStream(source))
        {
        }

        public bool SupportsPanning { get; } = true;
    }
}