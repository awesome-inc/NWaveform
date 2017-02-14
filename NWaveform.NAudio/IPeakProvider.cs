using System;
using NAudio.Wave;
using NWaveform.Model;

namespace NWaveform.NAudio
{
    public interface IPeakProvider
    {
        Func<float[], float> Filter { get; set; }
        PeakInfo[] Sample(WaveFormat waveFormat, byte[] data);
    }
}