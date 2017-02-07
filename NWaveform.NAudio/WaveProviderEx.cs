using System;
using NAudio.Wave;

namespace NWaveform.NAudio
{
    internal class WaveProviderEx : WaveChannel32
    {
        public WaveProviderEx(Uri source)
            : base(new AudioStream(source))
        {
        }
   }
}