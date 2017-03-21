using NAudio.Wave;

namespace NWaveform.NAudio
{
    public interface IWavePlayerFactory
    {
        IWavePlayer Create();
    }
}