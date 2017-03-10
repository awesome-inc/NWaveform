using NAudio.Wave;
using NWaveform.Model;

namespace NWaveform.NAudio
{
    public interface IPeakProvider
    {
        PeakInfo[] Sample(WaveFormat waveFormat, byte[] data);
    }
}