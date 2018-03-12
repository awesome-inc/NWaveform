using NAudio.Wave;

namespace NWaveform.App
{
    public interface IChannelMixer
    {
        ISampleProvider SampleProvider { get; }
        void Add(ISampleProvider channel);
        void Remove(ISampleProvider channel);
    }
}
