using System;
using NAudio.Wave;

namespace NWaveform.NAudio
{
    public class WaveProviderEx : WaveChannel32, IWaveProviderEx
    {
        public WaveProviderEx(Uri source)
            : this(new AudioStream(source))
        {
        }

        public WaveProviderEx(WaveStream stream) : base(stream)
        {}

        public bool SupportsPanning { get; } = true;
    }
}