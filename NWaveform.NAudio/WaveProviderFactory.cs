﻿using System;

namespace NWaveform.NAudio
{
    public class WaveProviderFactory : IWaveProviderFactory
    {
        public IWaveProviderEx Create(Uri source)
        {
            return new WaveProviderEx(source);
        }
    }
}